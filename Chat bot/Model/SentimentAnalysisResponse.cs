using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chatbot.Model
{
    public class SentimentAnalysisResponse
    {
        public string OriginalText { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
        public bool IsPositive { get; set; }
        public float Confidence { get; set; }
        public float Score { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }
}