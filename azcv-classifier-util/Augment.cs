using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace azcv_classifier_util
{

    [Verb("augment", HelpText = "Generates augmentations of the input image (filenames are suffixed with -<number>).")]
    class AugmentOptions
    {
        [Value(0, MetaName = "image file",
            HelpText = "Image file to augment.",
            Required = true)]
        public string ImageFile { get; set; }

        [Option('c', "count", HelpText = "Number of augmentations to generate (default: 5)", Required = false, Default = 5)]
        public int Count { get; set; }

        [Option('r', "rotationrange", HelpText = "Maximum rotation range in degrees (default: 10)", Required = false, Default = 10)]
        public int RotationRange { get; set; }

        [Option('s', "resizerange", HelpText = "Maximum resize range in percent (default: 10)", Required = false, Default = 10)]
        public int ResizeRange { get; set; }

        [Option('b', "blurrange", HelpText = "Maximum blur range as a weight value (default: 2)", Required = false, Default = 2)]
        public int BlurRange { get; set; }
    }

    class Augment
    {
        public AugmentOptions Options { get; set; }

        public Augment(AugmentOptions options)
        {
            Options = options;
        }

        public object Process()
        {
            if (!File.Exists(Options.ImageFile))
            {
                Console.WriteLine($"File not found: {Options.ImageFile}");
                return null;
            }

            var rnd = new Random();
            Console.WriteLine($"Generating {Options.Count} augmented images:");
            Console.WriteLine($" - Rotation range: +/- {Options.RotationRange}°");
            Console.WriteLine($" - Resize range: +/- {Options.ResizeRange}%");
            Console.WriteLine($" - Blur range: {Options.BlurRange}");
            Console.WriteLine();
            for (int i = 0; i < Options.Count; i++)
            {
                using (var image = Image.Load(Options.ImageFile))
                {
                    var rotationAngle = rnd.Next(Options.RotationRange * -1, Options.RotationRange);
                    var resizePercent = rnd.Next(Options.ResizeRange * -1, Options.ResizeRange);
                    var resizeRatio = 1 + ((double)resizePercent / 100);
                    var blurValue = (Single)(rnd.NextDouble() * Options.BlurRange);

                    var newFilename = Path.GetFileNameWithoutExtension(Options.ImageFile) + $"-{i}";
                    var outputFilename = Path.Combine(Path.GetDirectoryName(Options.ImageFile), newFilename) + Path.GetExtension(Options.ImageFile);
                    Console.Write($"[{i.ToString("00")}/{Options.Count}] {outputFilename}: rotation={rotationAngle.ToString("00")}°, resize={resizePercent.ToString("00")}%, blur={blurValue.ToString("00.00")}....");

                    image.Mutate(x => x
                         .Resize(new ResizeOptions()
                         {
                             Mode = ResizeMode.Crop,
                             Size = new Size((int)(image.Width * resizeRatio), (int)(image.Height * resizeRatio))
                         })
                         .Rotate(rotationAngle)
                         .GaussianBlur(blurValue)); ;

                    image.Save(outputFilename); // Automatic encoder selected based on extension.
                    Console.WriteLine("generated");
                }
            }

            return new object();
        }
    }
}
