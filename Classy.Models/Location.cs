using MongoDB.Bson.Serialization.Attributes;
using ServiceStack.FluentValidation;

namespace Classy.Models
{
    public class Coords 
    {
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Location
    {
        public Coords Coords { get; set; }
        public PhysicalAddress Address { get; set; }
    }

    public class LocationValidator : AbstractValidator<Location>
    {
        public LocationValidator()
        {
            RuleFor(x => x.Coords).Cascade(CascadeMode.StopOnFirstFailure).NotNull();
            RuleFor(x => x.Coords.Longitude).NotNull();
            RuleFor(x => x.Coords.Latitude).NotNull();
        }
    }
}
