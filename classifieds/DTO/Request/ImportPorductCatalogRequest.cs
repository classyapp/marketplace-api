using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class ImportPorductCatalogRequest : BaseRequestDto
    {
        public string ProfileId { get; set; }
        public int CatalogTemplateType { get; set; }
        public bool OverwriteListings { get; set; }
        public bool UpdateImages { get; set; }
    }

    public class ImportPorductCatalogValidator : AbstractValidator<ImportPorductCatalogRequest>
    {
        public ImportPorductCatalogValidator()
        {
        }
    }
}