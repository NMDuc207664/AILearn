using Chatbot.Model;
using Chatbot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WEBAPI2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IAIPipelineService _pipelineService;

        public AIController(IAIPipelineService pipelineService)
        {
            _pipelineService = pipelineService;
        }

        [HttpPost("analyze-sentiment")]
        public async Task<IActionResult> AnalyzeSentiment([FromBody] TextRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest(new { error = "Text cannot be empty" });


            var result = await _pipelineService.AnalyzeSentimentWithExplanationAsync(request.Text);

            return Ok(new
            {
                success = true,
                data = result
            });


        }



    }
}