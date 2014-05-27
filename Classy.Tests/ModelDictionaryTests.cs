using classy.Extentions;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using NUnit.Framework;

namespace Classy.Tests
{
    class ViewModel
    {
        public string MyString { get; set; }
        public int MyInt { get; set; }
        public ViewModel MyChild { get; set; }
    }
    [TestFixture]
    public class ModelDictionaryTests
    {
        ViewModel _viewModel;

        [SetUp]
        public void Init()
        {
            _viewModel = new ViewModel()
            {
                MyString = "String",
                MyInt = 42,
                MyChild = new ViewModel()
                {
                    MyString = "ChildString",
                    MyInt = 24
                }
            };
        }

        [Test]
        public void IncludeStringPropertyRetainsString()
        {
            ReadOnlyDictionary<string, object> dict = (ReadOnlyDictionary<string, object>)_viewModel.ToAPIModel().Include(x => x.MyString);
            Assert.AreEqual(_viewModel.MyString, dict["MyString"]);
        }

        [Test]
        public void IncludeDoesNotRetainNotIncludedString()
        {
            ReadOnlyDictionary<string, object> dict = (ReadOnlyDictionary<string, object>)_viewModel.ToAPIModel().Include(x => x.MyInt);
            Assert.IsFalse(dict.ContainsKey("MyString"));
        }

        [Test]
        public void IncludeChildObjectRetainsObject()
        {
            ReadOnlyDictionary<string, object> dict = (ReadOnlyDictionary<string, object>)_viewModel.ToAPIModel().Include(x => x.MyChild);
            Assert.AreEqual(_viewModel.MyChild, dict["MyChild"]);
        }

        [Test]
        public void SpecifyingSubPropertiesEnsuresOnlyThosePropertiesAreSet()
        {
            ReadOnlyDictionary<string, object> dict = (ReadOnlyDictionary<string, object>)_viewModel.ToAPIModel().Include(x => x.MyChild.MyInt);
            var propertyDict = (IDictionary<string, object>)dict["MyChild"];
            Assert.AreEqual(_viewModel.MyChild.MyInt, propertyDict["MyInt"]);
            Assert.IsFalse(propertyDict.ContainsKey("MyString"));
        }

        [Test]
        public void DeserializingSerializedObjectResultsInSameData()
        {
            var dict = _viewModel.ToAPIModel().Include(x => x.MyString);


            var s = ServiceStack.Text.JsonSerializer.SerializeToString(dict);
            ViewModel vm = ServiceStack.Text.JsonSerializer.DeserializeFromString<ViewModel>(s);

            Assert.AreEqual(_viewModel.MyString, vm.MyString, string.Format("{0} vs {1}", _viewModel.MyString, vm.MyString));
        }
    }
}
