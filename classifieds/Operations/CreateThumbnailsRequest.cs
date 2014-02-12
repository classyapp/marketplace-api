using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace classy.Operations
{
    public class CreateThumbnailsRequest
    {
        public string AppId { get; private set; }
        public string ListingId { get; private set; }
        public string MediaKey { get; private set; }
        public string ContentType { get; private set; }
        public byte[] Content { get; private set; }

        public CreateThumbnailsRequest(string appId, string listingId, string mediaKey, string contentType, byte[] content)
        {
            AppId = appId;
            ListingId = listingId;
            MediaKey = mediaKey;
            ContentType = contentType;
            Content = content;
        }
    }
}