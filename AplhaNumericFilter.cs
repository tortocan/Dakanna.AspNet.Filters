using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Web;

namespace Dakanna.AspNet.Filters
{
    [Serializable]
    public class AplhaNumericFilter : Dictionary<string, int>
    {
        private readonly string bindPrefix;

        public AplhaNumericFilter([CallerMemberName] string bindPrefix = "")
        {
            this.bindPrefix = bindPrefix.ToLower();
        }

        protected AplhaNumericFilter(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public AplhaNumericFilter(int capacity) : base(capacity)
        { }

        public AplhaNumericFilter(IEqualityComparer<string> comparer) : base(comparer)
        { }

        public AplhaNumericFilter(IDictionary<string, int> dictionary) : base(dictionary)
        { }

        public AplhaNumericFilter(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer)
        { }

        public AplhaNumericFilter(IDictionary<string, int> dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer)
        { }

        public new int this[string key]
        {
            get
            {
                if (ContainsKey($"{bindPrefix}[{key}]"))
                {
                    return base[$"{bindPrefix}[{key}]"];
                }

                return base[key];
            }
            set
            {
                var wrappedKey = WrappedKey(key);

                base[wrappedKey] = value + this["All"];
            }
        }

        public string WrappedKey(string key)
        {
            return key.StartsWith($"{bindPrefix}[") && key.EndsWith("]") ? key : $"{bindPrefix}[{key}]";
        }

        public string UnwrappedKey(string key)
        {
            return key.TrimStart($"{bindPrefix}[".ToCharArray()).TrimEnd(']');
        }

        public new bool ContainsKey(string key)
        {
            return base.ContainsKey(key) || base.ContainsKey($"{bindPrefix}[{key}]");
        }

        public new void Add(string key, int value)
        {
            AddOrUpdate(key, value);
        }

        public void AddOrUpdate(string key, int value)
        {
            var wrappedKey = WrappedKey(key);

            if (ContainsKey("All") && this["All"] > 0)
            {
                base[wrappedKey] = value + this["All"];
            }
            else
            {
                base[wrappedKey] = value;
            }
            var unwrappedKey = UnwrappedKey(key);
            if (ContainsKey(unwrappedKey))
            {
                Remove(unwrappedKey);
            }
        }

        public AplhaNumericFilter GetAplhaNumericFilters()
        {
            if (!ContainsKey("All"))
            {
                AddOrUpdate("All", 0);
            }
            else
            {
                AddOrUpdate("All", this["All"]);
            }

            GetAtoZ();
            Get0To10();

            return GetOrdered();
        }

        private void Get0To10()
        {
            for (int i = 0; i < 10; i++)
            {
                if (!ContainsKey(i.ToString()))
                {
                    AddOrUpdate(i.ToString(), this["All"]);
                }
                else
                {
                    AddOrUpdate(i.ToString(), this[i.ToString()]);
                }
            }
        }

        private void GetAtoZ()
        {
            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (!ContainsKey(c.ToString()))
                {
                    AddOrUpdate(c.ToString(), this["All"]);
                }
                else
                {
                    AddOrUpdate(c.ToString(), this[c.ToString()]);
                }
            }
        }

        public Tuple<Uri, string> GetTriggerQueryParamsAndText(Uri requestUrl, string key, int value)
        {
            var uriBuilder = new UriBuilder(requestUrl);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[key] = Convert.ToInt32(!Convert.ToBoolean(value)).ToString();

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
            var text = UnwrappedKey(key);
            return new Tuple<Uri, string>(uri, text);
        }

        private AplhaNumericFilter GetOrdered()
        {
            return new AplhaNumericFilter(this.OrderBy(x => UnwrappedKey(x.Key)).OrderByDescending(x => x.Key.Length).OrderByDescending(x => char.IsLetter(UnwrappedKey(x.Key)[0])).ThenBy(x => char.IsNumber(UnwrappedKey(x.Key)[0])).ToDictionary(x => x.Key, x => x.Value));
        }
    }
}