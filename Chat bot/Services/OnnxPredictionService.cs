using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chatbot.Model;
using Chatbot.Services.Interfaces;
using Microsoft.ML;
using WEBAPI2.ML;

namespace Chatbot.Services
{
    public class OnnxPredictionService : IOnnxPredictionService
    {
        private readonly MLContext _mlContext;
        private PredictionEngine<SentimentData, SentimentPrediction>? _predictionEngine;
        private const string ModelPath = "MLModels/sentimentModel.zip";
        private const string OnnxPath = "MLModels/sentimentModel.onnx";
        private bool _isInitialized = false;
        public OnnxPredictionService()
        {
            _mlContext = new MLContext();
        }


        public async Task<SentimentResult> PredictSentimentAsync(string text)
        {
            if (!_isInitialized || _predictionEngine == null)
            {
                await InitializeAsync();
            }

            return await Task.Run(() =>
            {

                var input = new SentimentData { Text = text };
                var prediction = _predictionEngine!.Predict(input);

                return new SentimentResult
                {
                    IsPositive = prediction.Prediction,
                    Confidence = prediction.Probability,
                    Score = prediction.Score
                };


            });
        }


        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            await Task.Run(() =>
            {

                Directory.CreateDirectory("MLModels");

                if (File.Exists(ModelPath))
                {
                    var trainedModel = _mlContext.Model.Load(ModelPath, out var modelSchema);
                    _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(trainedModel);

                    // Kiểm tra và tạo ONNX nếu chưa có
                    if (!File.Exists(OnnxPath))
                    {
                        ExportToOnnx(trainedModel);
                    }
                }
                else
                {
                    _predictionEngine = TrainAndSaveModel();
                }

                _isInitialized = true;

                // Test prediction
                TestPrediction();
            }

            );
        }

        private PredictionEngine<SentimentData, SentimentPrediction> TrainAndSaveModel()
        {


            // Load data
            IDataView dataView = _mlContext.Data.LoadFromTextFile<SentimentData>(
                path: "Data/train.tsv",
                hasHeader: true,
                separatorChar: '\t');

            // Create pipeline
            var pipeline = _mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "Text")
                .Append(_mlContext.Transforms.Conversion.MapValueToKey("TokensKey", "Tokens"))
                .Append(_mlContext.Transforms.Text.ProduceNgrams("Features", "TokensKey",
                    ngramLength: 2,
                    useAllLengths: true,
                    weighting: Microsoft.ML.Transforms.Text.NgramExtractingEstimator.WeightingCriteria.TfIdf))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: "Label",
                    featureColumnName: "Features"));

            // Train model
            var model = pipeline.Fit(dataView);

            // Save ML.NET model
            _mlContext.Model.Save(model, dataView.Schema, ModelPath);

            // Export to ONNX
            ExportToOnnx(model);

            return _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
        }

        private void ExportToOnnx(ITransformer model)
        {
            IDataView dataView = _mlContext.Data.LoadFromTextFile<SentimentData>(
                    path: "Data/train.tsv",
                    hasHeader: true,
                    separatorChar: '\t');

            using var stream = File.Create(OnnxPath);
            _mlContext.Model.ConvertToOnnx(model, dataView, stream);


        }

        private void TestPrediction()
        {
            var testData = new SentimentData { Text = "bộ phim này thật sự là quá tuyệt vời" };
            _predictionEngine!.Predict(testData);
        }
    }
}