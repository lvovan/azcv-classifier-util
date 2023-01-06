using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Newtonsoft.Json;

namespace azcv_classifier_util
{
  [Verb("backup", HelpText = "Create a backup of a project locally.")]
  class BackupVerb : CommonOptions
  {
    [Option("path", HelpText = "Local path where to store the backup.", Required = true)]
    public string Path { get; set; }

    [Usage(ApplicationAlias = "azcv-classifier-util")]
    public static IEnumerable<Example> Examples
    {
      get
      {
        yield return new Example("Backup to /tmp",
          new BackupVerb
          {
            Endpoint = "https://contoso.cognitiveservices.azure.com/",
            Key = "MyCVEndpointKey",
            ProjectId = Guid.Parse("20eff755-2f7a-48ef-b413-27ffac77cb78"),
            Path = "/tmp",
          });
      }
    }
  }

  class Backup
  {
    private BackupVerb options;

    public Backup(BackupVerb options)
    {
      this.options = options;
    }

    public async Task ProcessAsync()
    {
      using (var client =
        new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(this.options.Key))
        {
          Endpoint = this.options.Endpoint
        })
      {
        var rootPath = Path.GetFullPath(Path.Join(this.options.Path, this.options.ProjectId.ToString()));

        try
        {
          if (Directory.Exists(rootPath))
          {
            Directory.Delete(rootPath, true);
          }
          Directory.CreateDirectory(rootPath);

          var projectMeta = await client.ExportProjectAsync(this.options.ProjectId);
          var project = await client.GetProjectAsync(this.options.ProjectId);
          var tags = await client.GetTagsAsync(this.options.ProjectId);
          var images = await client.GetImagesAsync(this.options.ProjectId, take: projectMeta.ImageCount, skip: 0);

          var projectDTO = new ProjectBackup()
          {
            Id = project.Id,
            Name = project.Name,
            description = project.Description,
            Images = images.Select(i => new ImageBackup()
            {
              Id = i.Id,
              Width = i.Width,
              Height = i.Height,
              Regions = i.Regions.Select(r => new RegionBackup()
              {
                Height = r.Height,
                Left = r.Left,
                TagId = r.TagId,
                Top = r.Top,
                Width = r.Width
              }).ToList(),
              Uri = i.OriginalImageUri
            }).ToList(),
            Tags = tags,
          };

          var json = JsonConvert.SerializeObject(projectDTO, Formatting.Indented);
          System.IO.File.WriteAllText(Path.Join(rootPath, "project.json"), json);

          using (var httpClient = new HttpClient())
          {
            Func<string, ImageBackup, Task> downloadFile = async (rootPath, image) =>
            {
              using (var s = await httpClient.GetStreamAsync(image.Uri))
              {
                string imageType;
                // We need a seekable stream
                using (var ms = new MemoryStream())
                {
                  await s.CopyToAsync(ms);
                  imageType = ImageTypeHelper.GetKnownFileType(ms).ToString();
                }
                var path = Path.Join(rootPath, $"{image.Id}.{imageType.ToLowerInvariant()}");
                using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite))
                {
                  await s.CopyToAsync(fs);
                }
                image.Path = path;
              }
            };

            await Task.WhenAll(projectDTO.Images.Select(i => downloadFile(rootPath, i)));
          }

          Console.WriteLine($"The project {project.Name} ({project.Id}) was successfully exported to local folder {rootPath}");
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Backup failed: {ex.Message}");
        }
      }
    }
  }
}
