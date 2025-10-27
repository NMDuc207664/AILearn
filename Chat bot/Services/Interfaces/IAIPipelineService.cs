using Chatbot.Model;

namespace Chatbot.Services.Interfaces
{
    public interface IAIPipelineService
    {
        Task<SentimentAnalysisResponse> AnalyzeSentimentWithExplanationAsync(string text);
        Task InitializeAsync();
    }
}