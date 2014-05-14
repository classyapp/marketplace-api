using System;
using Classy.Interfaces.Search;
using Classy.Models.Search;
using Nest;

namespace classy.Manager.Search
{
    public class ProfileSearchProvider : IProfileSearchProvider
    {
        private readonly IElasticClient _client;

        public ProfileSearchProvider(ISearchClientFactory searchClientFactory)
        {
            _client = searchClientFactory.GetClient("profiles");
        }

        public SearchResults<ProfileIndexDto> Search(string query, int amount = 25, int page = 1)
        {
            throw new NotImplementedException();
        }
    }
}