using Classy.Models.Search;
using Nest;

namespace Classy.Interfaces.Search
{
    public interface IListingSearchProvider
    {
        void Index(ListingIndexDto[] listingDtos);
    }

    public class ListingSearchProvider : IListingSearchProvider
    {
        private readonly IElasticClient _client;

        public ListingSearchProvider(ISearchClientFactory searchClientFactory)
        {
            _client = searchClientFactory.GetClient("listings");
        }

        public void Index(ListingIndexDto[] listingDtos)
        {
            _client.IndexMany(listingDtos);
        }
    }
}