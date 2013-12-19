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
        public IList<CustomAttribute> Metadata { get; set; }
        public Seller SellerInfo { get; set; }
        public string UpdateType { get; set; }
    }

    public class UpdateProfileValidator : AbstractValidator<UpdateProfile>
    {
        
        public UpdateProfileValidator()
        {
            When(x => x.UpdateType == "CreateProfessionalProfile", () =>
            {
                RuleFor(x => x.SellerInfo.DisplayName).NotEmpty();
                RuleFor(x => x.SellerInfo.ContactInfo.FirstName).NotEmpty();
                RuleFor(x => x.SellerInfo.ContactInfo.LastName).NotEmpty();
                RuleFor(x => x.SellerInfo.ContactInfo.Phone).NotEmpty();
                RuleFor(x => x.SellerInfo.Category).NotEmpty().NotEqual("0");
                RuleFor(x => x.SellerInfo.ContactInfo.Location.Address.Street1).NotEmpty();
                RuleFor(x => x.SellerInfo.ContactInfo.Location.Address.City).NotEmpty();
                RuleFor(x => x.SellerInfo.ContactInfo.Location.Address.PostalCode).NotEmpty();
            }); 
        }
    }
}