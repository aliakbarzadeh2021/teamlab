﻿using System;
using System.Text;
using System.Web.UI;

namespace ASC.Web.Studio.Controls.Common
{
    public class NotFoundControl : LiteralControl
    {   
        public string LinkFormattedText { get; set; }

        public string LinkURL { get; set; }

        public bool HasLink { get; set; }

        public NotFoundControl()
        {
            this.Text = Resources.Resource.SearchNotFoundMessage;
        }

        protected override void Render(HtmlTextWriter output)
        {
            StringBuilder sb = new StringBuilder("<div class=\"noContentBlock\">");

            sb.Append(this.Text);
            if (HasLink && !String.IsNullOrEmpty(LinkURL) && !String.IsNullOrEmpty(LinkFormattedText))
                sb.AppendFormat("&nbsp;" + LinkFormattedText, LinkURL);

            sb.Append("</div>");
            output.WriteLine(sb.ToString());
        }
    }
}
