using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models
{
    public abstract class TrackableEvent : BaseObject
    {
        // core
        public abstract string Name { get; set; }
        public string ProfileId { get; set; }
        public string ObjectId { get; set; }

        // segmentation
        public bool IsProfessional { get; set; }
    }

    public class GenericEvent : TrackableEvent
    {
        private string _name;

        public GenericEvent(string name)
        {
            _name = name;
        }

        public override string Name
        {
            get { return _name; }
            set { }
        }
    }

    public class ClaimProxyEvent : TrackableEvent
    {
        public override string Name
        {
            get { return "proxy-claim"; }
            set { }
        }

        public string ClaimId { get; set; }
    }

    public interface IEventTracker
    {
        /// <summary>
        /// logs an event
        /// </summary>
        /// <param name="theEvent"></param>
        /// <returns>the logged event id</returns>
        string Track<T>(T theEvent) where T : TrackableEvent;
    }
}
