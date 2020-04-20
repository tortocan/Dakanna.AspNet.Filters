using System;
using System.Collections.Generic;
using System.Linq;

namespace Dakanna.AspNet.Filters
{
    public static class AplhaNumericFilterExtensions
    {
        public static IEnumerable<T> Apply<T>(this AplhaNumericFilter keyValuePairs, IEnumerable<T> source, IEnumerable<T> destination, params Func<T, string>[] selectors)
        {
            if (destination == null)
            {
                destination = source.ToList();
            }
            if (keyValuePairs.Any(x => x.Value > 0))
            {
                destination = source.Where(x =>
                {
                    var result = 0;
                    foreach (var selector in selectors)
                    {
                        var prop = selector(x);
                        var value = keyValuePairs.ContainsKey(prop[0].ToString()) ? keyValuePairs[prop[0].ToString()] : 0;
                        result += value;
                    }

                    return result > 0;
                });
                keyValuePairs.Build();
            }
            var allKey = $"{keyValuePairs.BindPrefix}[All]";
            if (keyValuePairs.ContainsKey(allKey) && keyValuePairs[allKey] > 0)
            {
                destination = source.ToList();
            }
            return destination;
        }

        public static AplhaNumericFilter Build(this AplhaNumericFilter keyValuePairs)
        {
            if (!keyValuePairs.ContainsKey(AplhaNumericFilterConstants.ALL))
            {
                keyValuePairs.AddOrUpdate(AplhaNumericFilterConstants.ALL, 0);
            }
            else
            {
                keyValuePairs.AddOrUpdate(AplhaNumericFilterConstants.ALL, keyValuePairs[AplhaNumericFilterConstants.ALL]);
            }

            keyValuePairs.GetAtoZ();
            keyValuePairs.Get0To10();

            return keyValuePairs;
        }

        private static void Get0To10(this AplhaNumericFilter keyValuePairs)
        {
            for (int i = 0; i < 10; i++)
            {
                if (!keyValuePairs.ContainsKey(i.ToString()))
                {
                    keyValuePairs.AddOrUpdate(i.ToString(), keyValuePairs[AplhaNumericFilterConstants.ALL]);
                }
                else
                {
                    keyValuePairs.AddOrUpdate(i.ToString(), keyValuePairs[i.ToString()]);
                }
            }
        }

        private static void GetAtoZ(this AplhaNumericFilter keyValuePairs)
        {
            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (!keyValuePairs.ContainsKey(c.ToString()))
                {
                    keyValuePairs.AddOrUpdate(c.ToString(), keyValuePairs[AplhaNumericFilterConstants.ALL]);
                }
                else
                {
                    keyValuePairs.AddOrUpdate(c.ToString(), keyValuePairs[c.ToString()]);
                }
            }
        }

        public static string WrappedKey(this AplhaNumericFilter keyValuePairs, string key)
        {
            return key.StartsWith($"{keyValuePairs.BindPrefix}[") && key.EndsWith("]") ? key : $"{keyValuePairs.BindPrefix}[{key}]";
        }

        public static string UnwrappedKey(this AplhaNumericFilter keyValuePairs, string key)
        {
            return key.TrimStart($"{keyValuePairs.BindPrefix}[".ToCharArray()).TrimEnd(']');
        }
    }
}