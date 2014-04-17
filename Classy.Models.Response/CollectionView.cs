using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Classy.Models.Response
{
    public class CollectionView
    {
        public string Id { get; set; }
        public string ProfileId { get; set; }
        public ProfileView Profile { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsPublic { get; set; }
        public IList<IncludedListingView> IncludedListings { get; set; }
        public IList<ListingView> Listings { get; set; }
        public IList<string> Collaborators { get; set; }
        public IList<string> PermittedViewers { get; set; }
        public int CommentCount { get; set; }
        public int ViewCount { get; set; }
        public int FavoriteCount { get; set; }
        public IList<MediaThumbnailView> Thumbnails { get; set; }
        public IList<string> CoverPhotos { get; set; }
        //
        public IList<CommentView> Comments { get; set; }
        public string DefaultCulture { get; set; }
    }
}
