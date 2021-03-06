﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.Azure.ResourceManager
{
    public static class Constants
    {
        public static class Templates
        {
            public const string TemplatePrefix = "S-Innovations.Azure.ResourceManager.Templates.";
            public const string AzureKeyVault = TemplatePrefix + "keyvault.json";

            public static class Resources
            {
                public const string ResourcePrefix = "Resources.";
                public const string ServerFarms = TemplatePrefix + ResourcePrefix + "serverfarms.json";
                public const string AutoScale = TemplatePrefix + ResourcePrefix + "autoscale.json";
                public const string AutoScaleProfileMetricTrigger = TemplatePrefix + ResourcePrefix + "AutoScale.metricTrigger.json";
                public const string AutoScaleProfile = TemplatePrefix + ResourcePrefix + "AutoScale.profile.json";
                public const string LogicApp = TemplatePrefix + ResourcePrefix + "LogicApps.workflows.json";
                public const string LogicAppHttpTrigger = TemplatePrefix + ResourcePrefix + "LogicApps.httpTrigger.json";
                public const string KeyVault = TemplatePrefix + ResourcePrefix + "KeyVault.keyvault.json";
                public const string KeyVaultAccessPolicy = TemplatePrefix + ResourcePrefix + "KeyVault.accessPolicy.json";
            }
            public static class Parameters
            {
                public const string ParametersPrefix = "Parameters.";
                public const string ServerFarms = TemplatePrefix + ParametersPrefix + "serverfarms.json";
                public const string LogicApp = TemplatePrefix + ParametersPrefix + "LogicApps.workflows.json";
                public const string KeyVault = TemplatePrefix + ParametersPrefix + "keyvault.json";
            
            }
            public static class Outputs
            {
                public const string OutputsPrefix = "Outputs.";
                public const string ServerFarms = TemplatePrefix + OutputsPrefix + "serverfarms.json";

                public static class ServerFarm
                {
                    public const string AppServicePlanId = "serverFarmId";
                }
            }
        }
    }
}
