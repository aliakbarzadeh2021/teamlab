using ASC.Core;
using ASC.Projects.Engine;
using ASC.Web.Files.Controls;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects
{
    public partial class TMDocs : BasePage
    {
        protected override void PageLoad()
        {
            var projectID = 0;
            int.TryParse(UrlParameters.ProjectID, out projectID);
            var project = Global.EngineFactory.GetProjectEngine().GetByID(projectID);

            if (project == null)
            {
                Response.Redirect(ProjectsCommonResource.StartURL.ToLower());
            }
            if (!ProjectSecurity.CanReadFiles(project))
            {
                Response.Redirect(ProjectsCommonResource.StartURL.ToLower());
            }

            var mainMenu = (MainMenu)LoadControl(MainMenu.Location);
            mainMenu.EnableShare = false;
            CommonContainerHolder.Controls.Add(mainMenu);

            var mainContent = (MainContent)LoadControl(MainContent.Location);
            mainContent.FolderIDCurrentRoot = FileEngine2.GetRoot(projectID);
            mainContent.TitlePage = ProjectsCommonResource.ModuleName;
            mainContent.CurrentUserAdmin = project.Responsible == SecurityContext.CurrentAccount.ID;
            CommonContainerHolder.Controls.Add(mainContent);

            Title = HeaderStringHelper.GetPageTitle(ProjectsFileResource.Files, Master.BreadCrumbs);
            SideNavigatorPanel.Visible = false;
        }
    }
}