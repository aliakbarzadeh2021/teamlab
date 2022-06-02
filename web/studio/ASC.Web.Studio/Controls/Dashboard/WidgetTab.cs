using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxPro;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Controls;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Controls.Dashboard.Dao;
using ASC.Web.Studio.Controls.Dashboard.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Controls.Dashboard
{
    public enum ColumnSchemaType
    {
        Schema_25_50_25 = 1,
        Schema_33_33_33 = 2,
    }

    [AjaxNamespace("WidgetControl")]
    public class WidgetTab : WebControl
    {
        private static Dictionary<Guid, WidgetContainer> defaultContainers = new Dictionary<Guid, WidgetContainer>();

        private Guid containerID;
        private string behaviorID;
        private ColumnSchemaType columnSchemaType;
        private Guid widgetContainerId;

        private Container container;
        private List<Guid> displayWidgets = new List<Guid>();
        private Dictionary<Type, IWidgetSettingsProvider> providers = new Dictionary<Type, IWidgetSettingsProvider>();


        public bool Settings
        {
            get;
            set;
        }

        public List<Widget> WidgetCollection
        {
            get;
            private set;
        }


        public WidgetTab()
        {
            WidgetCollection = new List<Widget>();
            Settings = true;
        }

        public WidgetTab(Guid containerID, ColumnSchemaType columnSchemaType, string behaviorID)
            : this()
        {
            this.containerID = containerID;
            this.columnSchemaType = columnSchemaType;
            this.behaviorID = behaviorID;
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.ClientScript.RegisterClientScriptInclude(GetType(), "studio_dashedboard_script", WebPath.GetPath("Controls/Dashboard/js/dashed_board.js"));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            InitDefaultContainer(containerID, columnSchemaType, WidgetCollection);

            var widgetContainer = WidgetManager.GetWidgetContainer(containerID, TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID) ??
                GetDefaultContainer(containerID, Guid.NewGuid());

            columnSchemaType = widgetContainer.ColumnSchemaType;
            widgetContainerId = widgetContainer.ID;

            var widgets = new List<Widget>();
            foreach (var widget in WidgetCollection)
            {
                var state = widgetContainer.States.Find(s => s.ID == widget.WidgetID);
                if (state != null)
                {
                    widget.InitWidgetState(state, behaviorID);
                    widgets.Add(widget);
                    displayWidgets.Add(widget.WidgetID);
                }
            }

            widgets.Sort((x1, x2) => x1.Position.X != x2.Position.X ? x1.Position.X.CompareTo(x2.Position.X) : x1.Position.Y.CompareTo(x2.Position.Y));
            widgets.ForEach(w => Controls.Add(w));

            container = new Container() { Header = new PlaceHolder(), Body = new PlaceHolder(), Options = new StyleOptions() };
            Controls.Add(container);

            Page.ClientScript.RegisterStartupScript(
                GetType(),
                Guid.NewGuid().ToString(),
                string.Format(" var {0} = new DashedBoard('{0}','{1}','{2}'); ", behaviorID, widgetContainerId, containerID),
                true);
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.Write(RenderSettingsBox());
            writer.Write("<div class='studioWidgetSetContent clearFix' id='widgetContainer_" + widgetContainerId + "'>");
            writer.Write("<div class='clearFix' style='padding:0; margin:0px;'>");
        }

        

        protected override void RenderContents(HtmlTextWriter writer)
        {
            var sb = new StringBuilder();
            int i = 0;
            foreach (var percent in WidgetManager.GetColumnSchemaPercents(columnSchemaType))
            {
                sb.Append("<ul id='widgetCol_" + widgetContainerId + "_" + i + "' name='" + i + "' " + (i == 0 ? "class=\"studioFirstColunm\"" : "") + " style=\"list-style-type: none; padding:0px 0px 5px 0px; margin:0; float:left;  width:" + percent + "%;\" >");
                foreach (var control in Controls)
                {
                    var widget = control as Widget;
                    if (widget != null && widget.WidgetState.X == i)
                    {
                        writer.Write(sb.ToString());
                        using (var stringWriter = new StringWriter())
                        {
                            using (var tempWriter = new HtmlTextWriter(stringWriter))
                                try
                                {
                                    //NOTE: we do such thing to avoid writing unnesesary elements to output
                                    widget.RenderControl(tempWriter);
                                    writer.Write(stringWriter.GetStringBuilder().ToString());
                                }
                                catch (Exception e)
                                {
                                    var broken = Page.LoadControl(BrokenWidget.Path) as BrokenWidget;
                                    widget.RenderBeginTag(writer);
                                    if (broken != null)
                                    {
                                        broken.Exception = e;
                                        broken.RenderControl(writer);
                                    }
                                    widget.RenderEndTag(writer);
                                }
                        }
                        sb = new StringBuilder();
                    }
                }

                sb.Append("</ul>");
                i++;
            }
            writer.Write(sb.ToString());
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.Write("</div>");
            writer.Write("</div>");
        }

        private IWidgetSettingsProvider GetSettingsProvider(string typeName)
        {
            try
            {
                return !string.IsNullOrEmpty(typeName) ? GetSettingsProvider(Type.GetType(typeName)) : null;
            }
            catch { return null; }
        }

        private IWidgetSettingsProvider GetSettingsProvider(Type providerType)
        {
            try
            {
                if (!providers.ContainsKey(providerType))
                {
                    providers[providerType] = (IWidgetSettingsProvider)Activator.CreateInstance(providerType);
                }
                return providers[providerType];
            }
            catch { return null; }
        }

        private string RenderWidgetSettings(Widget widget)
        {
            var checkedState = displayWidgets.Contains(widget.WidgetID) ? "checked='checked'" : string.Empty;
            var sb = new StringBuilder()
                .Append("<div class='clearFix'>")
                .Append("<input type='hidden' value='" + (widget.SettingsProviderType != null ? widget.SettingsProviderType.AssemblyQualifiedName : "") + "' id='widgetProviderType_" + widgetContainerId + "_" + widget.WidgetID + "'/>")
                .Append("<div style='float:left;'><input " + checkedState + " value=\"" + widget.WidgetID + "\" id=\"widgetItem_" + widgetContainerId + "_" + widget.WidgetID + "\" type=\"checkbox\"/></div>")
                .Append("<div style='float:left; width:90%; margin-left:5px;'>")
                .Append("<div style='margin-top:2px;'><label style='font-weight:bolder;' for=\"widgetItem_" + widgetContainerId + "_" + widget.WidgetID + "\">" + HttpUtility.HtmlEncode(widget.Name) + "</label></div>")
                .Append("<div class='textMediumDescribe' style='margin:3px 0px;'> " + HttpUtility.HtmlEncode(widget.Description) + "</div>");

            var provider = GetSettingsProvider(widget.SettingsProviderType);
            if (provider != null)
            {
                var settings = provider.Load(widget.WidgetID, SecurityContext.CurrentAccount.ID);
                if (settings != null && 0 < settings.Count)
                {
                    foreach (var setting in settings)
                    {
                        sb.Append("<div class='clearFix' style='margin:7px 0px;'>");
                        if (setting is NumberWidgetSettings)
                        {
                            sb.Append("<div style='float:left; width:45px;'><input id='widgetSettings_" + widgetContainerId + "_" + widget.WidgetID + "_" + setting.ID + "' value='" + (setting as NumberWidgetSettings).Value + "' class='textEdit' style='width:40px;' type='text'/></div>");
                            sb.Append("<div style='float:left; width:230px; margin-left:5px;'>" + setting.Title.HtmlEncode() + "</div>");
                        }
                        else if (setting is BoolWidgetSettings)
                        {
                            string state = (setting as BoolWidgetSettings).Value ? "checked='checked'" : "";
                            sb.Append("<div style='float:left; width:45px;'><input " + state + " id='widgetSettings_" + widgetContainerId + "_" + widget.WidgetID + "_" + setting.ID + "' type='checkbox'/></div>");
                            sb.Append("<div style='float:left; width:230px; margin-left:5px;'><label for='widgetSettings_" + widgetContainerId + "_" + widget.WidgetID + "_" + setting.ID + "'>" + setting.Title.HtmlEncode() + "</label></div>");
                        }
                        sb.Append("</div>");
                    }
                }
            }
            sb.Append("</div></div>");

            return sb.ToString();
        }

        private string RenderSettingsBox()
        {
            var sb = new StringBuilder()
                .AppendFormat("<div id='widgetSettingsDialogMessage_{0}' class='infoPanel alert' style='margin:10px 0;'></div>", widgetContainerId)
                .Append("<div style='display:none;'>")
                .AppendFormat("<div class='headerPanel' style='margin:5px 0px;'>{0}</div>", Resources.Resource.WidgetTabName)
                .Append("</div>")
                .AppendFormat("<div class='headerPanel' style='margin:-bottom:10px;'>{0}</div>", Resources.Resource.WidgetSettingsTitle)
                .Append("<div class='clearFix' style='margin-bottom:10px;'>");

            int i = 0;
            foreach (var w in WidgetCollection)
            {
                if (i == 2)
                {
                    sb.Append("</div>");
                    sb.Append("<div class='clearFix' style='margin-bottom:10px;'>");
                    i = 0;
                }
                sb.Append("<div style='float:left; width:49%; margin-left:5px;'>");
                sb.Append(RenderWidgetSettings(w));
                sb.Append("</div>");
                i++;
            }

            sb.Append("</div>");
            sb.Append("<div id='widgetSettings_panel_buttons' class='clearFix' style='margin-top:16px;'>");
            sb.Append("<a style='float:left;' class='baseLinkButton' href=\"javascript:" + behaviorID + ".SaveSettings();\">" + Resources.Resource.SaveButton + "</a>");
            sb.Append("<a style='float:left; margin-left:8px;' class='grayLinkButton' href=\"javascript:" + behaviorID + ".CancelSettings();\">" + Resources.Resource.CancelButton + "</a>");
            sb.Append("</div>");

            sb.Append("<div style='margin-top: 16px; display: none;' id='widgetSettings_action_loader' class='clearFix'>");
            sb.AppendFormat("<div class='textMediumDescribe'>{0}</div>", Resources.Resource.PleaseWaitMessage);
            sb.Append("<img src='" + ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif") + "'>");
            sb.Append("</div>");

            container.Body.Controls.Add(new Literal() { Text = sb.ToString() });
            container.Header.Controls.Add(new Literal() { Text = Resources.Resource.WidgetSettingsDialogTitle });
            container.Options.IsPopup = true;

            var sw = new StringWriter();
            var htw = new HtmlTextWriter(sw);
            container.RenderControl(htw);

            return "<div id='widgetSettingsDialog_" + widgetContainerId + "' style='display:none;'>" + sw.ToString() + "</div>";
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string SortWidgets(Guid widgetContainerID, Guid defaultContainerID, string positions)
        {
            if (SetupInfo.WorkMode == WorkMode.Promo) return "ok";

            var wc = WidgetManager.GetWidgetContainer(widgetContainerID) ?? GetDefaultContainer(defaultContainerID, widgetContainerID);
            if (wc != null)
            {
                foreach (string column_part in positions.Split('@'))
                {
                    var column_id = column_part.Split('$')[0];
                    var widget_ids = column_part.Split('$')[1];
                    var x = 0;
                    var y = 0;
                    if (Int32.TryParse(column_id, out x))
                    {
                        foreach (var widgetID in widget_ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var state = wc.States.Find(s => s.ID == new Guid(widgetID));
                            if (state != null)
                            {
                                state.X = x;
                                state.Y = y;
                                y++;
                            }
                        }
                    }
                }
                return WidgetManager.SaveWidgetContainer(wc) ? "ok" : "error";
            }
            return "tab not found";
        }

        public class WidgetItem
        {
            public Guid ID { get; set; }
            public string SettingsProviderType { get; set; }
            public List<WidgetSettings> Settings { get; set; }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveSettings(Guid defaultContainerID, Guid widgetContainerID, int schemaID, string widgetContainerName, List<WidgetItem> widgetItems)
        {
            try
            {
                if (widgetItems == null) return new { ContainerID = widgetContainerID, Success = 0, Message = Resources.Resource.ErrorSaveSettings };

                var wc = WidgetManager.GetWidgetContainer(widgetContainerID) ?? GetDefaultContainer(defaultContainerID, widgetContainerID);
                wc.States.RemoveAll(s => !widgetItems.Exists(w => w.ID == s.ID));
                foreach (var w in widgetItems)
                {
                    if (!wc.States.Exists(s => s.ID == w.ID))
                    {
                        var defaultState = GetDefaultState(defaultContainerID, w.ID);
                        if (defaultState != null)
                        {
                            wc.States.Add(new WidgetState(w.ID, wc.ID) { X = defaultState.X, Y = defaultState.Y });
                        }
                    }

                    var errorMessage = string.Empty;
                    var settingsProvider = GetSettingsProvider(w.SettingsProviderType);
                    if (settingsProvider != null && settingsProvider.Check(w.Settings, w.ID, SecurityContext.CurrentAccount.ID, out errorMessage) == false)
                    {
                        return new { ContainerID = widgetContainerID, Success = 0, Message = errorMessage.HtmlEncode() };
                    }
                }
                WidgetManager.SaveWidgetContainer(wc);

                foreach (var w in widgetItems)
                {
                    var settingsProvider = GetSettingsProvider(w.SettingsProviderType);
                    if (settingsProvider != null)
                    {
                        settingsProvider.Save(w.Settings, w.ID, SecurityContext.CurrentAccount.ID);
                    }
                }
                return new { ContainerID = widgetContainerID, Success = 1, Message = Resources.Resource.SuccessfullySaveSettingsMessage };
            }
            catch
            {
                return new { ContainerID = widgetContainerID, Success = 0, Message = Resources.Resource.ErrorSaveSettings };
            }
        }

        private void InitDefaultContainer(Guid containerID, ColumnSchemaType schema, List<Widget> widgets)
        {
            if (!defaultContainers.ContainsKey(containerID))
            {
                lock (defaultContainers)
                {
                    if (!defaultContainers.ContainsKey(containerID))
                    {
                        var widgetContainer = new WidgetContainer(Guid.Empty, containerID)
                        {
                            ColumnSchemaType = columnSchemaType,
                        };
                        foreach (var widget in widgets)
                        {
                            var state = new WidgetState(widget.WidgetID, widgetContainer.ID);
                            state.X = widget.Position.X;
                            state.Y = widget.Position.Y;
                            widgetContainer.States.Add(state);
                        }
                        defaultContainers[containerID] = widgetContainer;
                    }
                }
            }
        }

        private WidgetContainer GetDefaultContainer(Guid defaultId, Guid newId)
        {
            var template = defaultContainers[defaultId];
            var newContainer = new WidgetContainer(newId, template.ContainerID)
            {
                ColumnSchemaType = template.ColumnSchemaType,
                TenantID = TenantProvider.CurrentTenantID,
                UserID = SecurityContext.CurrentAccount.ID,
            };
            template.States.ForEach(s => newContainer.States.Add(new WidgetState(s.ID, newContainer.ID) { X = s.X, Y = s.Y }));
            return newContainer;
        }

        private WidgetState GetDefaultState(Guid defaultContainerID, Guid id)
        {
            if (!defaultContainers.ContainsKey(defaultContainerID)) return null;
            var template = defaultContainers[defaultContainerID];
            return template.States.Find(s => s.ID == id);
        }
    }
}
