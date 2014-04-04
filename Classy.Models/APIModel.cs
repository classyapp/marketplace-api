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
            var objectType = _object.GetType();
            Dictionary<string, object> dict = new Dictionary<string, object>();

            for (var i = 0; i < includedProperties.Length; i++)
            {
                var propertyGraph = GetMemberNameGraph(includedProperties[i].Body);
                PropertyGraphToDictionary(dict, _object, propertyGraph.First);
            }

            return new ReadOnlyDictionary<string, object>(dict);
        }

        private void PropertyGraphToDictionary(Dictionary<string, object> dict, object o, LinkedListNode<string> propertyNode)
        {
            var propertyName = propertyNode.Value;
            var propertyValue = o.GetType().GetProperty(propertyName).GetValue(o);
            if (propertyNode.Next == null)
            {
                dict[propertyName] = propertyValue;
            }
            else
            {
                if (!dict.ContainsKey(propertyName) || !(dict[propertyName] is Dictionary<string, object>))
                {
                    dict[propertyName] = new Dictionary<string, object>();
                }
                Dictionary<string, object> propertyDict = (Dictionary<string, object>) dict[propertyName];
                PropertyGraphToDictionary(propertyDict, propertyValue, propertyNode.Next);
            }
        }

        private static LinkedList<string> GetMemberNameGraph(Expression expression)
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
                return new LinkedList<string>(new string[] { memberExpression.Member.Name });
            }

            if (expression is MethodCallExpression)
            {
                // Reference type method
                var methodCallExpression =
                    (MethodCallExpression)expression;
                return new LinkedList<string>(new string[] { methodCallExpression.Method.Name });
            }

            if (expression is UnaryExpression)
            {
                // Property, field of method returning value type
                var unaryExpression = (UnaryExpression)expression;
                return GetMemberNameGraph(unaryExpression);
            }

            throw new ArgumentException("Invalid expression");
        }

        private static LinkedList<string> GetMemberNameGraph(
                UnaryExpression unaryExpression)
        {
            var memberExpression = ((MemberExpression)unaryExpression.Operand);

            LinkedList<string> parentMemberGraph = new LinkedList<string>();
            if (memberExpression.Expression is MemberExpression)
            {
                parentMemberGraph = GetMemberNameGraph((MemberExpression)memberExpression.Expression);
            }

            var memberName = "";
            if (unaryExpression.Operand is MethodCallExpression)
            {
                var methodExpression =
                    (MethodCallExpression)unaryExpression.Operand;
                memberName = methodExpression.Method.Name;
            }
            else
            {
                memberName = memberExpression.Member.Name;
            }


            if (parentMemberGraph.Count == 0)
            {
                return new LinkedList<string>(new string[] {memberName});
            }
            else
            {
                parentMemberGraph.AddLast(memberExpression.Member.Name);
                return parentMemberGraph;
            }
        }

    }
}
