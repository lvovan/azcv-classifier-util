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
    [Verb("reset", HelpText = "Resets a project by removing all tags and images.")]
    class ResetOptions: CvOptions
    {
        [Option('i', "interactive", HelpText = "Prompt for user confirmation before resetting the project.", Required = false, Default = true)]
        public bool IsInteractive { get; set; }
    }

    class Reset
    {
        public ResetOptions Options { get; set; }

        public Reset(ResetOptions options)
        {
            Options = options;
        }

        public object Process()
        {
            var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Program.SETTINGS_FILE));
            var trainingApi = CvService.AuthenticateTraining(settings.CvTrainingEndpoint, settings.CvTrainingKey, settings.CvPredictionKey);

            var project = trainingApi.GetProject(Options.ProjectId);

            if (Options.IsInteractive)
            {
                Console.WriteLine($"Irreversibly delete all images and tags from Custom Vision project '{project.Name}'? (yes/no)");
                var res = Console.ReadLine();
                if (res.ToLowerInvariant() != "yes")
                    return new object();
            }
            
            Console.WriteLine();
            try
            {
                Console.WriteLine("Deleting images...");
                trainingApi.DeleteImages(Options.ProjectId);
                Console.WriteLine("Deleting tags...");
                var tags = trainingApi.GetTags(Options.ProjectId);
                foreach (var tag in tags)
                {
                    Console.WriteLine($" - {tag.Name}");
                    trainingApi.DeleteTag(Options.ProjectId, tag.Id);
                }
                Console.WriteLine("Done!");
            }
            catch (CustomVisionErrorException ex) { Console.WriteLine($"Error: {ex.DetailedMessage()}"); }

            return new object();
        }
    }
}
