using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Interfaces.Managers
{
    public interface ITranslationProvider
    {
        string DetectLanguage(string textToTranslate);
        IDictionary<string, string> Translate(string textToTranslate, string[] targetCultures);
        IDictionary<string, string> Translate(string textToTranslate, string sourceCulture, string[] targetCultures);
    }
}
