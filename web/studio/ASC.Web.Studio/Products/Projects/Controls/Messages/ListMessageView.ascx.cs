#region Import

using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Projects.Core.Domain;
using ASC.Web.Controls;
using ASC.Web.Projects.Classes;
#endregion

namespace ASC.Web.Projects.Controls.Messages
{
    public partial class ListMessageView : BaseUserControl
    {

        public List<Message> Messages { get; set; }

        List<Message> RenderedMessages;
        List<int> CommentsCounts;
        List<List<ASC.Files.Core.File>> MessagesFiles;

        protected void Page_Load(object sender, EventArgs e)
        {
            DataRow row;
            DataTable table = MakeTable();

            List<Message> finded = new List<Message>();
            int start = (UrlParameters.PageNumber - 1) * Global.EntryCountOnPage;
            int finish = start + Global.EntryCountOnPage;
            if (finish > Messages.Count) finish = Messages.Count;
            for (int index = start; index < finish; index++)
                finded.Add(Messages[index]);

            RenderedMessages = finded;

            CommentsCounts = Global.EngineFactory.GetCommentEngine().GetCommentsCount(finded.ConvertAll(m => m as ProjectEntity));
            MessagesFiles = new List<List<ASC.Files.Core.File>>();
            foreach (var m in finded)
            {
                MessagesFiles.Add(FileEngine2.GetMessageFiles(m));
            }

            foreach (Message message in finded)
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

        public string GetMessage(Message message)
        {
            var page = new Page();

            var view = (MessageView)LoadControl(PathProvider.GetControlVirtualPath("MessageView.ascx"));
            view.Message = message;
            view.CommentsCount = CommentsCounts[RenderedMessages.IndexOf(message)];
            view.Files = MessagesFiles[RenderedMessages.IndexOf(message)];
            view.IsPreview = false;
            page.Controls.Add(view);

            using (var writer = new System.IO.StringWriter())
            {
                HttpContext.Current.Server.Execute(page, writer, false);
                return writer.ToString();
            }
        }

        public DataTable MakeTable()
        {
            var Table = new DataTable();
            var MessageIDColumn = new DataColumn();
            MessageIDColumn.DataType = System.Type.GetType("System.Int32");
            MessageIDColumn.ColumnName = "MessageID";
            Table.Columns.Add(MessageIDColumn);

            var Message = new DataColumn();
            Message.DataType = typeof(Message);
            Message.ColumnName = "Message";
            Table.Columns.Add(Message);

            return Table;
        }

        public DataRow InitDataRow(DataRow row, Message message)
        {
            row["MessageID"] = message.ID;
            row["Message"] = message;
            return row;
        }
    }
}