using System;
using System.Linq.Expressions;
using Classy.Models;
using NUnit.Framework;

namespace Classy.Tests.Search
{
    //[TestFixture]
    public class FoolingAround
    {
        //[Test]
        public void Test()
        {
            var a = ReturnPropertyName(x => x.ContactInfo);
            Assert.That(a, Is.EqualTo("ContactInfo"));
        }

        private string ReturnPropertyName<TProperty>(Expression<Func<Listing, TProperty>> propertyGetter)
        {
            var a = propertyGetter.Body as MemberExpression;
            return a.Member.Name;
        }
    }
}