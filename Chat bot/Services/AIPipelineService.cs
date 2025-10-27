using System.Diagnostics;
using Chatbot.Model;
using Chatbot.Services.Interfaces;
using WEBAPI4;


namespace Chatbot.Services
{
    public class AIPipelineService : IAIPipelineService
    {
        private readonly IOnnxPredictionService _onnxService;
        private readonly IAIService _semanticService;

        public AIPipelineService(
            IOnnxPredictionService onnxService,
            IAIService semanticService
          )
        {
            _onnxService = onnxService;
            _semanticService = semanticService;

        }

        public async Task InitializeAsync()
        {

            await _onnxService.InitializeAsync();

        }

        public async Task<SentimentAnalysisResponse> AnalyzeSentimentWithExplanationAsync(string text)
        {

            var prediction = await _onnxService.PredictSentimentAsync(text);
            var explanation = await _semanticService.ExplainSentimentAsync(text, prediction);

            var response = new SentimentAnalysisResponse
            {
                OriginalText = text,
                Sentiment = prediction.Label,
                IsPositive = prediction.IsPositive,
                Confidence = prediction.Confidence,
                Score = prediction.Score,
                Explanation = explanation,
            };


            return response;


        }
    }
}