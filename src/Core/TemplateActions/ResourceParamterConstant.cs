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
        private string path;
        private JToken value;
        public JsonPathSetter(string path, JToken value)
        {
            this.path = path;
            this.value = value;
        }

        public void TemplateAction(JObject obj)
        {
            (obj.SelectToken(path).Parent as JProperty).Value = value;
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
        public void TemplateAction(JObject obj)
        {
            foreach (var prop in obj.Properties())
            {
                ReplaceValue(prop.Value);
            }
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
                TemplateAction(token as JObject);
            }
            else if (token.Type == JTokenType.String)
            {
                var newValue = token.Value<string>().Replace($"parameters('{parameterName}')", value.ToString());
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
