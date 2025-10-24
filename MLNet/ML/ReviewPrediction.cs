using Microsoft.ML.Data;

namespace WEBAPI2.ML
{
    public class ReviewPrediction

    {

        [ColumnName("PredictedLabel")]

        public bool Prediction { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }

    }
}