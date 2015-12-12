using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SInnovations.Azure.ResourceManager.Templates.Resources
{
    public class AutoScaleProfileMetricTrigger : ResourceSource, IAfterLoadActions
    {
        public AutoScaleProfileMetricTrigger() : base(Constants.Templates.Resources.AutoScaleProfileMetricTrigger, typeof(AutoScale).Assembly)
        {

        }

        public async Task<JObject> ApplyAfterLoadActionsAsync(JObject obj)
        {
            //obj.SelectToken("properties")
            return obj;
        }
    }
}
