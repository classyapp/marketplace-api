using Classy.Models.Search;

namespace Classy.Interfaces.Search
{
    public interface IListingSearchProvider
    {
        void Index(ListingIndexDto[] listingDtos);

        SearchResults<ListingIndexDto> Search(string query);
    }
}