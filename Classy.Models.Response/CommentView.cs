using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Classy.Models.Response
{
    public class CommentView
    {
        public CommentView() { }
        //
        public string Id { get; set; }
        public string ProfileId { get; set; }
        public string Content { get; set; }
        //
        public ProfileView Profile { get; set; }
    }
}