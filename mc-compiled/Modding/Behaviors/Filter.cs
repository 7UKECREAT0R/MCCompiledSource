using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Behaviors
{
    public abstract class Filter
    {
        /// <summary>
        /// Get the test that occurs.
        /// </summary>
        /// <returns></returns>
        public abstract string GetTest();
        /// <summary>
        /// Get the value to test with.
        /// </summary>
        /// <returns></returns>
        public abstract object GetValue();

        /// <summary>
        /// Get any extra properties that go with this filter.
        /// </summary>
        /// <returns></returns>
        public abstract Newtonsoft.Json.Linq.JProperty[] GetExtraProperties();

        public EventSubject subject;
        public FilterOperator check;

        public Newtonsoft.Json.Linq.JObject ToJSON()
        {
            var json = new Newtonsoft.Json.Linq.JObject();
            json["test"] = GetTest();
            json["subject"] = subject.ToString();

            var properties = GetExtraProperties();
            if (properties != null)
                foreach (var property in properties)
                    json[property.Name] = property.Value;

            json["operator"] = check.String();
            json["value"] = GetValue().ToString();
            return json;
        }
    }
    /// <summary>
    /// Indicates a filter which checks a boolean value.
    /// </summary>
    public abstract class BooleanFilter : Filter
    {
        public bool checkValue;
        public override object GetValue() => checkValue;
    }


    /// <summary>
    /// Operators used in filter tests.
    /// </summary>
    public enum FilterOperator
    {
        EQUAL,
        UNEQUAL,
        LESS,
        GREATER,
        LESS_EQUAL,
        GREATER_EQUAL
    }
    public static class FilterOperatorExtensions
    {
        public static string String(this FilterOperator @operator)
        {
            switch (@operator)
            {
                case FilterOperator.EQUAL:
                    return "equals";
                case FilterOperator.UNEQUAL:
                    return "not";
                case FilterOperator.LESS:
                    return "<";
                case FilterOperator.GREATER:
                    return ">";
                case FilterOperator.LESS_EQUAL:
                    return "<=";
                case FilterOperator.GREATER_EQUAL:
                    return ">=";
                default:
                    return "??";
            }
        }
    }
}
