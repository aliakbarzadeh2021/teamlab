using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Web.Core;
using ASC.Web.Core.WebZones;


namespace ASC.Web.Studio.Core.Users
{
    [WebZone(WebZoneType.Nowhere)]
    public class PeopleProduct : AbstractProduct
    {
        public static Guid PeopleID { get { return new Guid("9EC81384-7909-44b1-B8F6-AFC912BC19ED"); } }
        private ProductContext _context;


        public override ProductContext Context
        {
            get { return _context; }
        }

        public override void Init(ProductContext context)
        {
            _context = context;           
            _context.DefaultSortOrder = -100;            
        }

        public override IModule[] Modules
        {
            get { return new IModule[0]; }
        }

        public override string ProductDescription
        {
            get { return "People"; }
        }

        public override Guid ProductID
        {
            get { return PeopleProduct.PeopleID; }
        }

        public override string ProductName
        {
            get { return "People"; }
        }

        public override void Shutdown()
        {
            
        }

        public override string StartURL
        {
            get { return "~/employee.aspx"; }
        }
    }
}
