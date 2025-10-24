using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEBAPI4
{
    public interface IAIService
    {
        Task<string> ChatAsync(string userMessage, List<ChatMessageDto>? history = null);
        Task<string> TranslateAsync(string text, string targetLanguage);
    }
}