using Microsoft.ML.Data;

namespace WEBAPI2.ML
{
    public class ReviewInput

    {

        [LoadColumn(0)]

        public string Text { get; set; }

        [LoadColumn(1), ColumnName("Label")]

        public bool Label { get; set; }

    }
}