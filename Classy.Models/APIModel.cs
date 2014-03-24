using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models
{
    public class APIModel<T>
    {
        private T _object;
        public APIModel(T o)
        {
            _object = o;
        }

        public object Include(params Expression<Func<T, object>>[] includedProperties)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            string[] names = new string[includedProperties.Length];
            for (var i = 0; i < includedProperties.Length; i++)
            {
                names[i] = GetMemberName(includedProperties[i].Body);
            }

            var props = _object.GetType().GetProperties();

            foreach (var p in props)
            {
                if (names.Any(x => x == p.Name))
                {
                    dict.Add(p.Name, p.GetValue(_object));
                }
            }

            return new ReadOnlyDictionary<string, object>(dict);
        }

        private static string GetMemberName(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentException(
                    "The expression cannot be null.");
            }

            if (expression is MemberExpression)
            {
                // Reference type property or field
                var memberExpression = (MemberExpression)expression;
                return memberExpression.Member.Name;
            }

            if (expression is MethodCallExpression)
            {
                // Reference type method
                var methodCallExpression =
                    (MethodCallExpression)expression;
                return methodCallExpression.Method.Name;
            }

            if (expression is UnaryExpression)
            {
                // Property, field of method returning value type
                var unaryExpression = (UnaryExpression)expression;
                return GetMemberName(unaryExpression);
            }

            throw new ArgumentException("Invalid expression");
        }

        private static string GetMemberName(
                UnaryExpression unaryExpression)
        {
            if (unaryExpression.Operand is MethodCallExpression)
            {
                var methodExpression =
                    (MethodCallExpression)unaryExpression.Operand;
                return methodExpression.Method.Name;
            }

            return ((MemberExpression)unaryExpression.Operand)
                        .Member.Name;
        }

    }
}
