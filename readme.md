
# S-Innovations.Azure.ResourceManager
![Build Status](https://sinnovations.visualstudio.com/DefaultCollection/_apis/public/build/definitions/40c16cc5-bf99-47d4-a814-56c38cc0ea24/1/badge)

A set of helper libraries that tries to make it easier to author and maintain Resource Manager Templates aswell as utility functions to common devops tasks involved when maintaining resources on Azure. 

### Disclaimer
The libraries are my opininated result of how I prefer to work with my azure resources. I will provide limited example and documentation on how to use it for others to get the idea and hopefully adjust to their needs.


## Introduction to Resource Manager Templates

The library provides extensions to edit and and manipulate the Azure Resource Manager Json Templates. Some common templates are included as embeded resources in the dll but the following should be seen as a proof of concept.

Do note that the lirary if for editing templates, it is not the scope to provide all resource templates in the provider as this would be a time consuming manual task to maintain which also was one of the points in the following [twitter conversation](https://twitter.com/pksorensen/status/675244196882685956).


### ResourceSource
The ResourceSource class is a representation of a template element. And if one needs a serverfarm template one could include it like the following
```
ResourceSource serverFarm = Constants.Templates.Resources.ServerFarms;
                            //"S-Innovations.Azure.ResourceManager.Templates.Resources.serverfarms.json";
```
which is possible because of implicit operator, and its seen here that it can be used to load the embeded resources.
```
public static implicit operator ResourceSource(string source)
{
    return new ResourceSource
    {
        Path = source,
        Assembly = typeof(ResourceSource).Assembly
    };
}
```
For using resource templates within your own dlls you simply construct ResourceSource
```
public ResourceSource(string path, Assembly assembly)
{
	this.Path = path;
	this.Assembly = assembly;
}
```

The embeded serverfarms resource template looks like:
```
{
  "apiVersion": "2014-06-01",
  "name": "[parameters('hostingPlanName')]",
  "type": "Microsoft.Web/serverfarms",
  "location": "[parameters('hostingPlanLocation')]",
  "properties": {
    "name": "[parameters('hostingPlanName')]",
    "sku": "[parameters('hostingPlanSku')]",
    "workerSize": "[parameters('hostingPlanWorkerSize')]",
    "numberOfWorkers": 1
  }
}
```

So the library require one to get the basic understanding of templates.

### Authoring Helpers
The library includes template helpers that makes it easy to using c# to edit and manipulate the resources dynamic.

#### JsonPathSetter
The JsonPathSetter can be used to alter part of the template like the following example of a LogicApp workflow template.
```
{
  "type": "Microsoft.Logic/workflows",
  "apiVersion": "2015-02-01-preview",
  "name": "[parameters('logicAppName')]",
  "location": "[parameters('hostingPlanLocation')]",
  "properties": {
    "sku": {
      "name": "[parameters('hostingPlanSku')]",
      "plan": {
        "id": "[concat(resourceGroup().id, '/providers/Microsoft.Web/serverfarms/',parameters('hostingPlanName'))]"
      }
    },
    "definition": {
      "$schema": "https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2014-12-01-preview/workflowdefinition.json#",
      "contentVersion": "1.0.0.0",
      "parameters": { },
      "triggers": { },
      "actions": { },
      "outputs": { }
    }
  }
}
```
which can be loaded and manipulated like the followin.
```
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
```
where the HttpTrigger class is an example of making classes to give better context and reusablility of templates that you use more.
```
{
  "type": "Http",
  "inputs": {
    "method": "PUT",
    "uri": ""
  },
  "recurrence": {
    "frequency": "Hour",
    "interval": 2
  }
}
```

with a full example
```
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
```
gives the following template
```
{
  "$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
  "contentVersion": "1.0.0.0",
  "resources": [
    {
      "type": "Microsoft.Logic/workflows",
      "apiVersion": "2015-02-01-preview",
      "name": "[parameters('logicAppName')]",
      "location": "[parameters('hostingPlanLocation')]",
      "properties": {
        "sku": {
          "name": "[parameters('hostingPlanSku')]",
          "plan": {
            "id": "[concat(resourceGroup().id, '/providers/Microsoft.Web/serverfarms/',parameters('hostingPlanName'))]"
          }
        },
        "definition": {
          "$schema": "https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2014-12-01-preview/workflowdefinition.json#",
          "contentVersion": "1.0.0.0",
          "parameters": {},
          "triggers": {
            "http1": {
              "type": "Http",
              "inputs": {
                "method": "PUT",
                "uri": "http://requestb.in/18z7v7w1"
              },
              "recurrence": {
                "frequency": "Hour",
                "interval": 1
              }
            },
            "http2": {
              "type": "Http",
              "inputs": {
                "method": "PUT",
                "uri": "http://requestb.in/18z7v7w1"
              },
              "recurrence": {
                "frequency": "Hour",
                "interval": 2
              }
            }
          },
          "actions": {},
          "outputs": {}
        }
      }
    }
  ],
  "parameters": {
    "logicAppName": {
      "type": "string",
      "metadata": {
        "description": "The name of the logic app to create."
      }
    }
  },
  "variables": {}
}
```
#### ResourceParamterConstant
ResourceParamterConstant can be used to replace parameters in the templates to constant values, but JsonPathSetter is preferable.

#### DependsOn
```
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
    new ResourceSource("S-Innovations.Azure.ResourceManager.Tests.paramlist.json", typeof(UnitTest1).Assembly)
    );

Console.WriteLine(serverFarms.ToString(Newtonsoft.Json.Formatting.Indented));
```

would give 

```
{
  "$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
  "contentVersion": "1.0.0.0",
  "resources": [
    {
      "apiVersion": "2014-06-01",
      "name": "[parameters('hostingPlanName')]",
      "type": "Microsoft.Web/serverfarms",
      "location": "[parameters('hostingPlanLocation')]",
      "properties": {
        "name": "[parameters('hostingPlanName')]",
        "sku": "[parameters('hostingPlanSku')]",
        "workerSize": "[parameters('hostingPlanWorkerSize')]",
        "numberOfWorkers": 1
      }
    },
    {
      "apiVersion": "2014-04-01",
      "name": "[concat(parameters('hostingPlanName'),'-autoscale')]",
      "type": "Microsoft.Insights/autoscalesettings",
      "location": "[parameters('hostingPlanLocation')]",
      "tags": {
        "displayName": "AutoScaleSettings"
      },
      "properties": {
        "profiles": [
          {
            "name": "Default",
            "capacity": {
              "minimum": 1,
              "maximum": 5,
              "default": 1
            },
            "rules": [
              {
                "metricTrigger": {
                  "metricName": "CpuPercentage",
                  "metricResourceUri": "[concat(resourceGroup().id, '/providers/Microsoft.Web/serverfarms/', parameters('hostingPlanName'))]",
                  "timeGrain": "PT1M",
                  "statistic": "Average",
                  "timeWindow": "PT10M",
                  "timeAggregation": "Average",
                  "operator": "GreaterThan",
                  "threshold": 80
                },
                "scaleAction": {
                  "direction": "Increase",
                  "type": "ChangeCount",
                  "value": 1,
                  "cooldown": "PT10M"
                }
              },
              {
                "metricTrigger": {
                  "metricName": "CpuPercentage",
                  "metricResourceUri": "[concat(resourceGroup().id, '/providers/Microsoft.Web/serverfarms/', parameters('hostingPlanName'))]",
                  "timeGrain": "PT1M",
                  "statistic": "Average",
                  "timeWindow": "PT1H",
                  "timeAggregation": "Average",
                  "operator": "LessThan",
                  "threshold": 60
                },
                "scaleAction": {
                  "direction": "Decrease",
                  "type": "ChangeCount",
                  "value": 1,
                  "cooldown": "PT1H"
                }
              }
            ]
          }
        ],
        "enabled": true,
        "name": "[concat(parameters('hostingPlanName'),'-autoscale')]",
        "targetResourceUri": "[concat(resourceGroup().id, '/providers/Microsoft.Web/serverfarms/', parameters('hostingPlanName'))]"
      },
      "dependsOn": [
        "[parameters('hostingPlanName')]"
      ]
    }
  ],
  "parameters": {
    "hostingPlanName": {
      "type": "string",
      "metadata": {
        "description": "The name of the App Service plan to use for hosting the web app."
      }
    },
    "hostingPlanLocation": {
      "type": "string",
      "metadata": {
        "description": "The location to use for creating the web app and hosting plan."
      }
    },
    "hostingPlanSku": {
      "type": "string",
      "allowedValues": [
        "Free",
        "Shared",
        "Basic",
        "Standard"
      ],
      "defaultValue": "Free",
      "metadata": {
        "description": "The pricing tier for the hosting plan."
      }
    },
    "hostingPlanWorkerSize": {
      "allowedValues": [
        "0",
        "1",
        "2"
      ],
      "defaultValue": "0",
      "metadata": {
        "description": "The instance size of the hosting plan (small, medium, or large)."
      },
      "type": "string"
    }
  },
  "variables": {}
}


```










