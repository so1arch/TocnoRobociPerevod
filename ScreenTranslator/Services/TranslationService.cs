using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace ScreenTranslator.Services
{
    public class TranslationService
    {
        private readonly HttpClient _httpClient = new();
        private readonly ConcurrentDictionary<string, string> _cache = new();

        public async Task<string> TranslateAsync(string text, string targetLang = "ru")
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            string cleanText = text.Trim();

            if (_cache.TryGetValue(cleanText, out var cachedTranslation))
            {
                return cachedTranslation;
            }

            try
            {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl={targetLang}&dt=t&q={HttpUtility.UrlEncode(cleanText)}";
                var response = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);
                
                var translationBuilder = new System.Text.StringBuilder();
                var segments = doc.RootElement[0];

                foreach (var segment in segments.EnumerateArray())
                {
                    translationBuilder.Append(segment[0].GetString());
                }

                string result = translationBuilder.ToString();
                if (!string.IsNullOrEmpty(result))
                {
                    _cache[cleanText] = result;
                    return result;
                }
            }
            catch
            {
                // В случае сбоя сети возвращаем оригинальный текст
            }

            return cleanText;
        }
    }
}