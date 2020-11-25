using System;
using System.Collections.Generic;
using System.Text;

namespace azcv_classifier_util
{
	class Settings
	{
		public string CvTrainingEndpoint { get; set; }
		public string CvTrainingKey { get; set; }
		public string CvPredictionEndpoint { get; set; }
		public string CvPredictionKey { get; set; }
	}
}
