using Microsoft.AspNetCore.Mvc;
using WEBAPI4;

namespace WEBAPI2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly ILogger<AIController> _logger;

        public AIController(IAIService aiService, ILogger<AIController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                    return BadRequest(new { error = "Message cannot be empty" });

                var response = await _aiService.ChatAsync(request.Message, request.History);

                return Ok(new
                {
                    success = true,
                    message = response,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Chat endpoint");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }



        [HttpPost("translate")]
        public async Task<IActionResult> Translate([FromBody] TranslateRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Text))
                    return BadRequest(new { error = "Text cannot be empty" });

                if (string.IsNullOrWhiteSpace(request.TargetLanguage))
                    return BadRequest(new { error = "Target language cannot be empty" });

                var translation = await _aiService.TranslateAsync(request.Text, request.TargetLanguage);

                return Ok(new
                {
                    success = true,
                    originalText = request.Text,
                    translatedText = translation,
                    targetLanguage = request.TargetLanguage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Translate endpoint");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatMessageDto>? History { get; set; }
    }

    public class TextRequest
    {
        public string Text { get; set; } = string.Empty;
    }

    public class TranslateRequest
    {
        public string Text { get; set; } = string.Empty;
        public string TargetLanguage { get; set; } = string.Empty;
    }
}