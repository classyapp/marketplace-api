using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classy.Models;

namespace Classy.Repository
{
    public interface ITranslationRepository
    {
        Translation GetById(string appId, string profileId, string culture);
        string Insert(Translation translation);
        void Update(Translation translation);
        void Delete(string appId, string profileId, string culture);
        void DeleteAll(string appId, string profileId);
    }
}
