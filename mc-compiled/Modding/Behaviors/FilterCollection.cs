using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Behaviors
{
    /// <summary>
    /// A collection of filters, optionally merged in a comparison.
    /// </summary>
    public class FilterCollection : List<Filter>
    {
        public FilterMerge merge = FilterMerge.all_of;

        public JObject ToJSON()
        {
            if (Count == 0)
                return new JObject();
            if (Count == 1)
                return this[0].ToJSON();

            return new JObject()
            {
                [merge.ToString()] = new JArray(this.Select(filter => filter.ToJSON()))
            };
        }
    }
    public enum FilterMerge
    {
        all_of,
        any_of,
        none_of
    }
}
