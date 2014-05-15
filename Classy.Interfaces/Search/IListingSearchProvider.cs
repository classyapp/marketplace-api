using Classy.Models.Search;

namespace Classy.Interfaces.Search
{
    public interface IListingSearchProvider
    {
        void Index(ListingIndexDto[] listingDtos, string appId);

        SearchResults<ListingIndexDto> Search(string query, string appId, int amount = 25, int page = 1);
    }
}