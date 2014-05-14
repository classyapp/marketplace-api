using Classy.Models;

namespace classy.Manager
{
    public interface IAppManager
    {
        App GetAppById(string appId);

        IndexingInfo GetIndexingInfo();
    }
}
