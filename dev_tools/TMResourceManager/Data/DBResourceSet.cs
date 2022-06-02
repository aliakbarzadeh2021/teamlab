using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Resources;
using System.Globalization;

namespace TMResourceData
{
    public class DBResourceSet : ResourceSet
    {
        internal DBResourceSet(string _fileName, CultureInfo _culture)
            : base(new DBResourceReader(_fileName, _culture)) { }


        public override Type GetDefaultReader()
        {
            return typeof(DBResourceReader);
        }

        public override string GetString(string name, bool ignoreCase)
        {
            return base.GetString(name, ignoreCase);
        }

        public void SetString(object _name, object _newvalue)
        {
            if (Table[_name] != null)
                Table[_name] = _newvalue;
            else
                Table.Add(_name, _newvalue);
        }

        public int TableCount
        {
            get
            {
                return Table.Count;
            }
        }
    }
}
