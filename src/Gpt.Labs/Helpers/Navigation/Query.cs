namespace Gpt.Labs.Helpers.Navigation
{
    #region

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;

    #endregion

    /// <summary>
    ///     The query builder.
    /// </summary>
    public class Query : SortedDictionary<string, object>, IEquatable<Query>
    {
        #region Static Fields

        /// <summary>
        ///     The query string regex.
        /// </summary>
        private static readonly Regex QueryStringRegex = new Regex(@"[\?&](?<name>[^&=]+)=(?<value>[^&=]+)");

        #endregion

        #region Public Static Methods and Operators

        /// <summary>
        /// Checks whether is objects are equal
        /// </summary>
        /// <param name="a">
        /// The a.
        /// </param>
        /// <param name="b">
        /// The b.
        /// </param>
        /// <returns>
        /// The result of check
        /// </returns>
        public static bool operator ==(Query a, Query b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null || a.Count != b.Count)
            {
                return false;
            }

            // Return true if the fields match:
            return a.ToString() == b.ToString();
        }

        /// <summary>
        /// Checks whether are objects not equal
        /// </summary>
        /// <param name="a">
        /// The a.
        /// </param>
        /// <param name="b">
        /// The b.
        /// </param>
        /// <returns>
        /// The result of check
        /// </returns>
        public static bool operator !=(Query a, Query b)
        {
            return !(a == b);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Clones curent query
        /// </summary>
        /// <returns>
        /// The new query with current values
        /// </returns>
        public Query Clone()
        {
            var data = new Query();

            foreach (var item in this)
            {
                data.Add(item.Key, item.Value);
            }

            return data;
        }

        /// <summary>
        /// Gets query object from query string.
        /// </summary>
        /// <param name="queryString">
        /// The query string.
        /// </param>
        /// <returns>
        /// The constructed <see cref="Query"/> object.
        /// </returns>
        public static Query Parse(object queryString)
        {
            var data = new Query();

            if (queryString == null)
            {
                return data;
            }

            var matches = QueryStringRegex.Matches(queryString.ToString());
            for (var i = 0; i < matches.Count; i++)
            {
                data.Add(matches[i].Groups["name"].Value, WebUtility.UrlDecode(matches[i].Groups["value"].Value));
            }

            return data;
        }

        /// <summary>
        ///     Converts represented object to query string.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public override string ToString()
        {
            var query = "?";

            for (var i = 0; i < Count; i++)
            {
                var item = this.ElementAt(i);

                if (item.Value != null)
                {
                    string value;

                    if (item.Value.GetType().IsArray && item.Value is IEnumerable enumerable)
                    {
                        value = string.Join("|", enumerable.Cast<object>());
                    }
                    else
                    {
                        value = item.Value.ToString();
                    }

                    query += item.Key + "=" + WebUtility.UrlEncode(value);
                }

                if (i < Count - 1)
                {
                    query += "&";
                }
            }

            return query;
        }

        /// <summary>
        /// The try get value.
        /// </summary>
        /// <typeparam name="T">
        /// The type of value
        /// </typeparam>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool TryGetValue<T>(string key, out T value)
        {
            try
            {
                if (TryGetValue(key, out var inValue))
                {
                    if (inValue == null)
                    {
                        value = default;
                        return true;
                    }

                    if (!TryGetValue(typeof(T), inValue, out var outType, out var outValue))
                    {
                        value = default;
                        return false;
                    }

                    var safeValue = Convert.ChangeType(outValue, outType);
                    value = (T)safeValue;
                    return true;
                }
            }
            catch (FormatException)
            {
                // Ok to leave it empty
            }

            value = default;
            return false;
        }

        public bool TryGetCollectionValue<T>(string key, out List<T> value)
        {
            try
            {
                if (TryGetValue(key, out var inValue))
                {
                    if (inValue == null)
                    {
                        value = default;
                        return true;
                    }

                    var values = inValue.ToString().Split('|');

                    var arrayData = new List<T>();

                    foreach (var item in values)
                    {
                        if (TryGetValue(typeof(T), item, out var itemType, out var itemValue))
                        {
                            var safeValue = Convert.ChangeType(itemValue, itemType);
                            arrayData.Add((T)safeValue);
                        }
                    }

                    value = arrayData;

                    return true;
                }
            }
            catch (FormatException)
            {
                // Ok to leave it empty
            }

            value = default;
            return false;
        }

        public T GetValue<T>(string key)
        {
            T val;
            TryGetValue(key, out val);

            return val;
        }

        public List<T> TryGetCollectionValue<T>(string key)
        {
            List<T> val;
            TryGetCollectionValue(key, out val);

            return val;
        }

        public override bool Equals(object obj)
        {
            var p = obj as Query;
            if (p == null)
            {
                return false;
            }

            return p.Count == Count && ToString().Equals(p.ToString());
        }

        public bool Equals(Query p)
        {
            if (p == null)
            {
                return false;
            }

            return p.Count == Count && ToString().Equals(p.ToString());
        }

        public bool EqualsWithExclude(Query p, params string[] keysToExclude)
        {
            if (p == null)
            {
                return false;
            }

            var left = this;
            var right = p;

            if (keysToExclude != null && keysToExclude.Length != 0)
            {
                left = left.Clone();
                right = right.Clone();

                foreach (var item in keysToExclude)
                {
                    if (left.ContainsKey(item))
                    {
                        left.Remove(item);
                    }

                    if (right.ContainsKey(item))
                    {
                        right.Remove(item);
                    }
                }
            }

            return left.Count == right.Count && left.ToString().Equals(right.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        #endregion

        #region Private Methods

        private static bool TryGetValue(Type rootType, object inValue, out Type outType, out object outValue)
        {
            outType = Nullable.GetUnderlyingType(rootType) ?? rootType;

            if (outType == typeof(DateTime))
            {
                if (DateTime.TryParse(inValue.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
                {
                    outValue = date;
                }
                else
                {
                    outValue = default(DateTime);
                    return false;
                }
            }
            else if (outType == typeof(DateTimeOffset))
            {
                if (DateTimeOffset.TryParse(inValue.ToString(), out var date))
                {
                    outValue = date;
                }
                else
                {
                    outValue = default(DateTimeOffset);
                    return false;
                }
            }
            else if (outType == typeof(Guid))
            {
                if (Guid.TryParse(inValue.ToString(), out var date))
                {
                    outValue = date;
                }
                else
                {
                    outValue = default(Guid);
                    return false;
                }
            }
            else if (outType.BaseType == typeof(Enum))
            {
                try
                {
                    outValue = Enum.Parse(outType, inValue.ToString());
                }
                catch (Exception)
                {
                    outValue = default(Enum);
                    return false;
                }
            }
            else
            {
                outValue = inValue;
            }

            return true;
        }

        #endregion
    }
}
