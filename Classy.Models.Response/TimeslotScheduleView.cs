using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models.Response
{
    public class DateRangeView
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class TimeslotPricePointView
    {
        public int LengthInMinutes { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
    }

    public class TimeslotScheduleView
    {
        public IList<DateRangeView> BlackoutTimes { get; set; }
        public IList<TimeslotPricePointView> PricePoints { get; set; }
    }

    public class BookedTimeslotView
    {
        public string Id { get; set; }
        public string ListingId { get; set; }
        public string ProfileId { get; set; }
        public DateRangeView DateRange { get; set; }
        public string Comment { get; set; }
        public double Price { get; set; }
        public string TransactionId { get; set; }
        public bool IsCancelled { get; set; }
    }
}
