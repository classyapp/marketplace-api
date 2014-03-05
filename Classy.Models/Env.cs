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
                }
                // check got at least one thing
                if (GPSCoordinates == null && string.IsNullOrEmpty(CountryCode))
                {

                    location.Address = new PhysicalAddress() { Country = defaultCountry };
                }
            }
            return location;
        }
    }
}
