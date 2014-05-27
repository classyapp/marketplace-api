using System;

namespace Classy.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MongoCollectionAttribute : Attribute
    {
        public string Name { get; set; }
    }
}