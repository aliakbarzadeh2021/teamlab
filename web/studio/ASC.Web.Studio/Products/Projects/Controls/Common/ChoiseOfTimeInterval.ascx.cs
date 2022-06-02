#region Import

using System;
using System.Data;
using System.Web.UI.WebControls;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Classes;
using System.Text;
using System.Collections.Generic;
using ASC.Web.Projects.Configuration;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Resources;
using System.Web;


#endregion

namespace ASC.Web.Projects.Controls.Common
{
    public partial class ChoiseOfTimeInterval : BaseUserControl
    {
        #region Property

        public static String FromDateTiks
        {
            get
            {
                string result = HttpContext.Current.Request["fromDate"];
                return result == null ? "" : result;
            }
        }
        public static String ToDateTiks
        {
            get
            {
                string result = HttpContext.Current.Request["toDate"];
                return result == null ? "" : result;
            }
        }
        public static String TimeRange
        {
            get
            {
                string result = HttpContext.Current.Request["timeRange"];
                return result == null ? "" : result;
            }
        }
        public static String Module
        {
            get
            {
                string result = HttpContext.Current.Request["module"];
                return result == null ? "" : result;
            }
        }

        public TextBox FromDate
        {
            get
            {
                return tbxFromDate;
            }
        }
        public TextBox ToDate
        {
            get
            {
                return tbxToDate;
            }
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            InitControls();
        }

        #endregion

        #region Methods

        public void InitControls()
        {
            InitDateTbx();

            if (TimeRange != "")
                hiddenCurrentTimeRange.Value = TimeRange;
           else hiddenCurrentTimeRange.Value = "3";

        }
        public int isPostBack()
        {
            if (IsPostBack) return 1;
            else return 0;
        }
        public void InitDateTbx()
        {
            if (FromDateTiks != "")
            {
                tbxFromDate.Text = new DateTime(Convert.ToInt64(FromDateTiks)).ToString(DateTimeExtension.DateFormatPattern);
            }
            else
            {
                tbxFromDate.Text = ASC.Core.Tenants.TenantUtil.DateTimeNow().AddDays(1 - (int)ASC.Core.Tenants.TenantUtil.DateTimeNow().DayOfWeek-7).ToString(DateTimeExtension.DateFormatPattern);
            }
            if (ToDateTiks != "")
            {
                tbxToDate.Text = new DateTime(Convert.ToInt64(ToDateTiks)).ToString(DateTimeExtension.DateFormatPattern);
            }
            else
            {
                tbxToDate.Text = ASC.Core.Tenants.TenantUtil.DateTimeNow().AddDays(7 - (int)ASC.Core.Tenants.TenantUtil.DateTimeNow().DayOfWeek-7).ToString(DateTimeExtension.DateFormatPattern);
            }
        }

        #endregion
    }
}