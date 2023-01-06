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
  }
}
