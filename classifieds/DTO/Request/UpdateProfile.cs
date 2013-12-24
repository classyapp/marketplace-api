using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class UpdateProfile : BaseRequestDto
    {
        public string ProfileId { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public ProfessionalInfo ProfessionalInfo { get; set; }
        public string UpdateType { get; set; }
    }

    public class UpdateProfileValidator : AbstractValidator<UpdateProfile>
    {
        
        public UpdateProfileValidator()
        {
            When(x => x.UpdateType == "CreateProfessionalProfile", () =>
            {
                RuleFor(x => x.ProfessionalInfo.CompanyName).NotEmpty();
                RuleFor(x => x.ProfessionalInfo.CompanyContactInfo.FirstName).NotEmpty();
                RuleFor(x => x.ProfessionalInfo.CompanyContactInfo.LastName).NotEmpty();
                RuleFor(x => x.ProfessionalInfo.CompanyContactInfo.Phone).NotEmpty();
                RuleFor(x => x.ProfessionalInfo.Category).NotEmpty().NotEqual("0");
                RuleFor(x => x.ProfessionalInfo.CompanyContactInfo.Location.Address.Street1).NotEmpty();
                RuleFor(x => x.ProfessionalInfo.CompanyContactInfo.Location.Address.City).NotEmpty();
                RuleFor(x => x.ProfessionalInfo.CompanyContactInfo.Location.Address.PostalCode).NotEmpty();
            }); 
        }
    }
}