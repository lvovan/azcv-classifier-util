using CommandLine;
using CommandLine.Text;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace azcv_classifier_util
{
  [Verb("train", HelpText = "Trains the model.")]
  class TrainVerb : CommonOptions
  {
    [Option('b', "budgethours", HelpText = "Maximum number of reserved training hours (Default: none for 'quick training').", Required = false)]
    public int? BudgetHours { get; set; }

    [Option('e', "email", HelpText = "Email address the service will send an email to when the training is completed.", Required = false)]
    public string Email { get; set; }

    [Usage(ApplicationAlias = "azcv-classifier-util")]
    public static IEnumerable<Example> Examples
    {
      get
      {
        yield return new Example("Start training project with id 20eff755-2f7a-48ef-b413-27ffac77cb78",
          new TrainVerb
          {
            Endpoint = "https://contoso.cognitiveservices.azure.com/",
            Key = "MyCVEndpointKey",
            ProjectId = Guid.Parse("20eff755-2f7a-48ef-b413-27ffac77cb78")
          });
      }
    }
  }

  class Train
  {
    private TrainVerb options { get; set; }

    public Train(TrainVerb options)
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

        try
        {
          await client.TrainProjectAsync(options.ProjectId, reservedBudgetInHours: options.BudgetHours, notificationEmailAddress: options.Email);

          Console.WriteLine();
          Console.WriteLine($"Training of project '{project.Name}' started!");
        }
        catch (CustomVisionErrorException ex)
        {
          Console.WriteLine($"Error: {ex.DetailedMessage()}");
        }
      }
    }
  }
}
