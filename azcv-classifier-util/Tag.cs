using CommandLine;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
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

        [Option('c', "croparea", HelpText = "Area to consider for classification training. Expected format is: x,y,width,height.", Required = false)]
        public string CropArea { get; set; }

        [Option('i', "interactive", HelpText = "Prompt for user confirmation before uploading and tagging.", Required = false, Default = true)]
        public bool IsInteractive { get; set; }
    }

    class Tag
    {
        const int BATCH_SIZE = 4;

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
            Console.WriteLine();
            foreach (var directory in tagDirectories)
            {
                var files = Directory.GetFiles(directory);
                totalFileCount += files.Length;
                Console.WriteLine($"  - {directory.Split(Path.DirectorySeparatorChar).Last()}, {files.Length} image(s)");
            }
            Console.WriteLine($"{totalFileCount} image(s) in total");
            if (totalFileCount == 0)
                Console.WriteLine("Nothing to do...");
            else if (Options.IsInteractive)
            {
                Console.WriteLine();
                Console.WriteLine("Press CTRL+C to abort or ENTER to continue...");
                Console.ReadLine();
            }
            Console.WriteLine();
            if (totalFileCount == 0)
                return new object();

            // Go through each subdirectory
            var fileCountFormat = string.Empty;
            for (int i = 0; i < totalFileCount.ToString().Length; i++)
                fileCountFormat += "0";
            var fileCount = 1;
            foreach (var directory in tagDirectories)
            {
                var targetTagName = directory.Split(Path.DirectorySeparatorChar).Last();
                Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models.Tag targetTag = null;

                try
                {
                    targetTag = trainingApi.GetTags(Options.ProjectId).SingleOrDefault(t => t.Name == targetTagName);

                    // If a tag with this name does not already exist, create it
                    if (targetTag == null)
                    {
                        if (targetTagName.ToLowerInvariant() == "negative")
                        {
                            targetTag = trainingApi.CreateTag(Options.ProjectId, targetTagName, type: "Negative");
                        }
                        else
                            targetTag = trainingApi.CreateTag(Options.ProjectId, targetTagName);
                        Console.WriteLine($"Created tag '{targetTagName}' (id={targetTag.Id})");
                    }
                    else
                        Console.WriteLine($"Reusing existing tag '{targetTag.Name}' (id={targetTag.Id})");
                }
                catch (CustomVisionErrorException ex)
                {
                    Console.WriteLine($"Error: {ex.DetailedMessage()}");
                    return new object();
                }

                if (cropRect == Rectangle.Empty)
                {
                    Console.WriteLine($"Batch uploading content from {directory}....");
                    var imageFiles = Directory.GetFiles(directory).Select(img => new ImageFileCreateEntry(Path.GetFileName(img), File.ReadAllBytes(img))).ToList();
                    try
                    {
                        while (imageFiles.Any())
                        {
                            var batchSize = Math.Min(imageFiles.Count, BATCH_SIZE);
                            var ifcb = new ImageFileCreateBatch(imageFiles.Take(batchSize).ToList(), new List<Guid>() { targetTag.Id });
                            var res = trainingApi.CreateImagesFromFiles(Options.ProjectId, ifcb);
                            var errorCount = PrintBatchErrors(res);
                            imageFiles.RemoveRange(0, batchSize);
                            fileCount += batchSize;
                            Console.WriteLine($" {batchSize} files uploaded with {errorCount} errors ({fileCount} total)");
                        }
                    }
                    catch (CustomVisionErrorException ex) { Console.WriteLine($"Error: {ex.DetailedMessage()}"); }
                    fileCount += imageFiles.Count;
                }
                else
                {
                    foreach (var file in Directory.GetFiles(directory))
                    {
                        Console.Write($"[{fileCount++.ToString(fileCountFormat)}/{totalFileCount}] Processing {Path.Combine(directory, file)}...");
                        using (var image = SixLabors.ImageSharp.Image.Load(file))
                        {
                            image.Mutate(x => x.Crop(cropRect));
                            using (var stream = new MemoryStream())
                            {
                                //image.Save(stream, new JpegEncoder() { Quality = 100 });
                                image.Save(stream, new PngEncoder());
                                image.Save(@"c:\temp\test.png", new PngEncoder());

                                try
                                {
                                    var res = trainingApi.CreateImagesFromData(Options.ProjectId, stream, new List<Guid>() { targetTag.Id });
                                    var errorCount = PrintBatchErrors(res);
                                }
                                catch (CustomVisionErrorException ex) { Console.WriteLine($"Error: {ex.DetailedMessage()}"); }
                            }
                        }
                    }
                }
            }

            return new object();
        }

        private int PrintBatchErrors(ImageCreateSummary res)
        {
            int errorCount = 0;
            if (res.IsBatchSuccessful)
                return errorCount;

            foreach (var r in res.Images.Where(i => !i.Status.StartsWith("OK", StringComparison.InvariantCultureIgnoreCase)))
            {
                Console.WriteLine($"{r.Image?.Id}: {r.Status}");
                errorCount++;
            }
            return errorCount;
        }
    }
}
