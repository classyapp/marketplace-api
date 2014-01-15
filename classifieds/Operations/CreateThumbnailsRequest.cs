using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace classy.Operations
{
    public class CreateThumbnailsRequest
    {
        public string ListingId { get; private set; }
        public string AppId { get; private set; }
        public string MediaKey { get; private set; }

        public CreateThumbnailsRequest(string listingId, string appId, string mediaKey)
        {
            ListingId = listingId;
            AppId = appId;
            MediaKey = mediaKey;
        }
    }
}