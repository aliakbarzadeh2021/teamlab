using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Controls
{
    public class ViewSwitcherComboboxItem : Control
	{
		public string SortLabel { get; set; }

        public string HintText { get; set; }

		public string SortUrl { get; set; }

		public string CssClass = "viewSwitcherDropdownItem";
	}
}
