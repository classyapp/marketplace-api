using Classy.Models.Search;

namespace Classy.Interfaces.Search
{
    public interface IProfileSearchProvider
    {
        SearchResults<ProfileIndexDto> Search(string query, int amount = 25, int page = 1);
        SearchResults<ProfileIndexDto> Search(string query, string appId, int amount = 25, int page = 1);
    }
}