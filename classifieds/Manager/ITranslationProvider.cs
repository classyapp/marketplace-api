using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classy.Manager
{
    public interface ITranslationProvider
    {
        string DetectLanguage(string textToTranslate);
        IDictionary<string, string> Translate(string textToTranslate, string[] cultures);
    }
}
