using CommandLine;
using System;
using System.Threading.Tasks;

namespace azcv_classifier_util
{
    class Options
    {
        [Option('p', "projectid", HelpText = "Project ID.", Required = true)]
        public string ProjectId { get; set; }
    }

    [Verb("tag", HelpText = "Add file contents to the index.")]
    class TagOptions
    {
        [Option('f', "folder", HelpText = "Folder where the training images will be loaded from.", Required = true)]
        public string Folder { get; set; }

        [Option('c', "croparea", HelpText = "Area to consider for classification training. Expected format is: top,left,width,height.", Required = false)]
        public string CropArea { get; set; }
    }

    [Verb("train", HelpText = "Trains the model.")]
    class TrainOptions
    {        
    }

    [Verb("publish", HelpText = "Publish the model.")]
    class PublishOptions
    {
    }


    [Verb("test", HelpText = "Test a local image")]
    class TestOptions
    {
        [Value(0, MetaName = "image file",
            HelpText = "Image file to run against the published model.",
            Required = true)]
        public string ImageFile { get; set; }
    }


    class Program
    {
        public const string VERSION = "1.0";

        static async Task Main(string[] args)
        {
            Console.WriteLine($"Azure Custom Vision Classifier Utility v{Program.VERSION}");

            var res = CommandLine.Parser.Default.ParseArguments<TagOptions, TrainOptions, PublishOptions, AugmentOptions>(args)
                .MapResult(
                  (TagOptions opts) => throw new NotImplementedException(),
                  (TrainOptions opts) => throw new NotImplementedException(),
                  (PublishOptions opts) => throw new NotImplementedException(),
                  (AugmentOptions opts) => RunAugment(opts),
                  (TestOptions opts) => throw new NotImplementedException(),
                  errs => 1);
        }

        private static object RunAugment(AugmentOptions opts)
        {
            return (new Augment(opts)).Process();
        }
    }
}
