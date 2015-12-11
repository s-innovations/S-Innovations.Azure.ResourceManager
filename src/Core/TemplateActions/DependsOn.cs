using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SInnovations.Azure.ResourceManager.TemplateActions
{
    public class DependsOnRef : ITemplateAction
    {
        DependsOn dependsOn;
        internal JObject obj;
        public DependsOnRef(DependsOn dependsOn)
        {
            this.dependsOn = dependsOn;
        }

        
        public void TemplateAction(JObject obj)
        {
            dependsOn.counter++;
            this.obj = obj;

            if (dependsOn.counter >= 2)
                dependsOn.AddToDependOn();

        }




    }
   
    public class DependsOn : ITemplateAction
    {
        private DependsOnRef dependsOnRef;
        private JObject obj;
        internal int counter = 0;
        public DependsOn(ResourceSource other)
        {
            other.Add(dependsOnRef = new DependsOnRef(this));
        }

        public void TemplateAction(JObject obj)
        {
            counter++;
            this.obj = obj;

            if (counter >= 2)
                AddToDependOn();
           

             


        }

        internal void AddToDependOn()
        {
            var token = obj["dependOn"] as JArray;
            if (token == null)
                obj["dependOn"] = token = new JArray();

            token.Add(dependsOnRef.obj.SelectToken("name"));

        }
    }
}
