using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Classy.Models
{
    /// <summary>
    /// Environment settings for use throughout the API
    /// </summary>
    public class Env
    {
        private static Dictionary<string, Coords> DefaultCoordinates;

        static Env()
        {
            DefaultCoordinates = new Dictionary<string, Coords>();
            DefaultCoordinates.Add("US", new Coords { Longitude = -77.0364, Latitude = 38.8951 });
            DefaultCoordinates.Add("UK", new Coords { Longitude = -0.1277, Latitude = 51.5073 });
            DefaultCoordinates.Add("FR", new Coords { Longitude = 2.3522, Latitude = 48.8566 });
            DefaultCoordinates.Add("IT", new Coords { Longitude = 12.4608, Latitude = 41.9015 });
            DefaultCoordinates.Add("ES", new Coords { Longitude = -3.7038, Latitude = 40.4168 });
            DefaultCoordinates.Add("IL", new Coords { Longitude = 35.2137, Latitude = 31.7683 });
            DefaultCoordinates.Add("BE", new Coords { Longitude = 4.3517, Latitude = 50.853 });
            DefaultCoordinates.Add("NL", new Coords { Longitude = 4.8952, Latitude = 52.3702 });
        }

        public Env()
        {
            // set defaults
            CultureCode = "en-US";
            CurrencyCode = "USD";
            CountryCode = "US";
        }

        /// <summary>
        /// The id of the calling application. 
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// The full culture code. Defaults to en-US.
        /// </summary>
        public string CultureCode { get; set; }
        /// <summary>
        /// The 3-char ISO currency code. Defaults to "USD".
        /// </summary>
        public string CurrencyCode { get; set; }
        /// <summary>
        /// The 2-char ISO country code. Defaults to "US".
        /// </summary>
        public string CountryCode { get; set; }
        /// <summary>
        /// latitude and longitude
        /// </summary>
        public GPSLocation GPSCoordinates { get; set; }

        private Location location;

        public Location GetDefaultLocation(string defaultCountry)
        {
            if (location == null)
            {
                location = new Location();
                if (GPSCoordinates != null)
                {
                    location.Coords = new Coords { Longitude = GPSCoordinates.Longitude, Latitude = GPSCoordinates.Latitude };
                }
                if (!string.IsNullOrEmpty(CountryCode))
                {
                    location.Address = new PhysicalAddress() { Country = CountryCode };
                    if (location.Coords == null)
                    {
                        Coords coords = DefaultCoordinates[defaultCountry];
                        location.Coords = new Coords { Longitude = coords.Longitude, Latitude = coords.Latitude };
                    }
                }
                // check got at least one thing
                if (GPSCoordinates == null && string.IsNullOrEmpty(CountryCode))
                {
                    Coords coords = DefaultCoordinates[defaultCountry];
                    location.Coords = new Coords { Longitude = coords.Longitude, Latitude = coords.Latitude };
                    location.Address = new PhysicalAddress() { Country = defaultCountry };
                }
            }
            return location;
        }
    }
}
