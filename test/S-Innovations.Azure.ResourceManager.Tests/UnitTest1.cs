using System;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using SInnovations.Azure.ResourceManager.TemplateActions;
using SInnovations.Azure.ResourceManager.Templates.Resources;

namespace SInnovations.Azure.ResourceManager.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            ResourceSource serverFarm = Constants.Templates.Resources.ServerFarms;

            var serverFarms = await ResourceManagerHelper.CreateTemplateAsync(
                new[]{
                    serverFarm,
                    new AutoScale {
                        new DependsOn(serverFarm),
                        new AutoScaleProfile {
                            new ResourceParamterConstant("autoScaleProfileMaxCapacity",5),
                            new JsonPathSetter("capacity.default", 1),
                            new AutoScaleProfileMetricTrigger {
                                new ResourceParamterConstant("threshold",80),
                                new ResourceParamterConstant("direction","Increase"),
                                 new JsonPathSetter("metricTrigger.operator","GreaterThan"),
                                   new JsonPathSetter("metricTrigger.timeWindow", XmlConvert.ToString(TimeSpan.FromMinutes(10))),
                                 new JsonPathSetter("scaleAction.cooldown", XmlConvert.ToString(TimeSpan.FromMinutes(10))),
                                },
                            new AutoScaleProfileMetricTrigger {
                                 new ResourceParamterConstant("threshold",60),
                                 new ResourceParamterConstant("direction","Decrease"),
                                 new JsonPathSetter("metricTrigger.operator","LessThan"),
                                 new JsonPathSetter("metricTrigger.timeWindow", XmlConvert.ToString(TimeSpan.FromHours(1))),
                                 new JsonPathSetter("scaleAction.cooldown", XmlConvert.ToString(TimeSpan.FromHours(1))),
                            }
                        }
                    }
                },
                Constants.Templates.Parameters.ServerFarms,
                new ResourceSource("SInnovations.Azure.ResourceManager.Tests.paramlist.json", typeof(UnitTest1).Assembly)
               );

            Console.WriteLine(serverFarms.ToString(Newtonsoft.Json.Formatting.Indented));
        }
    }
}
