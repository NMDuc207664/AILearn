using Chatbot.Model;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace WEBAPI4.Interfaces
{
    public class AIService : IAIService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;


        public AIService(IConfiguration configuration)
        {


            var endpoint = configuration["AzureOpenAI:Endpoint"];
            var apiKey = configuration["AzureOpenAI:ApiKey"];
            var deploymentName = configuration["AzureOpenAI:DeploymentName"];

            var builder = Kernel.CreateBuilder();

            builder.AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                endpoint: endpoint,
                apiKey: apiKey
            );

            _kernel = builder.Build();
            _chatService = _kernel.GetRequiredService<IChatCompletionService>();


        }


        public async Task<string> ExplainSentimentAsync(string originalText, SentimentResult prediction)
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(@"Bạn là một chuyên gia phân tích cảm xúc văn bản. 
Nhiệm vụ của bạn là giải thích kết quả phân tích cảm xúc từ mô hình ML một cách dễ hiểu, chi tiết và hữu ích.
Hãy trả lời bằng tiếng Việt và giữ giọng điệu thân thiện, chuyên nghiệp.");

            var prompt = $@"Mô hình ML đã phân tích đoạn văn bản sau:

**Văn bản gốc:** ""{originalText}""

**Kết quả từ mô hình:**
- Cảm xúc: {prediction.Label} ({(prediction.IsPositive ? "Tích cực" : "Tiêu cực")})
- Độ tin cậy: {prediction.Confidence:P2}
- Điểm số: {prediction.Score:F4}

Hãy giải thích kết quả này bao gồm:
1. Tại sao mô hình đưa ra kết luận này?
2. Những từ khóa, cụm từ nào ảnh hưởng đến kết quả?
3. Độ tin cậy {prediction.Confidence:P2} có ý nghĩa gì?
4. Có điều gì đặc biệt trong văn bản này không?

Giữ câu trả lời ngắn gọn (3-5 câu), dễ hiểu và hữu ích.";

            chatHistory.AddUserMessage(prompt);


            var response = await _chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings: new AzureOpenAIPromptExecutionSettings
                {
                    Temperature = 0.7,//độ sáng tạo 
                    MaxTokens = 500,//độ dài câu
                    TopP = 0.95// cách lựa chọn từ ngữ
                }
            );

            return response.Content ?? "Không thể tạo giải thích.";


        }


    }
}