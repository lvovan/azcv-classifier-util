using CommandLine;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace azcv_classifier_util
{
    [Verb("tag", HelpText = "Add file contents to the index.")]
    class TagOptions : CvOptions
    {
        [Option('f', "folder", HelpText = "Base folder where the training images will be loaded from. Each folder within this base folder will be used as a tag name, and underlying files associated with that tag.", Required = true)]
        public string Folder { get; set; }

        [Option('a', "croparea", HelpText = "Area to consider for classification training. Expected format is: x,y,width,height.", Required = false)]
        public string CropArea { get; set; }

        [Option('i', "interactive", HelpText = "Prompt for user confirmation before uploading and tagging.", Required = false, Default = true)]
        public bool IsInteractive { get; set; }
    }

    class Tag
    {
        public TagOptions Options { get; set; }

        public Tag(TagOptions options)
        {
            Options = options;
        }

        public object Process()
        {
            if(!Directory.Exists(Options.Folder))
            {
                Console.WriteLine($"Folder not found: {Options.Folder}");
                return null;
            }

            var tagDirectories = Directory.GetDirectories(Options.Folder);
            if (tagDirectories.Length == 0)
            {
                Console.WriteLine($"Not subfolders found in {Options.Folder}. Subfolders are required to define tags and associated images.");
                return null;
            }

            var cropRect = new Rectangle();
            if(!string.IsNullOrEmpty(Options.CropArea))
            {
                var dims = Options.CropArea.Split(",");
                try
                {
                    var x = int.Parse(dims[0]);
                    var y = int.Parse(dims[1]);
                    var w = int.Parse(dims[2]);
                    var h = int.Parse(dims[3]);
                    cropRect = new Rectangle(x, y, w, h);
                }
                catch
                {
                    Console.WriteLine($"Unable to parse {Options.CropArea} into a rectangle. Required format is: x,y,width, height");
                    return new object();
                }
                Console.WriteLine($"Crop area is [{cropRect.Left},{cropRect.Top}], {cropRect.Width}x{cropRect.Height}");
            }
            else
            {
                Console.WriteLine("No cropping will be performed.");
            }

            var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Program.SETTINGS_FILE));
            var trainingApi = CvService.AuthenticateTraining(settings.CvTrainingEndpoint, settings.CvTrainingKey, settings.CvPredictionKey);

            // Explore subdirectories
            var totalFileCount = 0;
            Console.WriteLine($"Base folder: {Options.Folder}");
            foreach (var directory in tagDirectories)
            {
                var files = Directory.GetFiles(directory);
                totalFileCount += files.Length;
                Console.WriteLine($"  - {directory}, {files.Length} image(s)");
            }
            Console.WriteLine($"{totalFileCount} images in total");

            if (Options.IsInteractive)
            {
                Console.WriteLine();
                Console.WriteLine("Press CTRL+C to abort or ENTER to continue...");
                Console.ReadLine();
            }
            Console.WriteLine();

            // Go through each subdirectory
            var fileCountFormat = string.Empty;
            for (int i = 0; i < totalFileCount.ToString().Length; i++)
                fileCountFormat += "0";
            var fileCount = 1;
            foreach (var directory in tagDirectories)
            {
                var targetTagName = Path.GetDirectoryName(directory).Split(Path.DirectorySeparatorChar).Last();
                var targetTag = trainingApi.CreateTag(Options.ProjectId, targetTagName);
                if (targetTagName.ToLowerInvariant() == "negative")
                    targetTag.Type = "Negative";
                foreach (var file in Directory.GetFiles(directory))
                {                    
                    Console.Write($"[{fileCount++.ToString(fileCountFormat)}/{totalFileCount}] Processing {Path.Combine(directory, file)}...");

                    // If no cropping is required
                    if (cropRect == Rectangle.Empty)
                    {
                        using (var stream = new MemoryStream(File.ReadAllBytes(file)))
                        {
                            trainingApi.CreateImagesFromData(Options.ProjectId, stream, new List<Guid>() { targetTag.Id });
                        }
                    }

                    // If cropping is required
                    else
                    {
                        using (var image = Image.Load(file))
                        {
                            image.Mutate(x => x.Crop(cropRect));
                            using (var stream = new MemoryStream())
                            {
                                image.Save(stream, new JpegEncoder());
                                trainingApi.CreateImagesFromData(Options.ProjectId, stream, new List<Guid>() { targetTag.Id });
                                
                            }
                        }
                    }
                    Console.WriteLine("ok");
                }
            }

            return new object();
        }
    }
}
