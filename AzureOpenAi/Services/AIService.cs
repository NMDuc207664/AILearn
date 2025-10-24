using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace WEBAPI4
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

        public async Task<string> ChatAsync(string userMessage, List<ChatMessageDto>? history = null)
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("Bạn là một trợ lý AI thông minh, hữu ích và thân thiện. Hãy trả lời bằng tiếng Việt.");

            // Thêm lịch sử chat (nếu có)
            if (history != null && history.Any())
            {
                foreach (var msg in history)
                {
                    chatHistory.AddUserMessage(msg.Content);
                }
            }

            chatHistory.AddUserMessage(userMessage);

            var response = await _chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings: new AzureOpenAIPromptExecutionSettings
                {
                    Temperature = 0.7,
                    MaxTokens = 2048,
                    TopP = 0.95
                }
            );

            return response.Content ?? "Xin lỗi, tôi không thể tạo phản hồi.";
        }



        public async Task<string> TranslateAsync(string text, string targetLanguage)
        {
            var prompt = $@"Dịch đoạn văn bản sau sang {targetLanguage}. Chỉ trả về bản dịch, không thêm giải thích:

{text}";

            return await ChatAsync(prompt);
        }

    }
}