using CommandLine;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace azcv_classifier_util
{

    [Verb("train", HelpText = "Trains the model.")]
    class TrainOptions : CvOptions
    {
        [Option('b', "budgethours", HelpText = "Maximum number of reserved training hours (Default: none for 'quick training').", Required = false)]
        public int? BudgetHours { get; set; }

        [Option('e', "email", HelpText = "Email address the service will send an email to when the training is completed.", Required = false)]
        public string Email { get; set; }
    }

    class Train
    {
        public TrainOptions Options { get; set; }

        public Train(TrainOptions options)
        {
            Options = options;
        }

        public object Process()
        {
            var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Program.SETTINGS_FILE));
            var trainingApi = CvService.AuthenticateTraining(settings.CvTrainingEndpoint, settings.CvTrainingKey, settings.CvPredictionKey);

            var project = trainingApi.GetProject(Options.ProjectId);

            try
            {
                trainingApi.TrainProject(Options.ProjectId, reservedBudgetInHours: Options.BudgetHours, notificationEmailAddress: Options.Email);

                Console.WriteLine();
                Console.WriteLine($"Training of project '{project.Name}' started!");
            }
            catch (CustomVisionErrorException ex)
            {
                Console.WriteLine($"Error: {ex.DetailedMessage()}");
            }
            return new object();
        }
    }
}
