using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using classy.Extensions;
using Classy.Repository;

namespace Classy.CatalogImportWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            // set up container
            Funq.Container _container = new Funq.Container();
            _container.WireUp();

            // get Jobs repository
            IJobRepository _jobRepository = _container.Resolve<IJobRepository>();
        }
    }
}
