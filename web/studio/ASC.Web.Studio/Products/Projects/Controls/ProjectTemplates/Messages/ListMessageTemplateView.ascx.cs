#region Import

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Projects.Core.Domain;
using ASC.Web.Controls;
using ASC.Web.Projects.Classes;
#endregion

namespace ASC.Web.Projects.Controls.ProjectTemplates.Messages
{
    public partial class ListMessageTemplateView : BaseUserControl
    {

        public List<TemplateMessage> Messages { get; set; }

        List<TemplateMessage> RenderedMessages;

        protected void Page_Load(object sender, EventArgs e)
        {
            DataRow row;
            DataTable table = MakeTable();

			List<TemplateMessage> finded = new List<TemplateMessage>();
            int start = (UrlParameters.PageNumber - 1) * Global.EntryCountOnPage;
            int finish = start + Global.EntryCountOnPage;
            if (finish > Messages.Count) finish = Messages.Count;
            for (int index = start; index < finish; index++)
                finded.Add(Messages[index]);

            RenderedMessages = finded;

            foreach (var message in finded)
            {
                row = table.NewRow();
                row = InitDataRow(row, message);
                table.Rows.Add(row);
            }
            rptContent.DataSource = table;
            rptContent.DataBind();

            if ((Messages.Count > 0) && (!IsPostBack))
            {
                string pageUrl = String.Concat(PathProvider.BaseAbsolutePath, "messages.aspx?", "prjID=", UrlParameters.ProjectID, "&ID=", UrlParameters.EntityID);

                phContent.Controls.Add(new PageNavigator
                {
                    CurrentPageNumber = UrlParameters.PageNumber,
                    EntryCountOnPage = Global.EntryCountOnPage,
                    VisiblePageCount = Global.VisiblePageCount,
                    EntryCount = Messages.Count,
                    //EntryCount = Global.EngineFactory.GetFileEngine().GetCurrentFiles(new OrderBy("create_on", SortDirection.Ascending)).Count,
                    VisibleOnePage = false,
                    ParamName = UrlConstant.PageNumber,
                    PageUrl = pageUrl
                });
            }
        }

        public string GetMessage(TemplateMessage message)
        {

            Page page = new Page();

			MessagePreviewTemplate oMessageView = (MessagePreviewTemplate)LoadControl(PathProvider.GetControlVirtualPath("MessagePreviewTemplate.ascx"));
            oMessageView.Message = message;
            oMessageView.IsPreview = false;

            page.Controls.Add(oMessageView);

            var writer = new System.IO.StringWriter();

            HttpContext.Current.Server.Execute(page, writer, false);

            string output = writer.ToString();

            writer.Close();

            return output;
        }

        public DataTable MakeTable()
        {
            DataTable Table = new DataTable();

            DataColumn MessageIDColumn = new DataColumn();
            MessageIDColumn.DataType = System.Type.GetType("System.Int32");
            MessageIDColumn.ColumnName = "MessageID";
            Table.Columns.Add(MessageIDColumn);

            DataColumn Message = new DataColumn();
            Message.DataType = typeof(TemplateMessage);
            Message.ColumnName = "Message";
            Table.Columns.Add(Message);

            return Table;
        }

        public DataRow InitDataRow(DataRow row, TemplateMessage message)
        {
            row["MessageID"] = message.Id;
            row["Message"] = message;

            return row;
        }

    }
}