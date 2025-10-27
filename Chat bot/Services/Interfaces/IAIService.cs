using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chatbot.Model;

namespace WEBAPI4
{
    public interface IAIService
    {
        Task<string> ExplainSentimentAsync(string originalText, SentimentResult prediction);
    }
}