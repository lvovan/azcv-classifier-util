using CommandLine;
using System;
using System.Threading.Tasks;
using System.Reflection;

namespace azcv_classifier_util
{
  class Program
  {
    public const string CONFIRM_PHRASE = "CTRL+C to abort or ENTER to continue...";

    static async Task Main(string[] args)
    {
      try
      {
        await Parser.Default.ParseArguments<TagVerb, TrainVerb, PublishVerb, AugmentVerb, TestVerb, ResetVerb, MigrateVerb, BackupVerb>(args)
          .MapResult(
            (TagVerb opts) => new CreateTag(opts).ProcessAsync(),
            (TrainVerb opts) => new Train(opts).ProcessAsync(),
            (PublishVerb opts) =>
            {
              Console.WriteLine("Not implemented");
              return Task.CompletedTask;
            },
            (AugmentVerb opts) => new Augment(opts).ProcessAsync(),
            (TestVerb opts) =>
            {
              Console.WriteLine("Not implemented");
              return Task.CompletedTask;
            },
            (ResetVerb opts) => new Reset(opts).ProcessAsync(),
            (MigrateVerb opts) => new Migrate(opts).ProcessAsync(),
            (BackupVerb opts) => new Backup(opts).ProcessAsync(),
            errs => Task.FromResult(1));
      }
      catch (Exception ex)
      {
        var tEx = ex as Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models.CustomVisionErrorException;
        var pEx = ex as Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.CustomVisionErrorException;

        Console.WriteLine(ex.Message);

        if (tEx != null)
        {
          Console.WriteLine(tEx.Body?.Message);
        }

        if (pEx != null)
        {
          Console.WriteLine(pEx.Body?.Message);
        }
      }
    }
  }
}
