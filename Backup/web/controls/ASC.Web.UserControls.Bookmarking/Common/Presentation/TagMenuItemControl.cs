using System;
using System.Text;
using System.Web.UI;

namespace ASC.Web.UserControls.Bookmarking.Common.Presentation
{
	public class TagMenuItemControl: Control
	{
		public string Name { get; set; }

		public string Description { get; set; }		

		public string URL { get; set; }

		public string RightAlignedContent { get; set; }

		protected override void Render(HtmlTextWriter writer)
		{
			writer.Write("<div class='clearFix' style='margin-top:10px; padding-left: 20px; padding-right: 20px;'>");
			RenderContents(writer);
			writer.Write("</div>");
		}

		protected virtual void RenderContents(HtmlTextWriter writer)
		{
			if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(URL))
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("<div style='float: left;'><a href=\"{0}\" title=\"{1}\" class='linkAction'>",
						ResolveUrl(this.URL), Description.HtmlEncode());
				sb.Append(this.Name.HtmlEncode());				
				sb.Append("</a></div>");
				sb.AppendFormat("<div style='float: right;'>{0}</div>", RightAlignedContent.HtmlEncode());
				writer.Write(sb);
			}
		}
	}
}
