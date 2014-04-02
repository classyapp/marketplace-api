using Classy.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    public class MongoEventTracker : IEventTracker
    {
        private MongoCollection<TrackableEvent> EventLog;

        public MongoEventTracker(MongoDatabase db)
        {
            EventLog = db.GetCollection<TrackableEvent>("events");
        }

        public string Track<T>(T theEvent) where T : TrackableEvent
        {
            if (string.IsNullOrEmpty(theEvent.AppId)) throw new ArgumentNullException("MongoEventTracker.Track: AppId missing");
            EventLog.Insert(theEvent);
            return theEvent.Id;
        }
    }
}
