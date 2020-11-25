using CommandLine;
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
            trainingApi.TrainProject(Options.ProjectId, reservedBudgetInHours: Options.BudgetHours);

            Console.WriteLine();
            Console.WriteLine($"Training of project '{project.Name}' started!");

            return new object();
        }
    }
}
