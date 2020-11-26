using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace azcv_classifier_util
{
    static class CvService
    {
        public static CustomVisionTrainingClient AuthenticateTraining(string endpoint, string trainingKey, string predictionKey)
        {
            // Create the Api, passing in the training key
            var trainingApi = new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(trainingKey))
            {
                Endpoint = endpoint
            };
            return trainingApi;
        }

        public static CustomVisionPredictionClient AuthenticatePrediction(string endpoint, string predictionKey)
        {
            // Create a prediction endpoint, passing in the obtained prediction key
            var predictionApi = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = endpoint
            };
            return predictionApi;
        }

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
