using Chatbot.Model;

namespace Chatbot.Services.Interfaces
{
    public interface IOnnxPredictionService
    {
        Task<SentimentResult> PredictSentimentAsync(string text);
        Task InitializeAsync();
    }
}