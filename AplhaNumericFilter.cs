using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace Dakanna.AspNet.Filters
{
    [Serializable]
    public class AplhaNumericFilter : SortedDictionary<string, int>
    {
        public string BindPrefix { get; }

        public AplhaNumericFilter([CallerMemberName] string bindPrefix = "") : base(new AplhaNumericLenghtComparer(string.IsNullOrWhiteSpace(bindPrefix) ? AplhaNumericFilterConstants.BINDPREFIX : bindPrefix.ToLower()))
        {
            this.BindPrefix = string.IsNullOrWhiteSpace(bindPrefix) ? AplhaNumericFilterConstants.BINDPREFIX : bindPrefix.ToLower();

            this.Build();
        }

        public AplhaNumericFilter(IDictionary<string, int> dictionary) : base(dictionary)
        { }

        public new int this[string key]
        {
            get
            {
                TryGetValue($"{BindPrefix}[{key}]", out int value);
                if (value == 0)
                {
                    TryGetValue(key, out int value2);
                    return value2;
                }
                return value;
            }
            set
            {
                var wrappedKey = this.WrappedKey(key);

                base[wrappedKey] = value + this[AplhaNumericFilterConstants.ALL];
            }
        }

        public new bool ContainsKey(string key)
        {
            return base.ContainsKey(key) || base.ContainsKey($"{BindPrefix}[{key}]");
        }

        public new void Add(string key, int value)
        {
            AddOrUpdate(key, value);
        }

        public void AddOrUpdate(string key, int value)
        {
            var wrappedKey = this.WrappedKey(key);

            if (ContainsKey(AplhaNumericFilterConstants.ALL) && this[AplhaNumericFilterConstants.ALL] > 0)
            {
                base[wrappedKey] = value + this[AplhaNumericFilterConstants.ALL];
            }
            else
            {
                base[wrappedKey] = value;
            }
            var unwrappedKey = this.UnwrappedKey(key);
            if (this.Keys.Any(x => x == unwrappedKey))
            {
                Remove(unwrappedKey);
            }
        }

        public Tuple<Uri, string> GetTriggerQueryParamsAndText(Uri requestUrl, string key, int value)
        {
            var uriBuilder = new UriBuilder(requestUrl);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[this.WrappedKey(key)] = Convert.ToInt32(!Convert.ToBoolean(value)).ToString();

            var cleanQuery = new List<string>();
            foreach (var queryKey in query.Keys)
            {
                if (int.Parse(query[queryKey.ToString()]) == 0)
                {
                    cleanQuery.Add(queryKey.ToString());
                }
            }
            cleanQuery.ForEach(x => query.Remove(x));

            uriBuilder.Query = query.ToString();
            var uri = uriBuilder.Uri;
            var text = this.UnwrappedKey(key);

            return new Tuple<Uri, string>(uri, text);
        }
    }
}