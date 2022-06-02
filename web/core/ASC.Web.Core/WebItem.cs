using System;
using System.Collections.Generic;

namespace ASC.Web.Core
{  

    public interface IWebItem
    {
        Guid ID { get; }
        string Name { get;}
        string Description { get; }
        WebItemContext Context { get; }
        string StartURL { get; }		
    }

	public class NavigationWebItem : AbstractModule
	{
		#region Members

		public Guid _ModuleID {get; set;}

		public string _ModuleName { get; set; }

		public string _ModuleDescription { get; set; }

		public string _StartURL { get; set; }

		public ModuleContext _Context { get; set; }

		#endregion

        public NavigationWebItem() : this(true) { }
        public NavigationWebItem(bool autoregister):base(autoregister){ }

		public override Guid ModuleID
		{
			get { return _ModuleID; }
		}

		public override string ModuleName
		{
			get { return _ModuleName; }
		}

		public override string ModuleDescription
		{
			get { return _ModuleDescription; }
		}

		public override string StartURL
		{
			get { return _StartURL; }
		}

		public override ModuleContext Context
		{
			get { return _Context; }
		}

		public string ModuleSysName { get; set; }

		public bool DisplayedAlways { get; set; }

        public override bool Equals(object obj)
        {
            var m = obj as IModule;
            return m != null && ModuleID == m.ModuleID;
        }
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
	}


}
