using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SInnovations.Azure.ResourceManager.TemplateActions;

namespace SInnovations.Azure.ResourceManager.Templates.Resources.LogicApps
{
  
  
    public class HttpTrigger : ResourceSource, IAfterLoadActions
    {
        private string name;

        public HttpTrigger(string name) : base(Constants.Templates.Resources.LogicAppHttpTrigger, typeof(HttpTrigger).Assembly)
        {
            this.name = name;
            this.Add(new JsonPathSetter(name+".inputs.uri", "http://requestb.in/18z7v7w1"));
        }

        public async Task<JObject> ApplyAfterLoadActionsAsync(JObject obj)
        {
            foreach(var pathSetters in this.Actions.OfType<JsonPathSetter>().Where(p=>!p.Path.StartsWith(name)))
                pathSetters.Path = name +"."+ pathSetters.Path;

            return new JObject(new JProperty(name, obj));
        }
    }
}
