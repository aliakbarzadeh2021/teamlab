using System;
using System.Collections.Generic;
using System.Text;

namespace ASC.Blogs.Core
{
    public interface ISettings
    {
        Guid ID { get; }
        ISettings GetDefault();
    }
}
