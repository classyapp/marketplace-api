using Funq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.UtilRunner.Utilities.SitemapBuilders
{
    public class HomelabSitemapUtil : IUtility
    {
        private Container _container;

        public HomelabSitemapUtil(Container container)
        {
            _container = container;
        }

        public StatusCode Run(string[] args)
        {
            var generator = new HomelabSitemapGenerator(_container);
            generator.Generate("http://www.homelab.com", "C:\\");
            return StatusCode.Success;
        }
    }
}
