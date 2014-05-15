using System;
using Classy.Interfaces.Search;
using Classy.Models.Search;

namespace classy.Manager.Search
{
    public class ProfileSearchProvider : IProfileSearchProvider
    {
        private const string IndexName = "profiles";
        private readonly ISearchClientFactory _searchClientFactory;

        public ProfileSearchProvider(ISearchClientFactory searchClientFactory)
        {
            _searchClientFactory = searchClientFactory;
        }

        public SearchResults<ProfileIndexDto> Search(string query, string appId, int amount = 25, int page = 1)
        {
            var client = _searchClientFactory.GetClient(IndexName, appId);
            throw new NotImplementedException();
        }
    }
}