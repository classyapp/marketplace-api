using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.UtilRunner.Utilities.SitemapBuilders
{
    public abstract class ClassySitemapGenerator : BaseSitemapIndexGenerator
    {
        public abstract void GenerateStaticNodes();
        public abstract void GenerateListingNodes();
        public abstract void GenerateProfessionalNodes();

        protected override void GenerateUrlNodes()
        {
            //GenerateStaticNodes();
            //GenerateListingNodes();
            GenerateProfessionalNodes();
        }
    }
}
