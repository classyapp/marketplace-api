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
        Metadata = 8
    }

    public class UpdateProfile : BaseRequestDto
    {
        public string ProfileId { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public ProfessionalInfo ProfessionalInfo { get; set; }
        public ContactInfo ContactInfo { get; set; }
        public ProfileUpdateFields Fields { get; set; }
        public string Password { get; set; }
    }

    public class UpdateProfileValidator : AbstractValidator<UpdateProfile>
    {
        
        public UpdateProfileValidator()
        {
            When(x => x.Fields.HasFlag(ProfileUpdateFields.ProfessionalInfo), () =>
            {
                RuleFor(x => x.ProfessionalInfo).Cascade(CascadeMode.StopOnFirstFailure).NotNull();
                RuleFor(x => x.ProfessionalInfo.CompanyName).NotEmpty();
                RuleFor(x => x.ProfessionalInfo.CompanyContactInfo.FirstName).NotEmpty();
                RuleFor(x => x.ProfessionalInfo.CompanyContactInfo.LastName).NotEmpty();
                RuleFor(x => x.ProfessionalInfo.CompanyContactInfo.Phone).NotEmpty();
                RuleFor(x => x.ProfessionalInfo.Category).NotEmpty().NotEqual("0");
                RuleFor(x => x.ProfessionalInfo.CompanyContactInfo.Location.Address.Street1).NotEmpty();
                RuleFor(x => x.ProfessionalInfo.CompanyContactInfo.Location.Address.City).NotEmpty();
                RuleFor(x => x.ProfessionalInfo.CompanyContactInfo.Location.Address.PostalCode).NotEmpty();
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
        }
    }
}