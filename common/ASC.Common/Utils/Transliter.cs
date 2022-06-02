#region usings

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace ASC.Common.Utils
{
    public static class Transliter
    {
        private static readonly IDictionary<string, IDictionary<char, string>> dics;

        static Transliter()
        {
            dics = new Dictionary<string, IDictionary<char, string>>();
            var d = new Dictionary<char, string>(33);
            d['а'] = "A";
            d['б'] = "B";
            d['в'] = "V";
            d['г'] = "G";
            d['д'] = "D";
            d['е'] = "E";
            d['Ё'] = "Jo";
            d['ж'] = "Zh";
            d['з'] = "Z";
            d['и'] = "I";
            d['й'] = "J";
            d['к'] = "K";
            d['л'] = "L";
            d['м'] = "M";
            d['н'] = "N";
            d['о'] = "O";
            d['п'] = "P";
            d['р'] = "R";
            d['с'] = "S";
            d['т'] = "T";
            d['у'] = "U";
            d['ф'] = "F";
            d['х'] = "H";
            d['ц'] = "C";
            d['ч'] = "Ch";
            d['ш'] = "Sh";
            d['щ'] = "W";
            d['ь'] = "";
            d['ы'] = "Y";
            d['ъ'] = "";
            d['э'] = "E";
            d['ю'] = "Ju";
            d['я'] = "Ja";
            dics["ruen"] = d;
        }

        public static string Translit(string sourceLang, string destLang, string str)
        {
            string dicCode = (sourceLang + destLang).ToLowerInvariant();
            if (!dics.ContainsKey(dicCode))
            {
                throw new NotSupportedException(string.Format("No dictionary translit from {0} to {1}.", sourceLang, destLang));
            }
            var dic = dics[dicCode];
            var destStr = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                var symbol = str[i];
                var upperSymbol = char.ToUpper(symbol);
                if (dic.ContainsKey(upperSymbol))
                {
                    var destSymbol = dic[upperSymbol];
                    if (char.IsUpper(symbol))
                    {
                        if (i < str.Length - 1 && char.IsUpper(str[i + 1]))
                        {
                            destStr.Append(destSymbol.ToUpper());
                        }
                        else
                        {
                            destStr.Append(destSymbol);
                        }
                    }
                    else
                    {
                        destStr.Append(destSymbol.ToLower());
                    }
                }
                else
                {
                    destStr.Append(symbol);
                }
            }
            return destStr.ToString();
        }
    }
}