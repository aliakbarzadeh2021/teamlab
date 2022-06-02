using System;
using System.Web.UI;

namespace ASC.Web.Files.Controls
{
    public partial class StatisticWidget : UserControl
    {
        public static string Location { get { return Files.Classes.PathProvider.GetFileStaticRelativePath("StatisticWidget/StatisticWidget.ascx"); } }
        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}