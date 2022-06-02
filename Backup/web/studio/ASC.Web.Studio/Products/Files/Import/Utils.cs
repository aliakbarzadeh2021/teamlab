using System;
using System.Xml.Linq;

namespace ASC.Web.Files.Import
{
    static class Utils
    {
        public static DateTime FromUnixTime(long time)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(time);
        }

        public static DateTime FromUnixTime2(long time)
        {
            return new DateTime(1970, 1, 1).AddSeconds(time);
        }

        public static string ElementValueOrDefault(this XElement element, XName name)
        {
            return ElementValueOrDefault(element, name, string.Empty);
        }

        public static string ElementValueOrDefault(this XElement element, XName name,  string defaultValue)
        {
            var value = element.Element(name);
            if (value!=null)
            {
                return value.Value;
            }
            return defaultValue;
        }

        public static string AttributeValueOrDefault(this XElement element, XName name)
        {
            return AttributeValueOrDefault(element, name, string.Empty);
        }

        public static string AttributeValueOrDefault(this XElement element, XName name, string defaultValue)
        {
            var attribute = element.Attribute(name);
            if (attribute != null)
            {
                return attribute.Value;
            }
            return defaultValue;
        }
    }
}
