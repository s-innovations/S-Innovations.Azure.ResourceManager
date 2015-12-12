using System;
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
            public const string TemplatePrefix = "SInnovations.Azure.ResourceManager.Templates.";
            public const string AzureKeyVault = TemplatePrefix + "keyvault.json";

            public static class Resources
            {
                public const string ResourcePrefix = "Resources.";
                public const string ServerFarms = TemplatePrefix + ResourcePrefix + "serverfarms.json";
                public const string AutoScale = TemplatePrefix + ResourcePrefix + "autoscale.json";
                public const string AutoScaleProfileMetricTrigger = TemplatePrefix + ResourcePrefix + "AutoScale.metricTrigger.json";
                public const string AutoScaleProfile = TemplatePrefix + ResourcePrefix + "AutoScale.profile.json";
                public const string LogicApp = TemplatePrefix + ResourcePrefix + "LogicApps.workflows.json";
            }
            public static class Parameters
            {
                public const string ParametersPrefix = "Parameters.";
                public const string ServerFarms = TemplatePrefix + ParametersPrefix + "serverfarms.json";
                public const string LogicApp = TemplatePrefix + ParametersPrefix + "LogicApps.workflows.json";
            }

        }
    }
}
