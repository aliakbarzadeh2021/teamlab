using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMResourceData.Model
{
    public class Author
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public List<ResCulture> Langs { get; set; }
    }
}
