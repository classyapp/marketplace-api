using classy.Extensions;

namespace Classy.CatalogImportWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            // set up container
            var _container = new Funq.Container();
            _container.WireUp();

            // import jobs
            var importer = new Importer(_container);
            importer.Run();
        }
    }
}
