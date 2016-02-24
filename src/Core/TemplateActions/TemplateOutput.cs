using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SInnovations.Azure.ResourceManager.TemplateActions
{
    public class TemplateVariable : ITemplateAction
    {
        string name;
        JToken value;
        public TemplateVariable(string name, JToken value)
        {
            this.name = name;
            this.value = value;
        }
        public async Task TemplateActionAsync(JObject obj)
        {
            obj[name] = value;
        }
    }
    public class TemplateOutput : ITemplateAction
    {
        string name;
        string type;
        JToken value;
        public TemplateOutput(string name, string type, JToken value)
        {
            this.name = name;
            this.type = type;
            this.value = value;
        }
        public async Task TemplateActionAsync(JObject obj)
        {
            if (obj.SelectToken("$schema")?.ToString().Contains("deploymentTemplate") ?? false)
            {
                obj = (obj["output"] ?? (obj["output"] = new JObject())) as JObject;
            }

            obj[this.name] =
                    new JObject(
                        new JProperty("type", this.type),
                        new JProperty("value", this.value)
                    );

        }

        public static ITemplateAction FromVaraible(string v1, string v2)
        {
            return new TemplateOutput(v1, v2, $"[variables('{v1}')]");
        }
    }
}

