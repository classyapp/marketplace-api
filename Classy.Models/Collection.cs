﻿using System;
using System.Collections.Generic;
using Classy.Models.Attributes;

namespace Classy.Models
{
    public static class CollectionType
    {
        public static readonly string PhotoBook = "PhotoBook";
        public static readonly string Project = "Project";
        public static readonly string WebPhotos = "WebPhotos";
    }

    /// <summary>
    /// represents a collection of listings curated by a user, with the ability to collaborate with other users
    /// </summary>
    [MongoCollection(Name = "collections")]
    public class Collection : BaseObject, ITranslatable<Collection>
    {
        public Collection()
        {
            Hashtags = new List<string>();
        }
        /// <summary>
        /// the profile id of the collection owner
        /// </summary>
        public string ProfileId { get; set; }
        /// <summary>
        /// Collection types: photobook, project, webphotos
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// the title of the collection
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// the content of the collection - free text 
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// the category for the collection
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// when set to true, the collection will be publicly visible
        /// </summary>
        public bool IsPublic { get; set; }
        /// <summary>
        /// a flag indicating if the collection was submitted for editorial approval by the creator
        /// </summary>
        public bool SumittedForEditorialApproval { get; set; }
        /// <summary>
        /// ProfileId of the editor that approved this collection (if any)
        /// </summary>
        public string EditorialApprovalBy { get; set; }
        /// <summary>
        /// timestamp of when editorial approval occured
        /// </summary>
        public DateTime? EditorialApprovalDate { get; set; }
        /// <summary>
        /// the ids of the listings included in this collection
        /// </summary>
        public IList<IncludedListing> IncludedListings { get; set; }
        /// <summary>
        /// a list of profile ids of collaborators - these profiles will be able to edit this collection
        /// </summary>
        public IList<string> Collaborators { get; set; }
        /// <summary>
        /// a list of profile ids of permitted viewers - when the collection is private, these profiles will be able to view this collection
        /// when the collection is public, this list is irrelevant
        /// </summary>
        public IList<string> PermittedViewers { get; set; }
        /// <summary>
        /// the number of comments for this collection
        /// </summary>
        public int CommentCount { get; set; }
        /// <summary>
        /// the number of times users viewed this collection
        /// </summary>
        public int ViewCount { get; set; }
        /// <summary>
        /// the number of times people favorited this collection
        /// </summary>
        public int FavoriteCount { get; set; }
        /// <summary>
        /// A collection of media files' keys to be used in creating the collage
        /// </summary>
        public IList<string> CoverPhotos { get; set; }
        /// <summary>
        /// tags that identify the collection in searches
        /// </summary>
        public IList<string> Hashtags { get; set; }

        public string DefaultCulture { get; set; }

        // Translations
        public IDictionary<string, CollectionTranslation> Translations { get; set; }

        public Collection Translate(string culture)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                if (Translations != null)
                {
                    CollectionTranslation translation = null;
                    if (Translations.TryGetValue(culture, out translation))
                    {
                        Title = string.IsNullOrEmpty(translation.Title) ? Title : translation.Title;
                        Content = string.IsNullOrEmpty(translation.Content) ? Content : translation.Content;
                    }
                }

                // Translate included listings
                foreach (var listing in IncludedListings)
                {
                    listing.Translate(culture);
                }
            }            

            return this;
        }
    }
}

