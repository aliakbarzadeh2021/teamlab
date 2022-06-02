using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Controls.BBCodeParser;
using ASC.Web.Controls.CommentInfoHelper;
using System.Linq;
using ASC.Web.Controls.CalendarInfoHelper;
using System.Globalization;
using System.Threading;
using AjaxPro;
using ASC.Core;
using ASC.Data.Storage;
using System.IO;
using System.Web.UI.HtmlControls;

namespace ASC.Web.Controls
{
    [ToolboxData("<{0}:Calendar runat=server></{0}:Calendar>")]
    public class Calendar : Control
    {
        #region Properties

        public List<CalendarEvent> CalendarEvents { get; set; }
        public string AjaxCreateNewEventFunctionName { get; set; }
        public string AjaxDeleteEventFunctionName { get; set; }
        public string AjaxEditEventFunctionName { get; set; }
        public int Action { get; set; }
        public bool ViewColorPicker { get; set; }
        public bool ViewICalFiles { get; set; }
        public IDataStore Store { get; set; }
        public Type ClassType { get; set; }
        
        protected Literal ltScript;

        #endregion

        #region Methods

        public void RegisterClientScripts()
        {
            RegisterScriptFile();
            RegisterStyleFile();
            RegisterDays();
            RegisterMonths();
            RegisterEvents();
            RegisterImages();
            RegisterJavascriptFunctions();
        }
        public void RegisterScriptFile()
        {
            if (!Page.ClientScript.IsClientScriptBlockRegistered("calendarScript"))
            {
                string scriptLocation = Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.Calendar.js.calendar.js");
                Page.ClientScript.RegisterClientScriptInclude("calendarScript", scriptLocation);
            }
            if (!Page.ClientScript.IsClientScriptBlockRegistered("jscolorScript"))
            {
                string jscolorScriptLocation = Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.Calendar.js.jscolor.js");
                Page.ClientScript.RegisterClientScriptInclude("jscolorScript", jscolorScriptLocation);
            }
        }
        public void RegisterStyleFile()
        {
            if (!Page.ClientScript.IsClientScriptBlockRegistered("calendarStyle"))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("<link rel=\"stylesheet\" text=\"text/css\" href=\"{0}\" />", Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.Calendar.css.calendar.css"));

                ((System.Web.UI.HtmlControls.HtmlHead)Page.Header).Controls.Add(new LiteralControl(sb.ToString()));
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "calendarStyle", "");
            }
        }
        public void RegisterMonths()
        {
            string[] months = new string[12];
            DateTime date = DateTime.MinValue;

            for (int i = 0; i < 12; i++)
            {
                months[i] = date.ToString("MMMM");
                date = date.AddMonths(1);
            }

            if (!Page.ClientScript.IsClientScriptBlockRegistered("566F11FF-7848-404c-891F-2A11B9C66703"))
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(string), "566F11FF-7848-404c-891F-2A11B9C66703", "Months = " +
                                                                JavaScriptSerializer.Serialize(months) + ";", true);
            }
        }
        public void RegisterEvents()
        {
            if (!Page.ClientScript.IsClientScriptBlockRegistered("44BCF169-7824-4a8a-8388-E19D2D2174D2"))
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(CalendarEvent), "44BCF169-7824-4a8a-8388-E19D2D2174D2", "Events = " +
                                                                JavaScriptSerializer.Serialize(CalendarEvents) + ";", true);
            }


            List<string> isAlreadyWrite = new List<string>();
            foreach (CalendarEvent calendarEvent in CalendarEvents)
            {
                isAlreadyWrite.Add("0,16");
            }

            if (!Page.ClientScript.IsClientScriptBlockRegistered("7E9AECBD-B080-4a20-9A14-7169B7212DD9"))
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(string), "7E9AECBD-B080-4a20-9A14-7169B7212DD9", "IsAlreadyWrite = " +
                                                                JavaScriptSerializer.Serialize(isAlreadyWrite) + ";", true);
            }
        }
        public void RegisterDays()
        {
            List<WrapperDayOfWeek> list = new List<WrapperDayOfWeek>();
            DateTime date = new DateTime();

            CultureInfo info = Thread.CurrentThread.CurrentCulture;
            DayOfWeek firstday = info.DateTimeFormat.FirstDayOfWeek;

            if (firstday == DayOfWeek.Monday)
            {
                for (int i = 1; i < 8; i++)
                {
                    date = new DateTime(2009, 12, 6 + i);
                    WrapperDayOfWeek item = new WrapperDayOfWeek();
                    item.title = date.ToString("dddd");
                    if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                        item.cssClass = "wbs_weekend";
                    else item.cssClass = string.Empty;
                    list.Add(item);
                }
            }

            if (firstday == DayOfWeek.Sunday)
            {
                for (int i = 1; i < 8; i++)
                {
                    date = new DateTime(2009, 12, 5 + i);
                    WrapperDayOfWeek item = new WrapperDayOfWeek();
                    item.title = date.ToString("dddd");
                    if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                        item.cssClass = "wbs_weekend";
                    else item.cssClass = string.Empty;
                    list.Add(item);
                }
            }

            if (!Page.ClientScript.IsClientScriptBlockRegistered("243AD9FE-AA56-4119-8AFF-79C20B0EFFF9"))
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(WrapperDayOfWeek), "243AD9FE-AA56-4119-8AFF-79C20B0EFFF9", "daysOfWeek = " +
                                                                JavaScriptSerializer.Serialize(list) + ";", true);
            }
            if (!Page.ClientScript.IsClientScriptBlockRegistered("DD724067-A53D-45fb-A9BE-8FC82BC4FB98"))
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(int), "DD724067-A53D-45fb-A9BE-8FC82BC4FB98", "firstDayOfWeekInCurrentCulture = " +
                                                                JavaScriptSerializer.Serialize((int)firstday) + ";", true);
            }
        }
        public void RegisterImages()
        {
            List<string> images = new List<string>();

            images.Add("url('" + Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.Calendar.images.arrow.gif") + "') no-repeat");
            images.Add("url('" + Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.Calendar.images.cross.gif") + "') no-repeat");
            images.Add("url('" + Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.Calendar.images.hs.png") + "') 0 0 no-repeat");
            images.Add("url('" + Page.ClientScript.GetWebResourceUrl(this.GetType(), "ASC.Web.Controls.Calendar.images.hv.png") + "') 0 0 no-repeat");

            if (!Page.ClientScript.IsClientScriptBlockRegistered("D9763D53-8E60-4cc7-97CB-7A63378892BE"))
            {
                Page.ClientScript.RegisterClientScriptBlock(typeof(string), "D9763D53-8E60-4cc7-97CB-7A63378892BE", "Images = " +
                                                                JavaScriptSerializer.Serialize(images) + ";", true);
            }
        }
        public void RegisterJavascriptFunctions()
        {
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<script language='javascript'>");

            sb.AppendLine("function createNewEvent(){");
            sb.AppendLine("var title = jq('#eventTitle').val();");
            sb.AppendLine("if(title=='') { alert('" + Resources.CalendarResource.TitleIsNullOrEmpty+"');return; } ");
            sb.AppendLine("if(!Calendar.ValidDate()) { alert('" + Resources.CalendarResource.IncorrectDate + "');return; } ");
            sb.AppendLine(AjaxCreateNewEventFunctionName + "(Calendar.createEventRequest(),function(res){");
            sb.AppendLine("if (res.error != null){alert(res.error.Message);return;}");
            sb.AppendLine("Events.push(eval('('+res.value+')'));");
            sb.AppendLine("IsAlreadyWrite.push(0);");

            if (Action == 2) sb.AppendLine("Calendar.drawWeek();");
            else sb.AppendLine("Calendar.drawMonth();");

            sb.AppendLine("Calendar.closePopup();");
            sb.AppendLine("})}");
            sb.AppendLine("");
            
            sb.AppendLine("function deleteEvent(){");
            sb.AppendLine(AjaxDeleteEventFunctionName + "(jq('#selectedEvent').val(),function(res){");
            sb.AppendLine("if (res.error != null){alert(res.error.Message);return;}");
            sb.AppendLine("for(var i=0;i<Events.length;i++){if(Events[i].ID=='event'+jq('#selectedEvent').val()){Events.splice(i,1);IsAlreadyWrite.splice(i,1);break;}}");
            
            if (Action == 2) sb.AppendLine("Calendar.drawWeek();");
            else sb.AppendLine("Calendar.drawMonth();");
            
            sb.AppendLine("Calendar.closePopup();");
            sb.AppendLine("});}");

            sb.AppendLine("function editEvent(){");
            sb.AppendLine("var title = jq('#eventTitle').val();");
            sb.AppendLine("if(title=='') { alert('" + Resources.CalendarResource.TitleIsNullOrEmpty + "');return; } ");
            sb.AppendLine("if(!Calendar.ValidDate()) { alert('" + Resources.CalendarResource.IncorrectDate + "');return; } ");
            sb.AppendLine(AjaxEditEventFunctionName + "(jq('#selectedEvent').val(),Calendar.createEventRequest(),function(res){");
            sb.AppendLine("if (res.error != null){alert(res.error.Message);return;}");
            sb.AppendLine("for(var i=0;i<Events.length;i++){if(Events[i].ID=='event'+jq('#selectedEvent').val()){Events.splice(i,1);break;}}");
            sb.AppendLine("Events.push(eval('('+res.value+')'));");

            if (Action == 2) sb.AppendLine("Calendar.drawWeek();");
            else sb.AppendLine("Calendar.drawMonth();");

            sb.AppendLine("Calendar.closePopup();");
            sb.AppendLine("})}");
            sb.AppendLine("");

            sb.AppendLine("</script>");
            ltScript.Text = sb.ToString();

        }
        //--------------------------------------------------------------------------

        public string GetUploadFileContent(HttpPostedFile postedFile)
        {
            Stream str;
            String strmContents = string.Empty;
            Int32 strLen, strRead;

            // Create a Stream object.
            str = postedFile.InputStream;

            // Find number of bytes in stream.
            strLen = Convert.ToInt32(str.Length);

            // Create a byte array.
            byte[] strArr = new byte[strLen];

            // Read stream into byte array.
            strRead = str.Read(strArr, 0, strLen);

            Encoding _ascii = Encoding.UTF8;
            char[] asciiChars = new char[_ascii.GetCharCount(strArr, 0, strArr.Length)];
            _ascii.GetChars(strArr, 0, strArr.Length, asciiChars, 0);

            string[] _response = new string(asciiChars).Split('_');
            for (int i = 0; i < _response.Length; i++)
                strmContents += _response[i];

            return strmContents;

        }
        public void GetFile()
        {
            Guid user = SecurityContext.CurrentAccount.ID;

            Uri fileUri = Store.GetUri(String.Empty, string.Format("iCal\\{0}.ics", user));

            String filePath = fileUri.ToString();

            Page.Response.ClearContent();
            Page.Response.ClearHeaders();
            Page.Response.ContentType = "text/ics";
            Page.Response.AppendHeader("Content-Disposition", string.Format("attachment; filename={0}.ics", "iCal"));
            Page.Response.TransmitFile(filePath);
            Page.Response.Flush();
            Page.Response.Close();
        }
        public void GenerateICalFile()
        {
            if (CalendarEvents.Count > 0)
            {
                Guid user = SecurityContext.CurrentAccount.ID;

                if (Store.IsFile(String.Empty, string.Format("iCal\\{0}.ics", user)))
                {
                    Store.Delete(string.Format("iCal\\{0}.ics", user));
                }

                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(ConvertListToICal(CalendarEvents))))
                {
                    Store.Save(string.Format("iCal\\{0}.ics", user), ms);
                }
            }
        }
        public string ConvertListToICal(List<CalendarEvent> list)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");

            foreach (CalendarEvent ce in list)
            {
                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine(string.Format("DTSTART:{0}", ConvertDateTimeToString(ce.StartingDate)));
                sb.AppendLine(string.Format("DTEND:{0}", ConvertDateTimeToString(ce.EndingDate)));
                sb.AppendLine(string.Format("DESCRIPTION:{0}", ce.Description));
                sb.AppendLine(string.Format("SUMMARY:{0}", ce.Title));
                sb.AppendLine(string.Format("URL:{0}", ce.EventURl));
                sb.AppendLine("END:VEVENT");
            }

            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }
        public string ConvertDateTimeToString(DateTime date)
        {
            string str = string.Empty;

            str += date.Year.ToString();
            str += date.Month < 10 ? "0" + date.Month.ToString() : date.Month.ToString();
            str += date.Day < 10 ? "0" + date.Day.ToString() : date.Day.ToString();
            str += "T";
            str += date.Hour < 10 ? "0" + date.Hour.ToString() : date.Hour.ToString();
            str += date.Minute < 10 ? "0" + date.Minute.ToString() : date.Minute.ToString();
            str += date.Second < 10 ? "0" + date.Second.ToString() : date.Second.ToString();
            str += "Z";

            return str;
        }
        public DateTime ConvertStringToDateTime(string date, bool onlyDate, bool isEndingDate)
        {
            int year = Convert.ToInt32(date.Substring(0, 4));
            int month = Convert.ToInt32(date.Substring(4, 2));
            int day = Convert.ToInt32(date.Substring(6, 2));
            if (!onlyDate)
            {
                int hour = Convert.ToInt32(date.Substring(9, 2));
                int minute = Convert.ToInt32(date.Substring(11, 2));
                int second = Convert.ToInt32(date.Substring(13, 2));

                return new DateTime(year, month, day, hour, minute, second);
            }
            if (isEndingDate)
                return new DateTime(year, month, day - 1, 23, 59, 59);
            return new DateTime(year, month, day);
        }
        public List<CalendarEvent> GetEventsFromFile(HttpPostedFile postedFile)
        {
            List<CalendarEvent> result = new List<CalendarEvent>();
            string fileContent = GetUploadFileContent(postedFile);
            List<string> events = new List<string>();
            int index = fileContent.IndexOf("BEGIN:VEVENT");
            if (index > -1)
                fileContent = fileContent.Remove(0, index);
            while (fileContent.IndexOf("END:VEVENT") > -1)
            {
                index = fileContent.IndexOf("END:VEVENT") + 10;
                events.Add(fileContent.Substring(0, index).Trim());
                fileContent = fileContent.Remove(0, index);
            }
            foreach (string item in events)
            {
                var properties = item.Split('\n');
                CalendarEvent ce = new CalendarEvent();
                foreach (string property in properties)
                {
                    string str = property.Trim();
                    if (str.StartsWith("URL:"))
                        ce.EventURl = str.Substring(4);
                    if (str.StartsWith("DESCRIPTION:"))
                        ce.Description = str.Substring(12);
                    if (str.StartsWith("SUMMARY:"))
                        ce.Title = str.Substring(8);

                    if (str.StartsWith("CREATED:"))
                        ce.CreatedAt = ConvertStringToDateTime(str.Substring(8), false, false);
                    if (str.StartsWith("DTSTART:"))
                        ce.StartingDate = ConvertStringToDateTime(str.Substring(8), false, false);
                    if (str.StartsWith("DTEND:"))
                        ce.EndingDate = ConvertStringToDateTime(str.Substring(6), false, true);

                    if (str.StartsWith("CREATED;VALUE=DATE:"))
                        ce.CreatedAt = ConvertStringToDateTime(str.Substring(19), true, false);
                    if (str.StartsWith("DTSTART;VALUE=DATE:"))
                        ce.StartingDate = ConvertStringToDateTime(str.Substring(19), true, false);
                    if (str.StartsWith("DTEND;VALUE=DATE:"))
                        ce.EndingDate = ConvertStringToDateTime(str.Substring(17), true, true);
                }
                result.Add(ce);
            }

            return result;
        }
        //--------------------------------------------------------------------------

        //----------html for CalendarView, ListView, WeekView---------------
        public string CalendarView()
        {
            StringBuilder sb = new StringBuilder();
            string url = HttpContext.Current.Request.Path;
            int prjID;
            if (!int.TryParse(HttpContext.Current.Request["prjID"], out prjID)) prjID = 0;
            string additionalParam = string.Empty;
            if (prjID != 0) additionalParam = string.Format("prjID={0}&", prjID);

            sb.Append(@"<script language=""javascript"">");
            sb.Append("window.onload = function()");
            sb.Append("{");
            sb.Append("Calendar.drawMonth();");
            sb.Append("Calendar.Init();");
            sb.Append("Calendar.initPrevMonthButton();");
            sb.Append("Calendar.initNextMonthButton();");
            sb.Append("Calendar.InitViewSwitcher();");
            sb.Append("};");
            sb.Append(@"</script>");

            sb.Append("<div class='clearFix borderBase' id='filterBlock'>");
            sb.AppendFormat("<span class='viewSwitcherItem' id='vsItem_0'><a class='linkAction' href='{0}?{2}action=0'>{1}</a></span>", url, Resources.CalendarResource.Month, additionalParam);
            sb.AppendFormat("<span class='viewSwitcherItem' id='vsItem_2'><a class='linkAction' href='{0}?{2}action=2'>{1}</a></span>", url, Resources.CalendarResource.Week, additionalParam);
            sb.AppendFormat("<span class='viewSwitcherItem' id='vsItem_1'><a class='linkAction' href='{0}?{2}action=1'>{1}</a></span>", url, Resources.CalendarResource.EventsList, additionalParam);
            //iCal
            if (ViewICalFiles)
            {
                sb.Append("<input type='file' name='uploadICalFile'/><span class='splitter'></span>");
                sb.AppendFormat("<a onclick='javascript:__doPostBack(\"uploadICalFile\",\"\");' class='linkAction' href='javascript:void(0);'>{0}</a><span class='splitter'></span>", Resources.CalendarResource.UploadFile);
                sb.AppendFormat("<a onclick='javascript:__doPostBack(\"getICalFile\",\"\");' class='linkAction' href='javascript:void(0);'>{0}</a>", Resources.CalendarResource.ICal);
            }
            sb.Append("</div>");
            
            
            sb.Append("<div class='clearFix'>");
            sb.Append("<div class='calendar-navigation-panel calendar-font-11'>");
            sb.AppendFormat("<a id='prevMonthButton' class='calendar-navigation-panel-left r' href='#' onclick='javascript: return false;'>{0}</a>", Resources.CalendarResource.PreviousMonth);
            sb.Append("<div class='calendar-navigation-panel-title'></div>");
            sb.AppendFormat("<a id='nextMonthButton' class='calendar-navigation-panel-right r' href='#' onclick='javascript: return false;'>{0}</a>", Resources.CalendarResource.NextMonth);
            sb.Append("</div>");
            sb.Append("<div id='jTemplatesContainer'>&nbsp;</div>");
            sb.Append("</div>");

            
            sb.Append("<div class='popupContainerClass' id='popup'>");
            sb.AppendFormat("<div class='containerHeaderBlock'><span id='createHeader'>{0}</span><span id='editHeader'>{1}</span></div>", Resources.CalendarResource.CreateNewEvent, Resources.CalendarResource.EditingEvents);
            sb.Append("<div class='containerBodyBlock'>");
            sb.Append("<div class='pm-headerPanelSmall-splitter'>");
            sb.AppendFormat("<div class='headerPanelSmall'>{0}</div>", Resources.CalendarResource.Title);
            sb.Append("<input class='textEdit' id='eventTitle' type='text' style='width:99%;' value='' />");
            sb.Append("</div>");
            sb.Append("<div class='pm-headerPanelSmall-splitter'>");
            sb.AppendFormat("<div class='headerPanelSmall'>{0}</div>", Resources.CalendarResource.StartingDate);
            sb.Append("<select id='eventStartingDateYear' class='comboBox' style='width:60px;'></select>");
            sb.Append("<select id='eventStartingDateMonth' class='comboBox' style='width:100px;'></select>");
            sb.Append("<select id='eventStartingDateDay' class='comboBox' style='width:45px;'></select>");
            sb.Append("<span class='button-splitter'></span>");
            sb.Append("<select id='eventStartingDateHour' class='comboBox' style='width:45px;'></select>");
            sb.Append("<select id='eventStartingDateMinute' class='comboBox' style='width:45px;'></select>");
            sb.Append("</div>");
            sb.Append("<div class='pm-headerPanelSmall-splitter'>");
            sb.AppendFormat("<div class='headerPanelSmall'>{0}</div>", Resources.CalendarResource.EndingDate);
            sb.Append("<select id='eventEndingDateYear' class='comboBox' style='width:60px;'></select>");
            sb.Append("<select id='eventEndingDateMonth' class='comboBox' style='width:100px;'></select>");
            sb.Append("<select id='eventEndingDateDay' class='comboBox' style='width:45px;'></select>");
            sb.Append("<span class='button-splitter'></span>");
            sb.Append("<select id='eventEndingDateHour' class='comboBox' style='width:45px;'></select>");
            sb.Append("<select id='eventEndingDateMinute' class='comboBox' style='width:45px;'></select>");
            sb.Append("</div>");
            if (ViewColorPicker)
            {
                sb.Append("<div class='pm-headerPanelSmall-splitter'>");
                sb.AppendFormat("<div class='headerPanelSmall'>{0}</div>", Resources.CalendarResource.Color);
                sb.Append("<input type='text' class='color textEdit' style='width:99%;' value='66ff00' id='eventColor'>");
                sb.Append("</div>");
            }
            sb.Append("<div class='pm-h-line' ><!– –></div>");
            sb.Append("<div style='text-align:left;'>");
            sb.AppendFormat("<span id='createEventButton'><a href='javascript:void(0)' onclick=\"createNewEvent()\" class='baseLinkButton'>{0}</a>", Resources.CalendarResource.CreateEvent);
            sb.Append("<span class='button-splitter'></span></span>");
            sb.AppendFormat("<span id='editEventButton'><a href='javascript:void(0)' onclick=\"editEvent()\" class='baseLinkButton'>{0}</a>", Resources.CalendarResource.EditEvent);
            sb.Append("<span class='button-splitter'></span></span>");
            sb.AppendFormat("<span id='deleteEventButton'><a href='javascript:void(0)' onclick=\"deleteEvent()\" class='baseLinkButton'>{0}</a>", Resources.CalendarResource.DeleteEvent);
            sb.Append("<span class='button-splitter'></span></span>");
            sb.AppendFormat("<a href='javascript:void(0)' onclick='Calendar.closePopup();' class='grayLinkButton'>{0}</a>", Resources.CalendarResource.Cancel);
            sb.Append("<input type='hidden' id='selectedEvent' value='-1'>");
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("</div>");


            
            sb.Append("<p style='display:none'>");
            
            sb.Append("<textarea id='calendarTemlate' rows='0' cols='0'>");

            sb.Append("<div class='calendar-week-days'>");
            sb.Append("{#foreach $T.daysOfWeek_table as dayOfWeek}");
            sb.Append("<div>{$T.dayOfWeek.title}</div>");
            sb.Append("{#/for}");
            sb.Append("</div>");

            sb.Append("<div class='wbs_month' id='month'>");
            sb.Append("{#foreach $T.daysOfMonth as dayOfMonth}");
            sb.Append("{#if (($T.dayOfMonth.index-1) % 7 == 0) || ($T.dayOfMonth.index == 1)}");
            sb.Append("<div class='wbs_week'>");
            sb.Append("{#/if}");
            sb.Append("<div class='wbs_day {$T.dayOfMonth.cssToday}' id='cell{$T.dayOfMonth.index}' onclick='{#if $T.dayOfMonth.dayNumber != ''}Calendar.click(this){#/if}'>");
            sb.Append("{#if $T.dayOfMonth.dayNumber != ''}<script>Calendar.writeEvents(jq('#cell{$T.dayOfMonth.index}').parent(),new Date(Calendar.CurrentDate.Year,Calendar.CurrentDate.Month,{$T.dayOfMonth.dayNumber}))</script>");
            sb.Append("<div class='wbs_number {#if $T.dayOfMonth.cssToday != 'day' && $T.dayOfMonth.cssToday != ''}today{#/if}'>{$T.dayOfMonth.dayNumber} {#if $T.dayOfMonth.cssToday != 'day' && $T.dayOfMonth.cssToday != ''}(" + Resources.CalendarResource.Today + "){#/if}</div>{#else}<div class='wbs_number_empty'></div>{#/if}");
            sb.Append("</div>");
            sb.Append("{#if $T.dayOfMonth.index % 7 == 0}");
            sb.Append("</div>");
            sb.Append("{#/if}");
            sb.Append("{#/for}");
            sb.Append("</div>");

            sb.Append("</textarea>");

            sb.Append("</p>");

            return sb.ToString();
        }
        public string ListView()
        {
            StringBuilder sb = new StringBuilder();
            string url = HttpContext.Current.Request.Path;
            int prjID;
            if (!int.TryParse(HttpContext.Current.Request["prjID"], out prjID)) prjID = 0;
            string additionalParam = string.Empty;
            if (prjID != 0) additionalParam = string.Format("prjID={0}&", prjID);

            sb.Append(@"<script language=""javascript"">");
            sb.Append("window.onload = function()");
            sb.Append("{");
            sb.Append("Calendar.listView();");
            sb.Append("Calendar.InitViewSwitcher();");
            sb.Append("};");
            sb.Append(@"</script>");

            sb.Append("<div class='clearFix borderBase' id='filterBlock'>");
            sb.AppendFormat("<span class='viewSwitcherItem' id='vsItem_0'><a class='linkAction' href='{0}?{2}action=0'>{1}</a></span>", url, Resources.CalendarResource.Month, additionalParam);
            sb.AppendFormat("<span class='viewSwitcherItem' id='vsItem_2'><a class='linkAction' href='{0}?{2}action=2'>{1}</a></span>", url, Resources.CalendarResource.Week, additionalParam);
            sb.AppendFormat("<span class='viewSwitcherItem' id='vsItem_1'><a class='linkAction' href='{0}?{2}action=1'>{1}</a></span>", url, Resources.CalendarResource.EventsList, additionalParam);
            //iCal
            if (ViewICalFiles)
            {
                sb.Append("<input type='file' name='uploadICalFile'/><span class='splitter'></span>");
                sb.AppendFormat("<a onclick='javascript:__doPostBack(\"uploadICalFile\",\"\");' class='linkAction' href='javascript:void(0);'>{0}</a><span class='splitter'></span>", Resources.CalendarResource.UploadFile);
                sb.AppendFormat("<a onclick='javascript:__doPostBack(\"getICalFile\",\"\");' class='linkAction' href='javascript:void(0);'>{0}</a>", Resources.CalendarResource.ICal);
            }
            sb.Append("</div>");

            
            sb.AppendFormat("<div class='pm-headerPanel-splitter headerBase'>{0}</div><div id='ongoingEvents'></div>", Resources.CalendarResource.OngoingEvents);
            sb.AppendFormat("<br /><div class='pm-headerPanel-splitter headerBase'>{0}</div><div id='upcomingEvents'></div>", Resources.CalendarResource.UpcomingEvents);
            sb.AppendFormat("<br /><div class='pm-headerPanel-splitter headerBase'>{0}</div><div id='pastEvents'></div>", Resources.CalendarResource.PastEvents);

                  
            sb.Append("<p style='display:none'>");

            
            sb.Append("<textarea id='listViewTemlate' rows='0' cols='0'>");
            sb.Append("{#if $T.Events.length > 0}");
            sb.Append("<table class='pm-tablebase' cellpadding='15' cellspacing='0'>");
            sb.Append("<thead>");
            sb.Append("<tr>");
            sb.AppendFormat("<td class='borderBase'>{0}</td>", Resources.CalendarResource.Title);
            sb.AppendFormat("<td class='borderBase' width='150px'>{0}</td>", Resources.CalendarResource.StartingDate);
            sb.AppendFormat("<td class='borderBase' width='150px'>{0}</td>", Resources.CalendarResource.EndingDate);
            sb.Append("</tr>");
            sb.Append("</thead>");
            sb.Append("<tbody >");
            sb.Append("{#foreach $T.Events as event}");
            sb.Append("<tr class='{#cycle values=[\'tintMedium\',\'\']}'>");
            sb.Append("<td class='borderBase'><a class='linkMediumDark' href='{$T.event.EventURl}'>{$T.event.Title}</a></td>");
            sb.Append("<td class='borderBase'>{$T.event.StartingDate}</td>");
            sb.Append("<td class='borderBase'>{$T.event.EndingDate}</td>");
            sb.Append("</tr>");
            sb.Append("{#/for}");
            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("{#else}");
            sb.Append(Resources.CalendarResource.NoData);
            sb.Append("{#/if}");
            sb.Append("</textarea>");

            sb.Append("</p>");

            return sb.ToString();
        }
        public string WeekView()
        {
            StringBuilder sb = new StringBuilder();
            string url = HttpContext.Current.Request.Path;
            int prjID;
            if (!int.TryParse(HttpContext.Current.Request["prjID"], out prjID)) prjID = 0;
            string additionalParam = string.Empty;
            if (prjID != 0) additionalParam = string.Format("prjID={0}&", prjID);

            sb.Append(@"<script language=""javascript"">");
            sb.Append("window.onload = function()");
            sb.Append("{");
            sb.Append("Calendar.drawWeek();");
            sb.Append("Calendar.Init();");
            sb.Append("Calendar.initPrevWeekButton();");
            sb.Append("Calendar.initNextWeekButton();");
            sb.Append("Calendar.InitViewSwitcher();");
            sb.Append("};");
            sb.Append(@"</script>");

            
            sb.Append("<div class='clearFix borderBase' id='filterBlock'>");
            sb.AppendFormat("<span class='viewSwitcherItem' id='vsItem_0'><a class='linkAction' href='{0}?{2}action=0'>{1}</a></span>", url, Resources.CalendarResource.Month, additionalParam);
            sb.AppendFormat("<span class='viewSwitcherItem' id='vsItem_2'><a class='linkAction' href='{0}?{2}action=2'>{1}</a></span>", url, Resources.CalendarResource.Week, additionalParam);
            sb.AppendFormat("<span class='viewSwitcherItem' id='vsItem_1'><a class='linkAction' href='{0}?{2}action=1'>{1}</a></span>", url, Resources.CalendarResource.EventsList, additionalParam);
            
            //iCal
            if (ViewICalFiles)
            {
                sb.Append("<input type='file' name='uploadICalFile'/><span class='splitter'></span>");
                sb.AppendFormat("<a onclick='javascript:__doPostBack(\"uploadICalFile\",\"\");' class='linkAction' href='javascript:void(0);'>{0}</a><span class='splitter'></span>", Resources.CalendarResource.UploadFile);
                sb.AppendFormat("<a onclick='javascript:__doPostBack(\"getICalFile\",\"\");' class='linkAction' href='javascript:void(0);'>{0}</a>", Resources.CalendarResource.ICal);
            }
            sb.Append("</div>");

            
            sb.Append("<div class='clearFix'>");
            sb.Append("<div class='calendar-navigation-panel calendar-font-11'>");
            sb.AppendFormat("<a id='prevWeekButton' class='calendar-navigation-panel-left r' href='#' onclick='javascript: return false;'>{0}</a>", Resources.CalendarResource.PreviousWeek);
            sb.AppendFormat("<div class='calendar-navigation-panel-title'>{0}<span id='title'></span></div>", Resources.CalendarResource.Week);
            sb.AppendFormat("<a id='nextWeekButton' class='calendar-navigation-panel-right r' href='#' onclick='javascript: return false;'>{0}</a>", Resources.CalendarResource.NextWeek);
            sb.Append("</div>");
            sb.Append("<div id='jTemplatesContainer'>&nbsp;</div>");
            sb.Append("</div>");

            
            sb.Append("<div class='popupContainerClass' id='popup'>");
            sb.AppendFormat("<div class='containerHeaderBlock'><span id='createHeader'>{0}</span><span id='editHeader'>{1}</span></div>", Resources.CalendarResource.CreateNewEvent, Resources.CalendarResource.EditingEvents);
            sb.Append("<div class='containerBodyBlock'>");
            sb.Append("<div class='pm-headerPanelSmall-splitter'>");
            sb.AppendFormat("<div class='headerPanelSmall'>{0}</div>", Resources.CalendarResource.Title);
            sb.Append("<input class='textEdit' id='eventTitle' type='text' style='width:99%;' value='' />");
            sb.Append("</div>");
            sb.Append("<div class='pm-headerPanelSmall-splitter'>");
            sb.AppendFormat("<div class='headerPanelSmall'>{0}</div>", Resources.CalendarResource.StartingDate);
            sb.Append("<select id='eventStartingDateYear' class='comboBox' style='width:60px;'></select>");
            sb.Append("<select id='eventStartingDateMonth' class='comboBox' style='width:100px;'></select>");
            sb.Append("<select id='eventStartingDateDay' class='comboBox' style='width:45px;'></select>");
            sb.Append("<span class='button-splitter'></span>");
            sb.Append("<select id='eventStartingDateHour' class='comboBox' style='width:45px;'></select>");
            sb.Append("<select id='eventStartingDateMinute' class='comboBox' style='width:45px;'></select>");
            sb.Append("</div>");
            sb.Append("<div class='pm-headerPanelSmall-splitter'>");
            sb.AppendFormat("<div class='headerPanelSmall'>{0}</div>", Resources.CalendarResource.EndingDate);
            sb.Append("<select id='eventEndingDateYear' class='comboBox' style='width:60px;'></select>");
            sb.Append("<select id='eventEndingDateMonth' class='comboBox' style='width:100px;'></select>");
            sb.Append("<select id='eventEndingDateDay' class='comboBox' style='width:45px;'></select>");
            sb.Append("<span class='button-splitter'></span>");
            sb.Append("<select id='eventEndingDateHour' class='comboBox' style='width:45px;'></select>");
            sb.Append("<select id='eventEndingDateMinute' class='comboBox' style='width:45px;'></select>");
            sb.Append("</div>");
            if (ViewColorPicker)
            {
                sb.Append("<div class='pm-headerPanelSmall-splitter'>");
                sb.AppendFormat("<div class='headerPanelSmall'>{0}</div>", Resources.CalendarResource.Color);
                sb.Append("<input type='text' class='color textEdit' style='width:99%;' value='66ff00' id='eventColor'>");
                sb.Append("</div>");
            }
            sb.Append("<div class='pm-h-line' ><!– –></div>");
            sb.Append("<div style='text-align:left;'>");
            sb.AppendFormat("<span id='createEventButton'><a href='javascript:void(0)' onclick=\"createNewEvent()\" class='baseLinkButton'>{0}</a>", Resources.CalendarResource.CreateEvent);
            sb.Append("<span class='button-splitter'></span></span>");
            sb.AppendFormat("<span id='editEventButton'><a href='javascript:void(0)' onclick=\"editEvent()\" class='baseLinkButton'>{0}</a>", Resources.CalendarResource.EditEvent);
            sb.Append("<span class='button-splitter'></span></span>");
            sb.AppendFormat("<span id='deleteEventButton'><a href='javascript:void(0)' onclick=\"deleteEvent()\" class='baseLinkButton'>{0}</a>", Resources.CalendarResource.DeleteEvent);
            sb.Append("<span class='button-splitter'></span></span>");
            sb.AppendFormat("<a href='javascript:void(0)' onclick='Calendar.closePopup();' class='grayLinkButton'>{0}</a>", Resources.CalendarResource.Cancel);
            sb.Append("<input type='hidden' id='selectedEvent' value='-1'>");
            sb.Append("<input type='hidden' id='isEditEvent' value='-1'>");
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("</div>");

             
            sb.Append("<p style='display:none'>");
            
            sb.Append("<textarea id='calendarTemlate' rows='0' cols='0'>");

            sb.Append("<div class='calendar-week-days'>");
                sb.Append("<div class='' style='width:12.3%'>&nbsp;</div>");
                sb.Append("{#foreach $T.daysOfWeek_table as dayOfWeek}");
                sb.Append("<div style='{$T.hackWidth}'>{$T.dayOfWeek.title}</div>");
                sb.Append("{#/for}");
            sb.Append("</div>");

            sb.Append("<div class='wbs_month' id='week'>");
                sb.Append("<div class='wbs_week' style='margin-bottom: 5px;'>");
                sb.Append("<div style='width:12.3%;border-top:1px solid #CCCCCC;height:99%;' class='wbs_day' id='cell0'></div>");
                    sb.Append("{#foreach $T.daysOfWeek as dayOfWeek}");
                    sb.Append("<div style='{$T.hackWidth}' class='wbs_day {$T.dayOfWeek.cssToday}' id='cell{$T.dayOfWeek.index}' onclick='Calendar.clickOnWeekView(this,{$T.dayOfWeek.year},{$T.dayOfWeek.month},{$T.dayOfWeek.dayNumber});'>");
                    sb.Append("<script>Calendar.writeEventsOnWeekView(jq('#cell{$T.dayOfWeek.index}').parent(),new Date({$T.dayOfWeek.year},{$T.dayOfWeek.month},{$T.dayOfWeek.dayNumber}))</script>");
                    sb.Append("<div class='wbs_number {#if $T.dayOfWeek.cssToday != 'day' && $T.dayOfWeek.cssToday != ''}today{#/if}' style='text-align:center;'>{$T.dayOfWeek.monthName}, {$T.dayOfWeek.dayNumber}</div>");        
                    sb.Append("</div>");
                    sb.Append("{#/for}");
                sb.Append("</div>");
            sb.Append("</div>");

            
            sb.Append("<br/><table width='100%' class='f11' id='week_table' style='-moz-user-select: none; cursor: default;'>");
                sb.Append("{#for i = 0 to 23 step=1}");    
                sb.Append("<tr>");
                sb.Append("<td style='text-align:center;border-left:1px solid #CCCCCC;{#if $T.i == 0}border-top:1px solid #CCCCCC;{#/if}' width='12%'>{#if $T.i < 10}0{#/if}{$T.i}:00</td>");
				    sb.Append("{#foreach $T.daysOfWeek as dayOfWeek}");
                    sb.Append("<td style='{#if $T.i == 0}border-top:1px solid #CCCCCC;{#/if}' id='cell_{$T.i}_{$T.dayOfWeek.index}' class='{$T.dayOfWeek.cssToday}' onclick='Calendar.clickOnWeekView(this,{$T.dayOfWeek.year},{$T.dayOfWeek.month},{$T.dayOfWeek.dayNumber})'></td>");
                    sb.Append("<script>Calendar.writeEventsByHours(jq('#cell_{$T.i}_{$T.dayOfWeek.index}'),new Date({$T.dayOfWeek.year},{$T.dayOfWeek.month},{$T.dayOfWeek.dayNumber}))</script>");
                    sb.Append("{#/for}");
                sb.Append("</tr>");
                sb.Append("{#/for}");
            sb.Append("</table><br/>");
            
            
            /*sb.Append("{#for i = 0 to 23 step=1}");
            sb.Append("<div class='wbs_month' id='hour{$T.i}'>");
                sb.Append("<div class='wbs_week' style='height:40px;'>");
                    sb.Append("<div style='width:12.5%' class='wbs_day'>{$T.i}</div>");
                    sb.Append("{#foreach $T.daysOfWeek as dayOfWeek}");
                    sb.Append("<div style='width:12.5%;z-index:{169-(($T.i*7)+$T.dayOfWeek.index)};' class='wbs_day {$T.dayOfWeek.cssToday}' onclick='Calendar.clickOnWeekView(this,{$T.dayOfWeek.year},{$T.dayOfWeek.month},{$T.dayOfWeek.dayNumber})' id='cell_{$T.i}_{$T.dayOfWeek.index}'></div>");
                    sb.Append("{#/for}");
                sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("{#/for}");*/
            

            sb.Append("</textarea>");

            sb.Append("</p>");

            return sb.ToString();
        }
        //--------------------------------------------------------------------------

        #endregion

        #region Events

        protected override void OnInit(EventArgs e)
        {
            ltScript = new Literal();
            this.Controls.Add(ltScript);
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Page.IsPostBack)
            {
                if (Page.Request.Form.Get("__EVENTTARGET") == "getICalFile") GetFile();
              
                if (Page.Request.Form.Get("__EVENTTARGET") == "uploadICalFile")
                {
                    HttpFileCollection fileColl = Page.Request.Files;
                    if (fileColl.Count > 0)
                    {
                        var obj = Activator.CreateInstance(ClassType);

                        List<object> objects = new List<object>();
                        List<CalendarEvent> newEvents = GetEventsFromFile(fileColl.Get(0));
                        objects.Add((object)newEvents);

                        CalendarEvents.AddRange(newEvents);

                        ClassType.GetMethod("SaveEvents").Invoke(obj, objects.ToArray());
                    }
                }
            }

            RegisterClientScripts();

            GenerateICalFile();

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }
        
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            if (Action == 2)
                writer.Write(WeekView());
            else if (Action == 1)
                writer.Write(ListView());
            else writer.Write(CalendarView());
            
        }

        #endregion

    }


}
