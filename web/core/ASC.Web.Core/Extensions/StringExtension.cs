using System;
using System.Collections.Generic;
using System.Web;
using System.Text.RegularExpressions;

namespace System
{
    public static class StringExtension
    {   
      
        public static string HtmlEncode(this string str)
        {
            if (str == null)
                return null;

            return HttpUtility.HtmlEncode(str);
        }

        /// <summary>
        /// Replace ' on ′
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceSingleQuote(this string str)
        {
            if (str == null)
                return null;

            return str.Replace("'", "′");
        }

        public static bool TestEmailRegex(this string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
                return false;

            //string patternLenient = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";

            string patternStrict = @"^(([^<>()[\]\\.,;:\s@\""]+"

                  + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"

                  + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"

                  + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"

                  + @"[a-zA-Z]{2,}))$";

            Regex reStrict = new Regex(patternStrict);
            return reStrict.IsMatch(emailAddress);
        }
    }
}
