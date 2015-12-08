﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.OData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SInnovations.Azure.ResourceManager
{
    public class ResourceManagerHelper
    {

        public static JObject CreateValue(string value)
        {
            return new JObject(new JProperty("value", value));
        }
        public static JProperty CreateValue(string key, JToken value)
        {
            return new JProperty(key, new JObject(new JProperty("value", value)));
        }
     
        public static AuthenticationResult GetAuthorizationHeader(string tenant, string clientId, string secret, string redirectUrl = null)
        {
            if (!string.IsNullOrEmpty(secret))
            {
                ClientCredential cc = new ClientCredential(clientId, secret);
                var context = new AuthenticationContext($"https://login.windows.net/{tenant}", new FileCache());
                var result = context.AcquireToken("https://management.core.windows.net/", cc);
                if (result == null)
                {
                    throw new InvalidOperationException("Failed to obtain the JWT token");
                }
                return result;
            }
            else
            {
                var context = new AuthenticationContext($"https://login.windows.net/{tenant}", new FileCache());
                var result = context.AcquireToken("https://management.core.windows.net/", clientId, new Uri(redirectUrl));
                if (result == null)
                {
                    throw new InvalidOperationException("Failed to obtain the JWT token");
                }
                return result;
            }
        }

        public static AuthenticationResult GetAuthorizationHeader(ApplicationCredentials options)
        {
            return GetAuthorizationHeader(options.TenantId, options.CliendId, options.Secret, options.ReplyUrl);
        }

        public static string test(string clientId, string redirectUri, string tenant = null)
        {
            var authContext = new AuthenticationContext($"https://login.windows.net/{tenant ?? "common"}", new FileCache());
            var t = authContext.GetAuthorizationRequestURL("https://management.core.windows.net/", clientId, new Uri(redirectUri), UserIdentifier.AnyUser, null);

            //    var code = "AAABAAAAiL9Kn2Z27UubvWFPbm0gLfmp1ExBQltdN9obd6rGamy-zGUM4_wBv9xKG8Q-XwQl2CpxGEz-N2LYNovZMcylQ1zR_u7XV5TD-aN2yOp1rjC2mJpzAI2AaiezbUOvHKouTgeAvWEDk3QUd_qhGZTWaVkOzYHFawqmPKXshpYQozsRslmvhr49VoVEgJs7eyF7COBennf6A3aVDBGtAijfouJLo1kKhYhalf3bRR1wdbLApj7GKaYb7oy-Q_6mGLry1rcQMNHg5h4gvRPeYqT7jX3FGmUePjj1-TwKsIylvvC4f8f69D4v_Wp11FsI6WLSH95wJAj6FKDG04ixUSoy6AXujJcWMZbv0AOzZ3X-V_EmMFM6InNrebmA_3awMibHNI62EtpOjpgnb4FjyboFplXhcNMOUio1DwwOu7sa0IFm0UVK1KTTCra6V34k9BiQfCR0bZXpG9fn3RqwaaCJZu4NBttP1oXoryrp6YsxbskOqJCTe-_AeiPMgcm-I24rzU_8x9ZKQ-JM5ACFySXQggq_csTcWG-Kj8-JT4VY4xKFOGBCO5czxn_g0bH3UuUf8DniOxZPZ0EEoDaUhxfraTpXVy9p5o4hXyr65Upt5eYy5LxR1Emdc-Mfho92SsEnimqMXexwtopnHqx0z-pr9OCe5vnZZdyWFbHNyhDeM6ADXvnKbTQy2xW3kaUMFWaaLd3-s4jq6_D4Py3Mawtz5iAA";
            //    authContext.AcquireTokenByAuthorizationCode(code, new Uri(redirectUri), new ClientCredential(clientId, secret));
            var token = authContext.AcquireToken("https://management.core.windows.net/", clientId, new Uri(redirectUri), PromptBehavior.Auto, UserIdentifier.AnyUser);

            return token.AccessToken;
        }

        public async static Task<Subscription[]> ListSubscriptions(string azureToken)
        {
            {

                using (var subscriptionClient = new SubscriptionClient(new TokenCredentials(azureToken)))
                {
                    subscriptionClient.SubscriptionId = "";
                    var subscriptions = await subscriptionClient.Subscriptions.ListAsync();
                    return subscriptions.ToArray();


                }
            }

        }
        private static JObject ReadData(string resourceName)
        {
            var assembly = typeof(ResourceManagerHelper).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return JObject.Load(jsonReader);
            }
        }
        public static Stream Read(string name)
        {
            return typeof(ResourceManagerHelper).Assembly.GetManifestResourceStream(name);
        }
        public static string LoadTemplate(string templatePath, string parameterPath, string variablePath, params string[] parameterNames)
        {
            return LoadTemplates(new[] { templatePath }, parameterPath, variablePath, parameterNames);
        }
        public static string LoadTemplates(string[] templatePaths, string parameterPath, string variablePath, params string[] parameterNames)
        {
            var templates = templatePaths.Select(templatePath => ReadData(templatePath)).ToArray();
            var template = templates.First();
            if (templates.Skip(1).Any())
            {
                var resources = template["resources"] as JArray;
                foreach (var templateCopy in templates.Skip(1))
                {
                    var resourcesCopy = templateCopy["resources"] as JArray;
                    foreach (var resourceCopy in resourcesCopy)
                        resources.Add(resourceCopy);
                }
            }

            var parameters = ReadData(parameterPath);
            var variables = ReadData(variablePath);
            template["parameters"] = new JObject(parameters.Properties().Where(p => parameterNames.Contains(p.Name)));

            var varProps = variables.Properties().Where(CreateFilter(parameterNames, "parameters")).ToArray();
            varProps = varProps.Where(CreateFilter(varProps.Select(p => p.Name).ToArray(), "variables")).ToArray();


            template["variables"] = new JObject(varProps);

            return template.ToString();

        }

        private static Func<JProperty, bool> CreateFilter(string[] parameterNames, string regexName)
        {
            return p =>
            {
                if (p.Value.Type == JTokenType.String)
                {
                    MatchCollection matches = Regex.Matches(p.Value.ToString(), regexName + @"\((.*?)\)");
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            var paramName = match.Groups[1].Value.Trim('\'', '"');
                            if (!parameterNames.Contains(paramName))
                                return false;

                        }
                    }
                }
                return true;
            };
        }
        public async static Task<ResourceGroup[]> ListResourceGroups(string subscriptionId, string token)
        {
            ServiceClientCredentials credential = new TokenCredentials(token);
            //  var resourceGroup = new ResourceGroup { Location = location, Tags = new Dictionary<string, string> { { "source", "SInnovations.Docker.ResourceManager" } } };
            using (var resourceManagementClient = new ResourceManagementClient(credential))
            {
                resourceManagementClient.SubscriptionId = subscriptionId;
                var rgResult = await resourceManagementClient.ResourceGroups.ListAsync(new ODataQuery<ResourceGroupFilter>());
                return rgResult.ToArray();
            }
        }
        public async static Task<ResourceGroup> CreateResourceGroupIfNotExistAsync(string subscriptionId, string token, string resourceGroupName, string location)
        {
            ServiceClientCredentials credential = new TokenCredentials(token);
            var resourceGroup = new ResourceGroup { Location = location, Tags = new Dictionary<string, string> { { "source", "SInnovations.Docker.ResourceManager" } } };
            using (var resourceManagementClient = new ResourceManagementClient(credential))
            {
                resourceManagementClient.SubscriptionId = subscriptionId;
                if (!(await resourceManagementClient.ResourceGroups.CheckExistenceAsync(resourceGroupName) ?? false))
                {
                    return await resourceManagementClient.ResourceGroups.CreateOrUpdateAsync(resourceGroupName, resourceGroup);
                }

                return await resourceManagementClient.ResourceGroups.GetAsync(resourceGroupName);
            }
        }
        public async static Task DeleteIfExists(string subscriptionId, string token, string resourceGroupName)
        {


            using (var resourceManagementClient = new ResourceManagementClient(new TokenCredentials(token)))
            {
                resourceManagementClient.SubscriptionId = subscriptionId;
                await resourceManagementClient.ResourceGroups.DeleteAsync(resourceGroupName);

            }
        }
        public async static Task DeleteTemplateDeployment(string subscriptionId, string token, string resourceGroup, string deploymentName)
        {

            using (var templateDeploymentClient = new ResourceManagementClient(new TokenCredentials(token)))
            {
                templateDeploymentClient.SubscriptionId = subscriptionId;

                await templateDeploymentClient.Deployments.DeleteAsync(resourceGroup, deploymentName);


            }
        }
        

        public async static Task<DeploymentExtended> CreateTemplateDeploymentAsync(string subscriptionId, string token, string resourceGroup, string deploymentName, string template, string parameters, bool waitForDeployment = true)
        {

            var hash = TemplateHelper.CalculateMD5Hash(template + parameters);
            var deployment = new Deployment();

            deployment.Properties = new DeploymentProperties
            {
                Mode = DeploymentMode.Incremental,
                Template = JObject.Parse(template),
                Parameters = JObject.Parse(parameters)
            };
            using (var templateDeploymentClient = new ResourceManagementClient(new TokenCredentials(token)))
            {
                templateDeploymentClient.SubscriptionId = subscriptionId;

                var rg = await templateDeploymentClient.ResourceGroups.GetAsync(resourceGroup);
                if (!(rg.Tags.ContainsKey(deploymentName) && rg.Tags[deploymentName] == hash && ((await templateDeploymentClient.Deployments.CheckExistenceAsync(resourceGroup, deploymentName)) ?? false)))
                {



                    var result = await templateDeploymentClient.Deployments.ValidateAsync(resourceGroup, deploymentName, deployment);



                    var deploymentResult = await templateDeploymentClient.Deployments.CreateOrUpdateAsync(resourceGroup,
                        deploymentName, deployment);

                    rg.Tags.Add(deploymentName, hash);
                    await templateDeploymentClient.ResourceGroups.CreateOrUpdateAsync(resourceGroup, rg);

                    while (waitForDeployment && !(deploymentResult.Properties.ProvisioningState == "Failed" || deploymentResult.Properties.ProvisioningState == "Succeeded"))
                    {
                        var deploymentResultWrapper = await templateDeploymentClient.Deployments.GetAsync(resourceGroup, deploymentResult.Name);

                        deploymentResult = deploymentResultWrapper;
                        await Task.Delay(5000);
                    }

                    return deploymentResult;
                }
                else {

                    var deploymentResult = await templateDeploymentClient.Deployments.GetAsync(resourceGroup, deploymentName);
                    return deploymentResult;
                }

            }

        }
    }
}
