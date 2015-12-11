using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace SInnovations.Azure.ResourceManager.Templates.Resources
{
    public class AutoScale : ResourceSourceComposer, IAfterLoadActions
    {
      
        public AutoScale() : base(Constants.Templates.Resources.AutoScale, typeof(AutoScale).Assembly)
        {

        }
       

        public void ApplyAfterLoadActions(JObject obj)
        {
            JArray profilesArray = obj.SelectToken("properties.profiles") as JArray;
            profilesArray.Clear();

            foreach (var profile in this.Select(TemplateHelper.ReadData))
                profilesArray.Add(profile);
        }
    }

    
    
}
