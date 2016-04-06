using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using SInnovations.Azure.ResourceManager.TemplateActions;

namespace SInnovations.Azure.ResourceManager.Tests
{
    [TestClass]
    public class ResourceParamterConstantTest
    {
        [TestMethod]
        public void TestMethod1()
        {

            var action = new ResourceParamterConstant("secretValue", "[concat('Data Source=tcp:', reference(concat('Microsoft.Sql/servers/', variables('sqlserverName'))).fullyQualifiedDomainName, ',1433;Initial Catalog=', parameters('databaseName'), ';User Id=', parameters('administratorLogin'), '@', variables('sqlserverName'), ';Password=', parameters('administratorLoginPassword'), ';')]");

            var template = new StreamReader(GetType().Assembly.GetManifestResourceStream("SInnovations.Azure.ResourceManager.Tests.Fixtures.secretTest.json")).ReadToEnd();
            var expected = new StreamReader(GetType().Assembly.GetManifestResourceStream("SInnovations.Azure.ResourceManager.Tests.Fixtures.secretTestExpected.json")).ReadToEnd();


            var obj = JObject.Parse(template);
            action.TemplateActionAsync(obj);

            Assert.AreEqual(JObject.Parse(expected).ToString(), obj.ToString());
        }
    }
}
