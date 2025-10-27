using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chatbot.Model;
using Chatbot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnnxPredictionController : ControllerBase
    {
        private readonly IOnnxPredictionService _onnxService;

        public OnnxPredictionController(IOnnxPredictionService onnxService)
        {
            _onnxService = onnxService;
        }
        [HttpPost("predict-sentiment-ONNX")]
        public async Task<IActionResult> PredictSentiment([FromBody] TextRequest request)
        {

            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest(new { error = "Text cannot be empty" });

            var result = await _onnxService.PredictSentimentAsync(request.Text);

            return Ok(new
            {
                success = true,
                data = new
                {
                    text = request.Text,
                    sentiment = result.Label,
                    isPositive = result.IsPositive,
                    confidence = result.Confidence,
                    score = result.Score
                }
            });

        }
    }
}