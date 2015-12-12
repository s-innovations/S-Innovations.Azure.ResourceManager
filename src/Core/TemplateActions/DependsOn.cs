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

        
        public async Task TemplateActionAsync(JObject obj)
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
        private string deploymentName;
        private string outputName;
        private string resourceGroup;
        private ApplicationCredentials options;

        internal int counter = 0;
        public DependsOn(ResourceSource other)
        {
            other.Add(dependsOnRef = new DependsOnRef(this));
        }

        public DependsOn(ApplicationCredentials options, string resourceGroup, string deploymentName, string outputName)
        {
            this.deploymentName = deploymentName;
            this.outputName = outputName;
            this.resourceGroup = resourceGroup;
            this.options = options;
        }

        public async Task TemplateActionAsync(JObject obj)
        {
            counter++;
            this.obj = obj;

            if (counter >= 2)
                AddToDependOn();

            if (!string.IsNullOrEmpty(deploymentName))
            {
                var output = await ResourceManagerHelper.GetTemplateDeploymentOutputAsync(options, resourceGroup, deploymentName);
                var token = obj["dependsOn"] as JArray;
                if (token == null)
                    obj["dependsOn"] = token = new JArray();

                token.Add(output[outputName]["value"]);
                
            }

             


        }

        internal void AddToDependOn()
        {
            var token = obj["dependsOn"] as JArray;
            if (token == null)
                obj["dependsOn"] = token = new JArray();

            token.Add(dependsOnRef.obj.SelectToken("name"));

        }
    }
}
