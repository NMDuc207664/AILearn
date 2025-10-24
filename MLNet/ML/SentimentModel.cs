using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;

namespace WEBAPI2.ML
{
    public class SentimentModel
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<ReviewInput, ReviewPrediction> _engine;
        public SentimentModel()
        {
            _mlContext = new MLContext();

            var allData = _mlContext.Data.LoadFromTextFile<ReviewInput>(
               path: "Data/train.tsv",
               hasHeader: true,
               separatorChar: '\t'
           );


            var trainTestSplit = _mlContext.Data.TrainTestSplit(allData, testFraction: 0.2);
            var trainData = trainTestSplit.TrainSet;

            var testData = trainTestSplit.TestSet;




            var pipeline = _mlContext.Transforms.Text.FeaturizeText("TextFeaturized", nameof(ReviewInput.Text))

                .Append(_mlContext.Transforms.NormalizeMinMax("TextFeaturized"))
                .Append(_mlContext.Transforms.Concatenate("Features", "TextFeaturized"))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());


            var models = pipeline.Fit(trainData);


            _engine = _mlContext.Model.CreatePredictionEngine<ReviewInput, ReviewPrediction>(models);




            var predictions = models.Transform(testData);

            var metrics = _mlContext.BinaryClassification.Evaluate(predictions);




        }
        public ReviewPrediction Predict(string text)
        {
            var input = new ReviewInput { Text = text };
            return _engine.Predict(input);
        }
    }
}