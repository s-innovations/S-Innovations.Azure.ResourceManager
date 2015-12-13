using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SInnovations.Azure.ResourceManager.Templates.Resources.LogicApps;

namespace SInnovations.Azure.ResourceManager
{
    public static class TemplateHelper
    {
        public static Task<JObject> ReadDataAsync(ResourceSource resourceName)
        {
            if (string.IsNullOrEmpty(resourceName.Path))
                return Load(resourceName, new JObject());

            using (Stream stream = resourceName.Assembly.GetManifestResourceStream(resourceName.Path))
            using (StreamReader reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var obj= JObject.Load(jsonReader);
                return Load(resourceName, obj);
               
            }
        }
        private static async Task<JObject> Load(ResourceSource resourceName, JObject obj)
        {
            if (resourceName is IAfterLoadActions)
            {
                obj = await((IAfterLoadActions)resourceName).ApplyAfterLoadActionsAsync(obj);
            }
            foreach (var action in resourceName)
                await action.TemplateActionAsync(obj);

            return obj;
        }
      

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
