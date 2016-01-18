using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SInnovations.Azure.ResourceManager.TemplateActions
{
    public class JsonPathSetter : ITemplateAction
    {
        public string Path { get; set; }
        private JToken value;
        public JsonPathSetter(string path, JToken value)
        {
            this.Path = path;
            this.value = value;
        }

        public async Task TemplateActionAsync(JObject obj)
        {
            (obj.SelectToken(Path).Parent as JProperty).Value = value;
        }
    }
    public class ResourceParamterConstant : ITemplateAction
    {
        private string parameterName;
        private JToken value;
        public ResourceParamterConstant(string parameterName, JToken value)
        {
            this.parameterName = parameterName;
            this.value = value;
        }
        public Task TemplateActionAsync(JObject obj)
        {
            foreach (var prop in obj.Properties())
            {
                ReplaceValue(prop.Value);
            }
            return Task.FromResult(0);
        }
        private void ReplaceValue(JToken token)
        {
            if (token.Type == JTokenType.Array)
            {
                foreach (var obj in token)
                {
                    ReplaceValue(obj);
                }
            }
            else if (token.Type == JTokenType.Object)
            {
                TemplateActionAsync(token as JObject).Wait();
            }
            else if (token.Type == JTokenType.String)
            {
                var newValue = token.Value<string>().Replace($"parameters('{parameterName}')", value.ToString());

                if (newValue.Equals(value.ToString()))
                    return;


                if (newValue.Trim('[', ']').Equals(value.ToString()))
                {
                    (token.Parent as JProperty).Value = value;
                }
                else {
                    (token.Parent as JProperty).Value = newValue;
                }
            }
        }
    }
}
