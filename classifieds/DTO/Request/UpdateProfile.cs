using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    [Flags]
    public enum ProfileUpdateFields 
    {
        SetPassword = 1,
        ContactInfo = 2,
        ProfessionalInfo = 4,
        Metadata = 8,
        ProfileImage = 16,
        CoverPhotos = 32
    }

    public class UpdateProfile : BaseRequestDto
    {
        public string ProfileId { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public ProfessionalInfo ProfessionalInfo { get; set; }
        public ContactInfo ContactInfo { get; set; }
        public ProfileUpdateFields Fields { get; set; }
        public string Password { get; set; }
        public string DefaultCulture { get; set; }
        public IList<string> CoverPhotos { get; set; }
    }

    public class UpdateProfileValidator : AbstractValidator<UpdateProfile>
    {
        
        public UpdateProfileValidator()
        {
            When(x => x.Fields.HasFlag(ProfileUpdateFields.ProfessionalInfo), () =>
            {
                RuleFor(x => x.ProfessionalInfo).Cascade(CascadeMode.StopOnFirstFailure).NotNull();
                RuleFor(x => x.ProfessionalInfo.CompanyName).NotEmpty();
                RuleFor(x => x.DefaultCulture).NotNull();
            });

            When(x => x.Fields.HasFlag(ProfileUpdateFields.Metadata), () =>
            {
                RuleFor(x => x.Metadata).NotNull();
            });

            When(x => x.Fields.HasFlag(ProfileUpdateFields.ContactInfo), () =>
            {
                RuleFor(x => x.ContactInfo).NotNull();
            });

            When(x => x.Fields.HasFlag(ProfileUpdateFields.SetPassword), () =>
            {
                RuleFor(x => x.Password).NotNull();
            });

            When(x => x.Fields.HasFlag(ProfileUpdateFields.CoverPhotos), () =>
            {
                RuleFor(x => x.CoverPhotos).NotEmpty();
                RuleFor(x => x.CoverPhotos.Count).LessThanOrEqualTo(4).GreaterThanOrEqualTo(1);
            });
        }
    }
}