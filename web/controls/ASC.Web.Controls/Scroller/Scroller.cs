using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;

namespace ASC.Web.Controls
{

    [
    ToolboxData("<{0}:Scroller runat=\"server\"></{0}:Scroller>"),
    ToolboxBitmap(typeof(Scroller), "Scroller.ToolboxBitmaps.Scroller.bmp"),
    ]
    public class Scroller : Control
    {

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!Page.ClientScript.IsClientScriptBlockRegistered("ScrollEngine"))
            {

                StringBuilder sb;

                sb = new StringBuilder();
                sb.AppendLine(@"<script language=""javascript"">");
                sb.AppendLine(@"var timers = new Array();");
                sb.AppendLine(@"var times = new Array();");
                sb.AppendLine(@"var parentHs = new Array();");
                sb.AppendLine(@"function scroll(id, timeDelay, parentHeight){");
                sb.AppendLine(@"var scrollDiv = document.getElementById(id);");
                sb.AppendLine(@"var maxHeight = scrollDiv.offsetHeight;");
                sb.AppendLine(@"times[id] = timeDelay;");
                sb.AppendLine(@"parentHs[id] = parentHeight;");
                sb.AppendLine(@"timers[id] = setTimeout(""scrollOnTime('"" + id + ""', "" + timeDelay + "",  "" + maxHeight + "", "" + parentHeight + "")"", timeDelay);");
                sb.AppendLine(@"}");
                sb.AppendLine(@"");
                sb.AppendLine(@"function scrollOnTime(id, timeDelay, maxHeight, parentHeight){");
                sb.AppendLine(@"var scrollDiv = document.getElementById(id);");
                sb.AppendLine(@"scrollDiv.style.top = (scrollDiv.offsetTop - 1) + 'px';");
                sb.AppendLine(@"if(maxHeight + scrollDiv.offsetTop <= 0) scrollDiv.style.top = parentHeight + 'px';");
                sb.AppendLine(@"timers[id] = setTimeout(""scrollOnTime('"" + id + ""', "" + timeDelay + "",  "" + maxHeight + "", "" + parentHeight + "")"", timeDelay);");
                sb.AppendLine(@"}");
                sb.AppendLine(@"");
                sb.AppendLine(@"function scrollMouseOver(id){");
                sb.AppendLine(@"clearTimeout(timers[id]);");
                sb.AppendLine(@"}");
                sb.AppendLine(@"");
                sb.AppendLine(@"function scrollMouseOut(id){");
                sb.AppendLine(@"scrollOnTime(id, times[id], document.getElementById(id).offsetHeight, parentHs[id])");
                sb.AppendLine(@"}");
                sb.AppendLine(@"");
                sb.AppendLine(@"</script>");



                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ScrollEngine", sb.ToString());
            }
        }

        private string RegisterScript()
        {
            StringBuilder sb;
            
            sb = new StringBuilder();
            sb.AppendLine(@"<script language=""javascript"">");
            sb.AppendFormat(@"scroll('{0}_scrollWindow', {1}, {2});", this.ClientID, TimeDelay, Height.Value);
            sb.AppendLine(@"</script>");

            return sb.ToString();
        }

        protected override void Render(HtmlTextWriter writer)
        {


            writer.Write(@"<div style=""height:{0};position:relative;overflow:hidden;"" onmouseover=""javascript:scrollMouseOver('{1}_scrollWindow')"" onmouseout=""javascript:scrollMouseOut('{1}_scrollWindow')"">", Height, this.ClientID);
            writer.Write(@"<div id=""{0}_scrollWindow"" style=""width:100%;position:absolute;top:0;left:0;"">", this.ClientID);

            if (InnerHtml.Equals(string.Empty))
            {
                base.Render(writer);
            }
            else
            {
                writer.Write(InnerHtml);
            }
            writer.Write(@"</div>");
            writer.Write(@"</div>");

            writer.Write(RegisterScript());
        }


        #region private properties
        private bool RegistredScript
        {
            get
            {

                return Convert.ToBoolean(ViewState["RegistredScript"]);
            }
            set
            {
                ViewState["RegistredScript"] = value;
            }
        }
        #endregion

        #region public properties
        public string InnerHtml
        {
            get
            {
                if (ViewState["InnerHtml"] == null)
                    return string.Empty;
                return ViewState["InnerHtml"].ToString();
            }
            set
            {
                ViewState["InnerHtml"] = value;
            }
        }

        public Unit Height
        {
            get
            {
                if (ViewState["Height"] == null || ViewState["Height"].ToString().Equals(string.Empty))
                    return Unit.Parse("200px");
                return Unit.Parse(ViewState["Height"].ToString());
            }
            set
            {
                ViewState["InnerHtml"] = value.ToString();
            }
        }

        public int TimeDelay
        {
            get
            {
                int def = 100;
                if (ViewState["TimeDelay"] == null)
                    return def;
                try
                {
                    return Convert.ToInt32(ViewState["TimeDelay"]);    
                }
                catch {}

                return def;
                
            }
            set
            {
                ViewState["TimeDelay"] = value;
            }
        }


        #endregion
    }



}
