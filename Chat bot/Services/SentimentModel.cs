using Microsoft.ML;

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

            var testData = new SentimentData { Text = "bộ phim này thật sự là quá tuyệt vời" };
            var result = _engine.Predict(testData);
            Console.WriteLine($" Prediction: {(result.Prediction ? "Positive" : "Negative")}, " +
                              $"Probability: {result.Probability:P2}, Score: {result.Score:F2}");
        }

        private PredictionEngine<SentimentData, SentimentPrediction> TrainAndSaveModel()
        {
            IDataView dataView = _mlContext.Data.LoadFromTextFile<SentimentData>(
                path: "Data/train.tsv",
                hasHeader: true,
                separatorChar: '\t');


            var pipeline = _mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "Text")
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("TokensKey", "Tokens"))
            .Append(_mlContext.Transforms.Text.ProduceNgrams("Features", "TokensKey",
                ngramLength: 2,
                useAllLengths: true,
                weighting: Microsoft.ML.Transforms.Text.NgramExtractingEstimator.WeightingCriteria.TfIdf))
            .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                labelColumnName: "Label",
                featureColumnName: "Features"));

            var model = pipeline.Fit(dataView);

            _mlContext.Model.Save(model, dataView.Schema, ModelPath);
            Console.WriteLine($" Model đã được lưu tại: {ModelPath}");

            using var stream = File.Create(OnnxPath);
            _mlContext.Model.ConvertToOnnx(model, dataView, stream);

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

            IDataView dataView = _mlContext.Data.LoadFromTextFile<SentimentData>(
                path: "Data/train.tsv",
                hasHeader: true,
                separatorChar: '\t');

            using var stream = File.Create(OnnxPath);
            _mlContext.Model.ConvertToOnnx(model, dataView, stream);


        }
    }
}