using Classy.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Classy.Models
{
    public class DateRange
    {
        public DateRange() { }
        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public DateRange Clone()
        {
 	        return new DateRange(this.Start, this.End);
        }
    }

    [MongoCollection(Name = "bookings")]
    public class BookedTimeslot : BaseObject
    {
        public string ListingId { get; set; }
        public string ProfileId { get; set; }
        public DateRange DateRange { get; set; }
        public string Comment { get; set; }
        public double Price { get; set; }
        public string TransactionId { get; set; }
        public bool IsCancelled { get; set; }
    }

    public class TimeslotPricePoint
    {
        public int LengthInMinutes { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
    }

    public class TimeslotSchedule
    {
        public IList<DateRange> BlackoutTimes { get; set; }
        public IList<TimeslotPricePoint> PricePoints { get; set; }
        
        // 
        public double GetTimeslotPrice(DateRange timeslot)
        {
            var minutes = timeslot.End.Subtract(timeslot.Start).TotalMinutes;

            var pricePoint = this.PricePoints.SingleOrDefault(x => x.LengthInMinutes >= minutes && x.LengthInMinutes <= minutes) ??
                this.PricePoints.SingleOrDefault(x => x.LengthInMinutes <= minutes);

            if (pricePoint == null) throw new ApplicationException("timeslot cannot be prices - it is below the minimum booking time allowed");

            return (minutes / pricePoint.LengthInMinutes) * pricePoint.Price;
        }
    }
}
