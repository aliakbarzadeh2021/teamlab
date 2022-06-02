using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMResourceData.Model
{
    public class ModuleWord
    {
        public int FileID { get; set; }
        public string FileName { get; set; }

        public string Title { get; set; }
        public string ValueFrom { get; set; }
        public string ValueTo { get; set; }
        public string TextComment { get; set; }
        public string Link { get; set; }
        public List<string> Alternative { get; set; }

        public int Flag { get; set; }
    }
}
