using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Web.UI;
using System.ComponentModel;

namespace ASC.Web.Controls
{

	public class ViewSwitcherTabItem : Control
	{
		public string OnClickText { get; set; }

		public string TabName { get; set; }

		private bool selected;

		public bool IsSelected
		{
			get
			{
				return selected;
			}
			set
			{
				selected = value;
			}
		}

		public bool SkipRender { get; set; }

		public string SortItemsDivID { get; set; }

		public bool HideSortItems { get; set; }

		public string DivID = Guid.NewGuid().ToString();

		public string GetSortLink
		{
			get
			{
				var tabCssName = "viewSwitcherTab";
				var javascriptText = string.Format(@" onclick=""{0}; viewSwitcherToggleTabs(this.id);"" ", OnClickText, HideSortItems.ToString().ToLower(), SortItemsDivID);
				if (IsSelected)
				{
					tabCssName = "viewSwitcherTabSelected";
				}

				var sb = new StringBuilder();
				sb.AppendFormat(@"<li id='{0}_ViewSwitcherTab' class='{1}' {2}>{3}</li>", DivID, tabCssName, javascriptText, TabName);
				return sb.ToString();
			}
		}

		public void RenderTabContent(HtmlTextWriter writer)
		{
			if (!SkipRender)
			{
				writer.Write(string.Format("<div id='{0}'{1}>", DivID, IsSelected ? string.Empty : " style='display: none;'"));
				this.RenderControl(writer);
				writer.Write("</div>");
			}
		}
	}
}
