using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public static void SetAuthorizationOptions(ApplicationCredentials options)
        {
            var token = GetAuthorizationHeader(options.TenantId, options.CliendId, options.Secret, options.ReplyUrl);

            options.AccessToken = token.AccessToken;
            options.RefreshToken = token.RefreshToken;
            options.TenantId = token.TenantId ?? FindClaim(token.AccessToken, "tid");
            options.ObjectId = token.UserInfo?.UniqueId ?? FindClaim(token.AccessToken, "oid");



        }
        public static void ParseAndSetClaimValues(ApplicationCredentials options)
        {
            options.TenantId = options.TenantId ?? FindClaim(options.AccessToken, "tid");
            options.ObjectId = options.ObjectId ?? FindClaim(options.AccessToken, "oid");
        }

        private static string FindClaim(string accessToken, string claim)
        {
            var base64 = accessToken.Split('.')[1];
            base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            var split = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));
            return split.SelectToken(claim).ToString();
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

        public static JObject CreateOutput(string v1, string v2, string v3)
        {
            return new JObject(
                new JProperty(v1,
                    new JObject(
                        new JProperty("type", v2),
                        new JProperty("value", v3)
                    )
                )
            );

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

        public static Stream Read(string name)
        {
            return typeof(ResourceManagerHelper).Assembly.GetManifestResourceStream(name);
        }
        public static Task<string> LoadTemplateAsync(string templatePath, string parameterPath, string variablePath, params string[] parameterNames)
        {
            return LoadTemplatesAsync(new[] { templatePath }, parameterPath, variablePath, parameterNames);
        }




        public static async Task<JObject> CreateTemplateAsync(ResourceSourceCollection resources,
            ResourceSourceCollection parameterPath,
            ResourceSourceCollection variablePath = null,
            ResourceSourceCollection outputs = null)
        {
            if (variablePath == null)
                variablePath = new ResourceSourceCollection();

            var template = new JObject(
                new JProperty("$schema", "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#"),
                new JProperty("contentVersion", "1.0.0.0"),
                new JProperty("resources", new JArray(
                    await Task.WhenAll(resources.Select(r => TemplateHelper.ReadDataAsync(r))))
                ));

            var parameterNames = Regex.Matches(template.ToString(), @"parameters\('(.*?)'\)").Cast<Match>().Where(k => k.Success).Select(k => k.Groups[1].Value).ToList();
            var variableNames = Regex.Matches(template.ToString(), @"variables\('(.*?)'\)").Cast<Match>().Where(k => k.Success).Select(k => k.Groups[1].Value).ToList();


            var parameters = new JObject((await Task.WhenAll(parameterPath.Select(p => TemplateHelper.ReadDataAsync(p)))).SelectMany(o => o.Properties()));
            var variables = new JObject((await Task.WhenAll(variablePath.Select(p => TemplateHelper.ReadDataAsync(p)))).SelectMany(o => o.Properties()));

            var paramterList = parameters.Properties().Where(p => parameterNames.Contains(p.Name)).ToList();
            var variableList = variables.Properties().Where(p => variableNames.Contains(p.Name)).ToList();




            var newEntries = false;
            do
            {
                var variableJson = new JObject(variableList).ToString();
                string[] parameterNamesUpdated = Regex.Matches(variableJson, @"parameters\('(.*?)'\)").Cast<Match>().Where(k => k.Success).Select(k => k.Groups[1].Value).Except(paramterList.Select(p => p.Name)).ToArray();
                string[] variableNamesUpdated = Regex.Matches(variableJson, @"variables\('(.*?)'\)").Cast<Match>().Where(k => k.Success).Select(k => k.Groups[1].Value).Except(variableList.Select(p => p.Name)).ToArray();

                newEntries = variableNamesUpdated.Length > 0 || parameterNamesUpdated.Length > 0;
                if (newEntries)
                {
                    paramterList.AddRange(parameters.Properties().Where(p => parameterNamesUpdated.Contains(p.Name)));
                    variableList.AddRange(variables.Properties().Where(p => variableNamesUpdated.Contains(p.Name)));
                }

            } while (newEntries);
            //string[] variableNames = Regex.Matches(template.ToString(), @"variables\('(.*?)'\)").Cast<Match>().Where(k => k.Success).Select(k => k.Groups[1].Value).ToArray();

            template["parameters"] = new JObject(paramterList);
            template["variables"] = new JObject(variableList);

            if (outputs != null)
                template["outputs"] = new JObject(
                   (await Task.WhenAll(outputs.Select(r => TemplateHelper.ReadDataAsync(r)))).SelectMany(o => o.Properties()));


            return template;
        }

        public static async Task<string> LoadTemplatesAsync(string[] templatePaths, string parameterPath, string variablePath, params string[] parameterNames)
        {
            var templates = await Task.WhenAll(templatePaths.Select(templatePath => TemplateHelper.ReadDataAsync(templatePath)));
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

            var parameters = await TemplateHelper.ReadDataAsync(parameterPath);
            var variables = await TemplateHelper.ReadDataAsync(variablePath);
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
                            if (parameterNames.Contains(paramName))
                                return true;

                        }
                    }
                }
                return false;
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
            var resourceGroup = new ResourceGroup { Location = location, Tags = new Dictionary<string, string> { { "hidden-source", typeof(ResourceManagerHelper).Assembly.FullName } } };
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
        public async static Task<ResourceGroup> UpdateResourceGroupAsync(string subscriptionId, string token, ResourceGroup group)
        {
            ServiceClientCredentials credential = new TokenCredentials(token);
            using (var resourceManagementClient = new ResourceManagementClient(credential))
            {
                resourceManagementClient.SubscriptionId = subscriptionId;
                return await resourceManagementClient.ResourceGroups.CreateOrUpdateAsync(group.Name, group);
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

        public async static Task<JObject> GetTemplateDeploymentOutputAsync(ApplicationCredentials credentials,
            string resourceGroup, string deploymentName)
        {
            using (var templateDeploymentClient = new ResourceManagementClient(new TokenCredentials(credentials.AccessToken)))
            {
                templateDeploymentClient.SubscriptionId = credentials.SubscriptionId;
                if (!await templateDeploymentClient.Deployments.CheckExistenceAsync(resourceGroup, deploymentName) ?? false)
                {
                    return null;
                }
                var deploymentResultWrapper = await templateDeploymentClient.Deployments.GetAsync(resourceGroup, deploymentName);
                return deploymentResultWrapper.Properties.Outputs as JObject;

            }
        }
        public async static Task<DeploymentValidateResult> ValidateTemplateDeploymentAsync(
            ApplicationCredentials credentials, string resourceGroup, string deploymentName,
            JObject template, JObject parameters, DeploymentMode mode = DeploymentMode.Incremental)
        {

            var hash = TemplateHelper.CalculateMD5Hash(template.ToString() + parameters.ToString());
            var deployment = new Deployment();

            deployment.Properties = new DeploymentProperties
            {
                Mode = mode,
                Template = template,
                Parameters = parameters
            };
            using (var templateDeploymentClient = new ResourceManagementClient(new TokenCredentials(credentials.AccessToken)))
            {
                templateDeploymentClient.SubscriptionId = credentials.SubscriptionId;


                var result = await templateDeploymentClient.Deployments.ValidateAsync(resourceGroup, deploymentName, deployment);

                if (result.Error != null)
                    throw new Exception(result.Error.Message);

                return result;




            }

        }
        public async static Task<DeploymentExtended> CreateTemplateDeploymentAsync(
            ApplicationCredentials credentials, string resourceGroup, string deploymentName,
            JObject template, JObject parameters, bool waitForDeployment = true, DeploymentMode mode = DeploymentMode.Incremental, bool prevalidate= false)
        {

            var hash = TemplateHelper.CalculateMD5Hash(template.ToString() + parameters.ToString());
            var deployment = new Deployment();
            
            deployment.Properties = new DeploymentProperties
            {
                Mode = mode,
                Template = template,
                Parameters = parameters
            };
            using (var templateDeploymentClient = new ResourceManagementClient(new TokenCredentials(credentials.AccessToken)))
            {
                templateDeploymentClient.SubscriptionId = credentials.SubscriptionId;

                var rg = await templateDeploymentClient.ResourceGroups.GetAsync(resourceGroup);
                if (rg.Tags == null)
                    rg.Tags = new Dictionary<string, string>();

                var tagName = $"hidden-armdeployment{hash}";
                var tagValue = deploymentName;
                if (rg.Tags.ContainsKey(tagName) && (await templateDeploymentClient.Deployments.CheckExistenceAsync(resourceGroup, rg.Tags[tagName]) ?? false))
                {
                    var deploymentResult = await templateDeploymentClient.Deployments.GetAsync(resourceGroup, rg.Tags[tagName]);
                    if (deploymentResult.Properties.ProvisioningState == "Succeeded")
                    {
                        return deploymentResult;
                    }
                }

                {


                    if (prevalidate)
                    {
                        var result = await templateDeploymentClient.Deployments.ValidateAsync(resourceGroup, deploymentName, deployment);

                        if (result.Error != null)
                            throw new Exception(result.Error.Message);
                    }

                    var deploymentResult = await templateDeploymentClient.Deployments.BeginCreateOrUpdateAsync(resourceGroup,
                        deploymentName, deployment);
                  

                    //foreach (var key in rg.Tags.Keys.Where(k => k.StartsWith("hidden-armdeployment")).ToArray())
                    //{
                    //    rg.Tags.Remove(key);
                    //}
                    rg.Tags[tagName] = tagValue;

                    await templateDeploymentClient.ResourceGroups.CreateOrUpdateAsync(resourceGroup, rg);

                    while (waitForDeployment && !(deploymentResult.Properties.ProvisioningState == "Failed" || deploymentResult.Properties.ProvisioningState == "Succeeded"))
                    {
                        var deploymentResultWrapper = await templateDeploymentClient.Deployments.GetAsync(resourceGroup, deploymentResult.Name);

                        deploymentResult = deploymentResultWrapper;
                        await Task.Delay(5000);
                    }

                    return deploymentResult;

                }

            }

        }
    }
}
