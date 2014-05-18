using System;
using System.ComponentModel.Design;

namespace classy.Infrastructure.Events
{
    public class EventsAggregator
    {
        public delegate void Event<T>(T args) where T : IEvent;

        public static event Event<IEvent> Handler;

        public void Publish<T>(T args) where T : IEvent
        {
            if (Handler != null)
                Handler(args);
        }
    }

    public interface IEvent { }

    public class MyEvent
    {
    }


    public class Subscriber<T> where T : IEvent
    {
        //public Subscriber()
        //{
        //    EventsAggregator.Handler += 
        //}

        //public void Subscribe<T>(T args) where T : IEvent
        //{
            
        //}
    }
}