using Classy.Interfaces.Search;
using classy.Manager.Search;
using Funq;

namespace Classy.Tests.Framework
{
    public interface ITestBase
    {
        Container Container { get; set; }
    }

    public class TestBase : ITestBase
    {
        public Container Container { get; set; }

        public TestBase()
        {
            Container = new Container();
        }
    }

    public class SearchProviderTestBase : TestBase
    {
        public readonly IListingSearchProvider ListingSearchProvider;

        public SearchProviderTestBase()
        {
            Container.Register<ISearchClientFactory>(_ => new SearchClientFactory());
            Container.Register<IListingSearchProvider>(
                c => new ListingSearchProvider(c.TryResolve<ISearchClientFactory>()));

            ListingSearchProvider = Container.Resolve<IListingSearchProvider>();
        }
    }
}