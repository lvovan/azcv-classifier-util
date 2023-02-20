using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;

namespace azcv_classifier_util
{
  [Verb("migrate", HelpText = "Migrate a project from a source computer vision to a destination computer vision.")]
  class MigrateVerb
  {
    [Option("sourceEndpoint", HelpText = "Custom Vision endpoint hosting the source project to be migrated.", Required = true)]
    public string SourceEndpoint { get; set; }

    [Option("destinationEndpoint", HelpText = "Target Custom Vision endpoint. If not specified, the sourceEndpoint will be used.", Required = false)]
    public string DestinationEndpoint { get; set; }

    [Option("sourceKey", HelpText = " Source Custom Vision endpoint key.", Required = true)]
    public string SourceKey { get; set; }

    [Option("destinationKey", HelpText = "Target Custom Vision endpoint key. If not specified, the sourceKey will be used", Required = false)]
    public string DestinationKey { get; set; }

    [Option("sourceProjectId", HelpText = "Project ID to be migrated", Required = true)]
    public Guid SourceProjectId { get; set; }

    [Option("destinationName", HelpText = "Optional, name of the project to use instead of auto-generated name.", Required = false)]
    public string DestinationName { get; set; }

    [Usage(ApplicationAlias = "azcv-classifier-util")]
    public static IEnumerable<Example> Examples
    {
      get
      {
        yield return new Example("Migrate a project to another custom vision resource (hosted in Japan). The new project will be named 'cvJapan'",
          new MigrateVerb
          {
            SourceProjectId = Guid.Parse("8892946f-10c5-4884-94a4-da6c95f98d67"),
            SourceEndpoint = "https://contoso.cognitiveservices.azure.com/",
            DestinationEndpoint = "https://japaneast.api.cognitive.microsoft.com/",
            DestinationKey = "destinationAPIKey",
            DestinationName = "cvJapan",
            SourceKey = "sourceAPIKey"
          });
        yield return new Example("Create a copy of a project within the same custom vision resource. The new project will have an auto-generated name",
          new MigrateVerb
          {
            SourceProjectId = Guid.Parse("8892946f-10c5-4884-94a4-da6c95f98d67"),
            SourceEndpoint = "https://contoso.cognitiveservices.azure.com/",
            SourceKey = "sourceAPIKey"
          });
      }
    }
  }

  class Migrate
  {
    private MigrateVerb options;

    public Migrate(MigrateVerb options)
    {
      this.options = options;
    }

    public async Task ProcessAsync()
    {
      var destinationEndpoint = string.IsNullOrEmpty(this.options.DestinationEndpoint) ? this.options.SourceEndpoint : this.options.DestinationEndpoint;
      var destinationKey = string.IsNullOrEmpty(this.options.DestinationEndpoint) ? this.options.SourceKey : this.options.DestinationKey;

      using (CustomVisionTrainingClient sourceClient =
        new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(this.options.SourceKey))
        {
          Endpoint = this.options.SourceEndpoint
        },
        destinationClient =
        new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(destinationKey))
        {
          Endpoint = destinationEndpoint
        })
      {
        // https://learn.microsoft.com/en-us/rest/api/customvision/training3.3/export-project/export-project?tabs=HTTP#projectexport
        var sourceProject = await sourceClient.ExportProjectAsync(this.options.SourceProjectId);

        // https://learn.microsoft.com/en-us/rest/api/customvision/training3.3/import-project/import-project?tabs=HTTP
        var destinationProject = await destinationClient.ImportProjectAsync(sourceProject.Token, this.options.DestinationName);

        Console.WriteLine($"The project {this.options.SourceProjectId} was successfully migrated to {destinationProject.Name} ({destinationProject.Id})");
      }
    }
  }
}
