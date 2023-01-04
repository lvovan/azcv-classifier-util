using Newtonsoft.Json.Linq;

namespace azcv_classifier_util
{
  public static class VisionExtension
  {
    public static string DetailedMessage(this Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models.CustomVisionErrorException cveex)
    {
      var json = cveex.Response.Content;
      dynamic jo = JObject.Parse(json);
      return jo.message;
    }

    public static string DetailedMessage(this Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.CustomVisionErrorException cveex)
    {
      var json = cveex.Response.Content;
      dynamic jo = JObject.Parse(json);
      return jo.message;
    }
  }
}
