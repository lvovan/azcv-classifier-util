using System;
using CommandLine;

namespace azcv_classifier_util
{
  internal abstract class CommonOptions
  {
    [Option('p', "projectid", HelpText = "Project ID.", Required = true)]
    public Guid ProjectId { get; set; }

    [Option('e', "endpoint", HelpText = "Custom Vision endpoint.", Required = true)]
    public string Endpoint { get; set; }

    [Option('k', "key", HelpText = "Custom Vision endpoint key.", Required = true)]
    public string Key { get; set; }
  }

  [Verb("publish", HelpText = "Publish the model.")]
  class PublishVerb : CommonOptions
  {
  }

  [Verb("test", HelpText = "Test a local image")]
  class TestVerb : CommonOptions
  {
    [Value(0, MetaName = "image file",
        HelpText = "Image file to run against the published model.",
        Required = true)]
    public string ImageFile { get; set; }
  }
}
