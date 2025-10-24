using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using System.IO;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

namespace WEBAPI2.ML
{
    public class SentimentModel
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<SentimentData, SentimentPrediction> _engine;
        private const string ModelPath = "MLModels/sentimentModel.zip";
        private const string OnnxPath = "MLModels/sentimentModel.onnx";

        public SentimentModel()
        {
            _mlContext = new MLContext();
            Directory.CreateDirectory("MLModels");


            if (File.Exists(ModelPath))
            {
                var trainedModel = _mlContext.Model.Load(ModelPath, out var modelSchema);
                _engine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(trainedModel);

                // Kiểm tra và tạo ONNX nếu chưa có
                if (!File.Exists(OnnxPath))
                {
                    ExportToOnnx(trainedModel);
                }
            }
            else
            {
                _engine = TrainAndSaveModel();
            }

            // Test nhanh khi khởi động
            var testData = new SentimentData { Text = "bộ phim này thật sự là quá tuyệt vời" };
            var result = _engine.Predict(testData);
            Console.WriteLine($" Prediction: {(result.Prediction ? "Positive" : "Negative")}, " +
                              $"Probability: {result.Probability:P2}, Score: {result.Score:F2}");
        }

        private PredictionEngine<SentimentData, SentimentPrediction> TrainAndSaveModel()
        {
            // 1️⃣ Nạp dữ liệu
            IDataView dataView = _mlContext.Data.LoadFromTextFile<SentimentData>(
                path: "Data/train.tsv",
                hasHeader: true,
                separatorChar: '\t');

            // 2️⃣ Tạo pipeline
            //     var pipeline = _mlContext.Transforms.Text.NormalizeText("NormalizedText", "Text")
            //         .Append(_mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "NormalizedText"))
            //         .Append(_mlContext.Transforms.Text.ProduceNgrams("Features", "Tokens"))
            //         .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
            //   labelColumnName: "Label",
            //   featureColumnName: "Features"));

            var pipeline = _mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "Text")
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("TokensKey", "Tokens"))
            .Append(_mlContext.Transforms.Text.ProduceNgrams("Features", "TokensKey",
                ngramLength: 2,
                useAllLengths: true,
                weighting: Microsoft.ML.Transforms.Text.NgramExtractingEstimator.WeightingCriteria.TfIdf))
            .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                labelColumnName: "Label",
                featureColumnName: "Features"));

            // 3️⃣ Huấn luyện model
            var model = pipeline.Fit(dataView);

            // 4️⃣ Lưu model ML.NET (.zip)
            _mlContext.Model.Save(model, dataView.Schema, ModelPath);
            Console.WriteLine($" Model đã được lưu tại: {ModelPath}");

            // 5️⃣ (Tùy chọn) Xuất sang ONNX
            try
            {
                using var stream = File.Create(OnnxPath);
                _mlContext.Model.ConvertToOnnx(model, dataView, stream);
                Console.WriteLine($" ONNX model exported: {OnnxPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Không thể xuất ONNX: {ex.Message}");
            }

            // 6️⃣ Tạo engine dự đoán
            return _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
        }

        private PredictionEngine<SentimentData, SentimentPrediction> LoadModel()
        {
            ITransformer trainedModel = _mlContext.Model.Load(ModelPath, out var modelSchema);
            return _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(trainedModel);
        }

        public SentimentPrediction Predict(string text)
        {
            var input = new SentimentData { Text = text };
            return _engine.Predict(input);
        }
        private void ExportToOnnx(ITransformer model)
        {
            try
            {
                // Load lại data để lấy schema cho ONNX export
                IDataView dataView = _mlContext.Data.LoadFromTextFile<SentimentData>(
                    path: "Data/train.tsv",
                    hasHeader: true,
                    separatorChar: '\t');

                using var stream = File.Create(OnnxPath);
                _mlContext.Model.ConvertToOnnx(model, dataView, stream);
                Console.WriteLine($"✅ ONNX model exported: {OnnxPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Không thể xuất ONNX: {ex.Message}");
            }
        }
    }
}