﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SInnovations.Azure.ResourceManager.Templates.Resources
{
    public class AutoScaleProfile : ResourceSourceComposer, IAfterLoadActions
    { 
     

        public AutoScaleProfile() : base(Constants.Templates.Resources.AutoScaleProfile, typeof(AutoScale).Assembly)
        {
          
        }

        public async Task<JObject> ApplyAfterLoadActionsAsync(JObject obj)
        {
            JArray rulesArray = obj.SelectToken("rules") as JArray;
            rulesArray.Clear();

            foreach (var rule in await Task.WhenAll(this.Select(TemplateHelper.ReadDataAsync)))
                rulesArray.Add(rule);

            return obj;
        }

    }
}
