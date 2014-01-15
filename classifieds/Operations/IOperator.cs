using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace classy.Operations
{
    public interface IOperator<T> where T : class
    {
        void PerformOperation(T request);
    }
}