using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace classy.DTO.Request
{
    public class ImportProductCatalogRequest : BaseRequestDto
    {
        public string ProfileId { get; set; }
        public string AWSFileKey { get; set; }
        public bool OverwriteListings { get; set; }
        public bool UpdateImages { get; set; }
        public int InputFormat { get; set; }

        public ImportProductCatalogRequest(string profileId, string AWSKey, bool overwriteListings, bool updateImages, int inputType)
        {
            this.ProfileId = profileId;
            this.AWSFileKey = AWSKey;
            this.OverwriteListings = overwriteListings;
            this.UpdateImages = updateImages;
            this.InputFormat = inputType;
        }
    }
}