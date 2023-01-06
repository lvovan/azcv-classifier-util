using CommandLine;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using System;
using System.Threading.Tasks;

namespace azcv_classifier_util
{
  [Verb("reset", HelpText = "Resets a project by removing all tags and images.")]
  class ResetVerb : CommonOptions
  {
    [Option('i', "interactive", HelpText = "Prompt for user confirmation before resetting the project.", Required = false, Default = true)]
    public bool IsInteractive { get; set; }
  }

  class Reset
  {
    private ResetVerb options { get; set; }

    public Reset(ResetVerb options)
    {
      this.options = options;
    }

    public async Task ProcessAsync()
    {
      using (var client =
       new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(this.options.Key))
       {
         Endpoint = this.options.Endpoint
       })
      {
        var project = await client.GetProjectAsync(options.ProjectId);

        if (options.IsInteractive)
        {
          Console.WriteLine($"Irreversibly delete all images and tags from Custom Vision project '{project.Name}'? (yes/no)");
          var res = Console.ReadLine();
          if (res.ToLowerInvariant() != "yes")
            return;
        }

        Console.WriteLine();
        try
        {
          Console.WriteLine("Deleting images...");
          await client.DeleteImagesAsync(options.ProjectId);
          Console.WriteLine("Deleting tags...");
          var tags = await client.GetTagsAsync(options.ProjectId);
          foreach (var tag in tags)
          {
            Console.WriteLine($" - {tag.Name}");
            await client.DeleteTagAsync(options.ProjectId, tag.Id);
          }
          Console.WriteLine("Done!");
        }
        catch (CustomVisionErrorException ex) { Console.WriteLine($"Error: {ex.DetailedMessage()}"); }
      }
    }
  }
}
