using MongoDB.Bson.Serialization.Attributes;
using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models
{
    public class Location
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public PhysicalAddress Address { get; set; }
    }

    public class LocationValidator : AbstractValidator<Location>
    {
        public LocationValidator()
        {
            RuleFor(x => x.Longitude)
                .GreaterThan(0)
                .WithErrorCode("Missing Lognitude");
            RuleFor(x => x.Latitude)
                .GreaterThan(0)
                .WithErrorCode("Missing Latitude");
        }
    }
}
