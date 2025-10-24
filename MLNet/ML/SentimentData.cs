
using Microsoft.ML.Data;

namespace WEBAPI2.ML
{
    public class SentimentData

    {

        [LoadColumn(0)]

        public string Text { get; set; }

        [LoadColumn(1), ColumnName("Label")]

        public bool Sentiment { get; set; }

    }
}