﻿using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    /// <summary>
    /// DTO used to create a new <see cref="Collection"/>
    /// </summary>
    public class CreateCollection : BaseRequestDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsPublic { get; set; }
        public IList<string> IncludedListings { get; set; }
        public IList<string> Collaborators { get; set; }
        public IList<string> PermittedViewers { get; set; }

        public CreateCollection()
        {
            IsPublic = true; // by default all collections are public
        }
    }

    public class CreateCollectionValidator : AbstractValidator<CreateCollection>
    {
        public CreateCollectionValidator()
        {
            RuleFor(x => x.Title).NotEmpty();
        }
    }
}