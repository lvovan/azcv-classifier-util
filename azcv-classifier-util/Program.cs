using CommandLine;
using System;
using System.Threading.Tasks;

namespace azcv_classifier_util
{
    class Options
    {
        [Option('p', "projectid", Required = true, HelpText = "Project ID.")]
        public string ProjectId { get; set; }
    }

    [Verb("tag", HelpText = "Add file contents to the index.")]
    class ResetOptions
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
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
