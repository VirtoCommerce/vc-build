using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Nuke.Common.Utilities;

namespace PlatformTools.Extensions
{
    public static class JTokenPropertyOrderExtensions
    {
        /// <summary>
        /// Returns a copy of <paramref name="jObject"/> with properties ordered as in <paramref name="template"/>, recursively.
        /// Properties missing from the template go last in their current order.
        /// Each array item takes its order from the template item at the same position when that item has the same Id (or Name),
        /// or when the item has neither; otherwise from the first template item with the same Id (or Name); otherwise it is kept as is.
        /// </summary>
        public static JObject WithPropertyOrderOf(this JObject jObject, JObject template)
        {
            ArgumentNullException.ThrowIfNull(jObject);
            ArgumentNullException.ThrowIfNull(template);

            var templateIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var (property, index) in template.Properties().Select((property, index) => (property, index)))
            {
                templateIndexes.TryAdd(property.Name, index);
            }

            var result = new JObject();
            foreach (var property in jObject.Properties().OrderBy(property => templateIndexes.GetValueOrDefault(property.Name, int.MaxValue)))
            {
                var templateValue = template.GetValue(property.Name, StringComparison.OrdinalIgnoreCase);
                result.Add(property.Name, Reorder(property.Value, templateValue));
            }

            return result;
        }

        /// <summary>
        /// Returns the value of the object property with the given name, matched case-insensitively, converted to a string.
        /// Returns null when the token is not an object, the property is missing, or its value is not a primitive.
        /// </summary>
        public static string GetStringValueIgnoreCase(this JToken token, string propertyName)
        {
            return (token as JObject)?.GetValue(propertyName, StringComparison.OrdinalIgnoreCase) is JValue value
                ? value.Value<string>()
                : null;
        }

        private static JToken Reorder(JToken token, JToken template)
        {
            return token switch
            {
                JObject tokenObject when template is JObject templateObject => tokenObject.WithPropertyOrderOf(templateObject),
                JArray tokenArray when template is JArray templateArray =>
                    new JArray(tokenArray.Select((item, index) => Reorder(item, FindTemplateItem(templateArray, item, index)))),
                _ => token,
            };
        }

        private static JToken FindTemplateItem(JArray template, JToken item, int index)
        {
            var sameIndexItem = template.ElementAtOrDefault(index);
            var identity = GetIdentity(item);

            if (identity == null || identity.EqualsOrdinalIgnoreCase(GetIdentity(sameIndexItem)))
            {
                return sameIndexItem;
            }

            return template.FirstOrDefault(templateItem => identity.EqualsOrdinalIgnoreCase(GetIdentity(templateItem)));
        }

        private static string GetIdentity(JToken token)
        {
            return token.GetStringValueIgnoreCase("Id") ?? token.GetStringValueIgnoreCase("Name");
        }
    }
}
