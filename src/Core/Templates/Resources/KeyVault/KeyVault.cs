using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SInnovations.Azure.ResourceManager.Attributes;

namespace SInnovations.Azure.ResourceManager.Templates.Resources
{
    public class KeyVault : ResourceSource, IAfterLoadActions
    {


        public KeyVault() : base(Constants.Templates.Resources.KeyVault, typeof(KeyVault).Assembly)
        {

        }

        public async Task<JObject> ApplyAfterLoadActionsAsync(JObject obj)
        {
            JArray rulesArray = obj.SelectToken("properties.accessPolicies") as JArray;

            var childs = await Task.WhenAll(this.Select(TemplateHelper.ReadDataAsync));
            if (childs.Any()) {
                rulesArray.Clear();
                
                foreach (var rule in childs)
                    rulesArray.Add(rule);
            }
            return obj;
        }
    }
}
