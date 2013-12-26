using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Response
{
    [Flags]
    public enum ActivityPredicate
    {
        View = 1,
        Like = 2,
        Follow = 4,
        Flag = 8,
        Book = 16,
        Purchase = 32,
        Mention = 64,
        Comment = 128,
        PostListing = 256,
        Review = 512,
        ProContact = 1024,
        SaveToCollection = 2048
    }

    public class TripleView
    {
        public string ObjectId { get; set; }
        public string Predicate { get; set; }
        public string SubjectId { get; set; }
    }
}