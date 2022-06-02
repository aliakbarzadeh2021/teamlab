using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMResourceData.Model
{
    public class ProjectModule
    {
        public string Name { get; set; }
        public bool IsLock { get; set; }
        public List<ModuleWord> Translated { get; set; }
        public List<ModuleWord> UnTranslated { get; set; }
        public List<ModuleWord> Changed { get; set; }

        public ProjectModule()
        {
            Translated = new List<ModuleWord>();
            UnTranslated = new List<ModuleWord>();
            Changed = new List<ModuleWord>();
        }
    }
}
