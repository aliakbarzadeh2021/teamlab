﻿using System.Collections.Generic;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Users.Activity;

namespace ASC.Web.Core
{   

    public class ModuleContext : WebItemContext
    {        
        public ModuleContext()
        {
            this.UserActivityPublishers = new List<IUserActivityPublisher>();
        }
        
        public IStatisticProvider StatisticProvider { get; set; }
    }    
}
