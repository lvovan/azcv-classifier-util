using CommandLine;
using System;
using System.Threading.Tasks;

namespace azcv_classifier_util
{
    class CvOptions
    {
        [Option('p', "projectid", HelpText = "Project ID.", Required = true)]
        public Guid ProjectId { get; set; }
    }

    [Verb("publish", HelpText = "Publish the model.")]
    class PublishOptions: CvOptions
    {
    }


    [Verb("test", HelpText = "Test a local image")]
    class TestOptions: CvOptions
    {
        [Value(0, MetaName = "image file",
            HelpText = "Image file to run against the published model.",
            Required = true)]
        public string ImageFile { get; set; }
    }


    class Program
    {
        public const string VERSION = "1.0";
        public const string SETTINGS_FILE = "settings.json";

        static async Task Main(string[] args)
        {
            Console.WriteLine($"Azure Custom Vision Classifier Utility v{Program.VERSION}");
            Console.WriteLine();

            var res = CommandLine.Parser.Default.ParseArguments<TagOptions, TrainOptions, PublishOptions, AugmentOptions, TestOptions, ResetOptions>(args)
                .MapResult(
                  (TagOptions opts) => RunTag(opts),
                  (TrainOptions opts) => RunTrain(opts),
                  (PublishOptions opts) => throw new NotImplementedException(),
                  (AugmentOptions opts) => RunAugment(opts),
                  (TestOptions opts) => throw new NotImplementedException(),
                  (ResetOptions opts) => RunReset(opts),
                  errs => 1);

            Console.WriteLine();
        }

        private static object RunTag(TagOptions opts)
        {
            return (new Tag(opts)).Process();
        }

        private static object RunAugment(AugmentOptions opts)
        {
            return (new Augment(opts)).Process();
        }

        private static object RunReset(ResetOptions opts)
        {
            return (new Reset(opts)).Process();
        }

        private static object RunTrain(TrainOptions opts)
        {
            return (new Train(opts)).Process();
        }
    }
}
