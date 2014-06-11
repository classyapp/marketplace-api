using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classy.Models;

namespace Classy.Repository
{
    public interface ITempMediaFileRepository
    {
        string Save(TempMediaFile mediaFile);
        void Delete(string appId, string fileId);
        TempMediaFile Get(string appId, string fileId);
    }
}
