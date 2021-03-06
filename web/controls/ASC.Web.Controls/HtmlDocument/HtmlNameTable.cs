// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Xml;

namespace ASC.Web.Controls
{
    internal class HtmlNameTable : XmlNameTable
    {
        private NameTable _nametable = new NameTable();

        internal HtmlNameTable()
        {
        }

        internal string GetOrAdd(string array)
        {
            string s = Get(array);
            if (s == null)
            {
                return Add(array);
            }
            return s;
        }

        public override string Add(string array)
        {
            return _nametable.Add(array);
        }

        public override string Get(string array)
        {
            return _nametable.Get(array);
        }

        public override string Get(char[] array, int offset, int length)
        {
            return _nametable.Get(array, offset, length);
        }

        public override string Add(char[] array, int offset, int length)
        {
            return _nametable.Add(array, offset, length);
        }
    }

}
