using Classy.Tests.Framework;
using NUnit.Framework;

namespace Classy.Tests.Search
{
    //[TestFixture]
    public class ListingSearchProviderTests : SearchProviderTestBase
    {

        // TODO: need an infrastructure for testing the index properly
        //[Test]
        public void Search_WordInTitle_ReturnRelevantDocuments()
        {
            var results = ListingSearchProvider.Search("classic", "v1.0");

            Assert.NotNull(results);
            Assert.That(results.TotalResults, Is.Not.EqualTo(0));
            Assert.IsNotEmpty(results.Results);
        }
    }
}