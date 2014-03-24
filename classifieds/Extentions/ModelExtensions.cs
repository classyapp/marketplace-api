using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Classy.Models;

namespace classy.Extentions
{
    public static class ModelExtensions
    {
        public static APIModel<T> ToAPIModel<T>(this T obj)
        {
            return new APIModel<T>(obj);
        }
    }
}