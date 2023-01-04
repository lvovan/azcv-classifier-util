using System.Collections.Generic;
using System;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;

namespace azcv_classifier_util
{
  public class ProjectBackup
  {
    public Guid Id {get;set;}
    public string Name {get;set;}
    public string description {get;set;}
    public IList<ImageBackup> Images { get; set; }
    public IList<Tag> Tags { get; set; }
  }

  public class ImageBackup
  {
    public Guid Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Path { get; set; }
    public IList<RegionBackup> Regions { get; set; }
    [NonSerialized()]
    internal string Uri ;
  }

  public class RegionBackup {
    public Guid RegionId { get; }
    public Guid TagId { get; set; }
    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
  }
}
