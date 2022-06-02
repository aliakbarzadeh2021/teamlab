using System;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web;
using System.Globalization;

namespace ASC.Web.Controls
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Web.UI;

    using System.Collections.Specialized;
    using System.Drawing;







    [
    DefaultProperty("Value"),
    ToolboxBitmap(typeof(ColorPicker), "ColorPicker.ToolboxBitmaps.ColorPicker.bmp"),
    ToolboxData("<{0}:ColorPicker runat=\"server\"/>"),
    ]
    public class ColorPicker : Control, IPostBackDataHandler
    {

        #region Events

        public delegate void ColorChangedHandler(object sender, ColorPickerColorChangedArgs e);
        public event ColorChangedHandler ColorChanged;

        #endregion

        #region Private Fields

        private bool _AutoPostBack = false;
        private string _Width = "25px";
        private string _Height = "18px";
        private string _BorderColor = "#999";


        #endregion

        #region Public Properties

        /// <summary>
        /// Get Set AutoPostBack
        /// </summary>
        public bool AutoPostBack
        {
            get
            {
                return _AutoPostBack;
            }
            set
            {
                _AutoPostBack = value;
            }
        }

        /// <summary>
        /// Width of the control
        /// </summary>
        public string Width
        {
            get
            {
                return _Width.TrimEnd("px".ToCharArray());
            }
            set
            {
                _Width = string.Format("{0}px", value.Trim().TrimEnd("px".ToCharArray()));
            }
        }

        /// <summary>
        /// Height of the control
        /// </summary>
        public string Height
        {
            get
            {
                return _Height.TrimEnd("px".ToCharArray());
            }
            set
            {
                _Height = string.Format("{0}px", value.Trim().TrimEnd("px".ToCharArray()));
            }
        }

        /// <summary>
        /// BorderColor for the control
        /// </summary>
        public string BorderColor
        {
            get
            {
                return _BorderColor;
            }
            set
            {
                _BorderColor = value;
            }
        }

        #endregion





        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Value
        {
            get
            {
                if (ViewState["SelectedValue"] == null)
                    return "";
                else
                    return (String)ViewState["SelectedValue"];
            }
            set { ViewState["SelectedValue"] = value; }
        }
        protected override void OnPreRender(EventArgs e)
        {

            base.OnPreRender(e);


            if (!Page.ClientScript.IsClientScriptBlockRegistered("ASCOpenscript"))
            {

                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ASCOpenscript", IncludeOpenDialogScripts(Page, true));
            }

            if (!Page.ClientScript.IsClientScriptBlockRegistered("ASC_Colorpicker"))
            {
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ASC_Colorpicker", IncludeColorPickerDiv(Page, true));
            }

        }



        public string IncludeOpenDialogScripts(Page page, bool putMainScriptToHeader)
        {
            StringBuilder opendialogscript = new StringBuilder();
            if (putMainScriptToHeader)
            {
                page.ClientScript.RegisterClientScriptResource(this.GetType(), "ASC.Web.Controls.ColorPicker.JavaScript.rgbcolor.js");
            }
            else
            {
                string tempLink = "<script type=\"text/javascript\" src='{0}'></script>";
                string location = page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.ColorPicker.JavaScript.rgbcolor.js");
                opendialogscript.AppendLine(String.Format(tempLink, location));
            }

            
            opendialogscript.Append("<script type=\"text/javascript\">\n");
            opendialogscript.Append("    var ActiveColorPicker;\n");
            opendialogscript.Append("    function OpenColorPicker(event, id)\n");
            opendialogscript.Append("    {\n");
            opendialogscript.Append("        ActiveColorPicker=id; \n");
            opendialogscript.AppendFormat("        var colorpickerdiv=document.getElementById('{0}_ColorPicker');\n", this.ClientID);
            opendialogscript.Append("        colorpickerdiv.style.visibility='visible';\n");
            opendialogscript.Append("        colorpickerdiv.style.left=ActiveColorPicker.style.left+ActiveColorPicker.style.width;\n");
            opendialogscript.Append("        colorpickerdiv.style.top=event.clientY + 'px';\n");
            opendialogscript.Append("        colorpickerdiv.style.left=event.clientX + 'px';\n");
            opendialogscript.Append("    }\n");
            opendialogscript.Append("    function ColorPickerOnColorClick(e)\n");
            opendialogscript.Append("    {\n");
            opendialogscript.Append("        var td = (e.target) ? e.target : e.srcElement; \n");
            opendialogscript.Append("        var color=td.style.backgroundColor; \n");
            opendialogscript.Append("        ActiveColorPicker.style.backgroundColor=color;\n");
            opendialogscript.Append("        ActiveColorPicker.style.color=color;\n");
            opendialogscript.Append("        ActiveColorPicker.value=(new RGBColor(color).toHex());\n");
            opendialogscript.AppendFormat("        var colorpickerdiv=document.getElementById('{0}_ColorPicker');\n", this.ClientID);
            opendialogscript.Append("        colorpickerdiv.style.visibility='hidden';\n");

            if (_AutoPostBack)
                opendialogscript.Append("        " + page.ClientScript.GetPostBackEventReference(this, string.Empty) + ";\n");

            opendialogscript.Append("    }\n");
            opendialogscript.Append("</script>\n");

            return opendialogscript.ToString();
        }

        public string IncludeColorPickerDiv(Page page, bool putCssToHeader)
        {
            string tempLink = "<link rel='stylesheet' text='text/css' href='{0}' />";
            string location = page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.ColorPicker.Css.ColorPicker.css");

            StringBuilder colorpickerdiv = new StringBuilder();

            if(putCssToHeader)
            {
                LiteralControl include = new LiteralControl(String.Format(tempLink, location));
                ((System.Web.UI.HtmlControls.HtmlHead)page.Header).Controls.Add(include);
            }
            else
            {
                colorpickerdiv.AppendLine(String.Format(tempLink, location));
            }
            

            //page.ClientScript.RegisterClientScriptInclude("AKeyMane",
            //    page.ClientScript.GetWebResourceUrl(typeof(ColorPicker), "ASC.Web.Controls.ColorPicker.Css.ColorPicker.css"));

            
            colorpickerdiv.AppendFormat("<div id=\"{0}_ColorPicker\" class=\"colorpicker\">", this.ClientID);
            colorpickerdiv.Append("     <table cellpadding=\"0\" cellspacing=\"0\" onclick=\"javascript:ColorPickerOnColorClick(event);\">");
            string[] websafecolorlist = {"FFFFFF","CCCCCC","999999","666666","333333","000000","FFCC00","FF9900","FF6600","FF3300","FFFFFF","FFFFFF","FFFFFF","FFFFFF","FFFFFF","FFFFFF",
                    "99CC00","FFFFFF","FFFFFF","FFFFFF","FFFFFF","CC9900","FFCC33","FFCC66","FF9966","FF6633","CC3300","FFFFFF","FFFFFF","FFFFFF","FFFFFF","CC0033",
                    "CCFF00","CCFF33","333300","666600","999900","CCCC00","FFFF00","CC9933","CC6633","330000","660000","990000","CC0000","FF0000","FF3366","FF0033",
                    "99FF00","CCFF66","99CC33","666633","999933","CCCC33","FFFF33","996600","996600","663333","993333","CC3333","FF3333","CC3366","FF6699","FF0066",
                    "66FF00","99FF66","66CC33","669900","999966","CCCC66","FFFF66","996633","663300","996666","CC6666","FF6666","990033","CC3399","FF66CC","FF0099",
                    "33FF00","66FF33","339900","66CC00","99FF33","CCCC99","FFFF99","CC9966","CC6600","CC9999","FF9999","FF3399","CC0066","990066","FF33CC","FF00CC",
                    "00CC00","33CC00","336600","669933","99CC66","CCFF99","FFFFCC","FFCC99","FF9933","FFCCCC","FF99CC","CC6699","993366","660033","CC0099","330033",
                    "33CC33","66CC66","00FF00","33FF33","66FF66","99FF99","CCFFCC","FFFFFF","FFFFFF","FFFFFF","CC99CC","996699","993399","990099","663366","660066",
                    "006600","336633","009900","339933","669966","99CC99","FFFFFF","FFFFFF","FFFFFF","FFCCFF","FF99FF","FF66FF","FF33FF","FF00FF","CC66CC","CC33CC",
                    "003300","00CC33","006633","339966","66CC99","99FFCC","CCFFFF","3399FF","99CCFF","CCCCFF","CC99FF","9966CC","663399","330066","9900CC","CC00CC",
                    "00FF33","33FF66","009933","00CC66","33FF99","99FFFF","99CCCC","0066CC","6699CC","9999FF","9999CC","9933FF","6600CC","660099","CC33FF","CC00FF",
                    "00FF66","66FF99","33CC66","009966","66FFFF","66CCCC","669999","003366","336699","6666FF","6666CC","666699","330099","9933CC","CC66FF","9900FF",
                    "00FF99","66FFCC","33CC99","33FFFF","33CCCC","339999","336666","006699","003399","3333FF","3333CC","333399","333366","6633CC","9966FF","6600FF",
                    "00FFCC","33FFCC","00FFFF","00CCCC","009999","006666","003333","3399CC","3366CC","0000FF","0000CC","000099","000066","000033","6633FF","3300FF",
                    "00CC99","FFFFFF","FFFFFF","FFFFFF","FFFFFF","0099CC","33CCFF","66CCFF","6699FF","3366FF","0033CC","FFFFFF","FFFFFF","FFFFFF","FFFFFF","3300CC",
                    "FFFFFF","FFFFFF","FFFFFF","FFFFFF","FFFFFF","FFFFFF","00CCFF","0099FF","0066FF","0033FF","FFFFFF","FFFFFF","FFFFFF","FFFFFF","FFFFFF"};
            int rowcounter = 0;
            bool needrowstart = true;
            for (int i = 0; i < websafecolorlist.Length; i++)
            {
                if (needrowstart)
                {
                    needrowstart = false;
                    colorpickerdiv.Append("            <tr> ");
                }
                string color = "#" + websafecolorlist[i];
                colorpickerdiv.Append("                <td style=\"background-color:" + color + "\"></td>");
                if (rowcounter++ == 16)
                {
                    colorpickerdiv.Append("            </tr>");
                    needrowstart = true;
                    rowcounter = 0;
                }
            }
            colorpickerdiv.Append("     </table>");
            colorpickerdiv.Append("</div>");

            return colorpickerdiv.ToString();
        }

        protected override void Render(HtmlTextWriter output)
        {

            string html;
            string onclicker = "onclick=\"javascript:OpenColorPicker(event, this);\"";
            html = string.Format("<input style=\"width:{0}; height:{1}; border-color:{2};background-color:{3}; color:{3};\" {4} class=\"colorpickerbutton\" readonly=\"true\" type=\"text\" id =\"{5}\" name=\"{6}\" value=\"{3}\">",
                _Width, _Height, _BorderColor, Value, onclicker, this.ClientID, this.UniqueID);
            output.Write(html);
        }

        public bool LoadPostData(String postDataKey, NameValueCollection values)
        {
            if (Value != values[this.UniqueID])
            {
                Value = values[this.UniqueID];

                if (ColorChanged != null)
                    ColorChanged(this, new ColorPickerColorChangedArgs(ColorTranslator.FromHtml(Value)));
            }

            return false;
        }



        public void RaisePostDataChangedEvent()
        {
        }


    }
}

