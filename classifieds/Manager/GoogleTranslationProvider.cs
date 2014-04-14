using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using ServiceStack.Text;

namespace classy.Manager
{
    public class DetectLanguageItem
    {
        public string Language { get; set; }
        public float Confidence { get; set; }
    }

    public class DetectLanguageDetections
    {
        public IList<DetectLanguageItem> Detections { get; set; }
    }

    public class GoogleTranslationProvider : Classy.Interfaces.Managers.ITranslationProvider
    {
        private string _apiKey;

        public GoogleTranslationProvider(string apiKey)
        {
            _apiKey = apiKey;
        }

        public string DetectLanguage(string textToTranslate)
        {
            var apiUrl = string.Concat("https://www.googleapis.com/language/translate/v2/detect?key=", _apiKey, "&q=", HttpUtility.UrlEncode(textToTranslate));
            var client = new WebClient();
            var responseJson = client.DownloadString(apiUrl);
            var responseObject = JsonSerializer.DeserializeFromString<DetectLanguageDetections>(responseJson);
            return responseObject.Detections.Count > 0 ? responseObject.Detections[0].Language : null;
        }

        public IDictionary<string, string> Translate(string textToTranslate, string[] targetCultures)
        {
            var sourceCulture = DetectLanguage(textToTranslate);
            return Translate(textToTranslate, sourceCulture, targetCultures);
        }

        public IDictionary<string, string> Translate(string textToTranslate, string sourceCulture, string[] targetCultures)
        {
            var apiUrl = string.Concat("https://www.googleapis.com/language/translate/v2?key=", _apiKey, "&q=", HttpUtility.UrlEncode(textToTranslate));
            var client = new WebClient();
            var 
        }
    }
}