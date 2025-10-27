namespace Chatbot.Model
{
    public class SentimentResult
    {
        public bool IsPositive { get; set; }
        public float Confidence { get; set; }
        public float Score { get; set; }
        public string Label => IsPositive ? "Positive" : "Negative";
    }
}