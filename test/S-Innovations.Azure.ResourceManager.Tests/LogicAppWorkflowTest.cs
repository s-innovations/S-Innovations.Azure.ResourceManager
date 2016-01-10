using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SInnovations.Azure.ResourceManager.TemplateActions;
using SInnovations.Azure.ResourceManager.Templates.Resources.LogicApps;

namespace SInnovations.Azure.ResourceManager.Tests
{
    [TestClass]
    public class LogicAppWorkflowTest
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var logicAppTemplate = await ResourceManagerHelper.CreateTemplateAsync(
                new[]
                {
                    new ResourceSource(Constants.Templates.Resources.LogicApp)
                    {
                        new JsonPathSetter("properties.definition.triggers",
                            new ResourceSourceCollection {
                                new HttpTrigger("http1") {
                                    new JsonPathSetter("recurrence.interval",1)
                                },
                                new HttpTrigger("http2")
                            })
                    }
                },
                new[] {
                    Constants.Templates.Parameters.LogicApp
                    }
                );

            Console.WriteLine(logicAppTemplate.ToString(Formatting.Indented));
        }
    }
}
