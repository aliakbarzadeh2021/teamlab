using System;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using ASC.Files.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Controls;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Files.Import;

namespace ASC.Web.Files
{
    public partial class _Default : BasePage
    {
        protected override void PageLoad()
        {

            var sideNavigator = new SideNavigator();
            NavigationWidget navigationWidget = (NavigationWidget)LoadControl(NavigationWidget.Location);
            navigationWidget.FolderIDUserRoot = Global.FolderMy;
            navigationWidget.FolderIDCommonRoot = Global.FolderCommon;
            navigationWidget.FolderIDShare = Global.FolderShare;
            navigationWidget.FolderIDTrash = Global.FolderTrash;
            sideNavigator.Controls.Add(navigationWidget);
            SidePanelHolder.Controls.Add(sideNavigator);

            if (ASC.Core.SecurityContext.IsAuthenticated)
            {
                var favorites = new SideContainer();
                favorites.ImageURL = PathProvider.GetImagePath("favorites.png");
                favorites.Title = FilesCommonResource.Favorites;
                FavoriteWidget fw = (FavoriteWidget)LoadControl(FavoriteWidget.Location);
                favorites.Controls.Add(fw);
                SidePanelHolder.Controls.Add(favorites);

                if (ImportConfiguration.SupportGoogleImport || ImportConfiguration.SupportBoxNetImport || ImportConfiguration.SupportZohoImport)
                {
                    var importWidget = new SideContainer();
                    importWidget.ImageURL = PathProvider.GetImagePath("tools_container.png");
                    importWidget.Title = FilesCommonResource.Tools;
                    importWidget.Controls.Add(LoadControl(ImportWidget.Location));
                    SidePanelHolder.Controls.Add(importWidget);
                }
            }

            var statisticWidget = new SideContainer();
            statisticWidget.ImageURL = PathProvider.GetImagePath("statistics.png");
            statisticWidget.Title = FilesCommonResource.Statistics;
            statisticWidget.Controls.Add(LoadControl(StatisticWidget.Location));
            SidePanelHolder.Controls.Add(statisticWidget);

            if (string.IsNullOrEmpty(WebConfigurationManager.AppSettings["files.tips"]) || bool.TrueString.Equals(WebConfigurationManager.AppSettings["files.tips"], StringComparison.InvariantCultureIgnoreCase))
            {
                var tipsContainer = new SideContainer { Title = FilesCommonResource.TipsAndTricks };
                tipsContainer.ImageURL = PathProvider.GetImagePath("tips.png");
                tipsContainer.Controls.Add(new System.Web.UI.HtmlControls.HtmlGenericControl() { InnerHtml = "<div id='tips_side_container' class='tips_side_container'></div>" });
                SidePanelHolder.Controls.Add(tipsContainer);
            }

            CommonContainerHolder.Controls.Add(LoadControl(MainMenu.Location));

            MainContent mainContent = (MainContent)LoadControl(MainContent.Location);
            mainContent.FolderIDUserRoot = Global.FolderMy;
            mainContent.FolderIDCommonRoot = Global.FolderCommon;
            mainContent.FolderIDShare = Global.FolderShare;
            mainContent.FolderIDTrash = Global.FolderTrash;
            mainContent.TitlePage = FilesCommonResource.TitlePage;
            CommonContainerHolder.Controls.Add(mainContent);

            if (Global.EnableShare && ASC.Core.SecurityContext.IsAuthenticated)
            {
                CommonContainerHolder.Controls.Add(LoadControl(AccessRights.Location));
            }

            Master.CommonContainerHeaderVisible = false;
            Master.LeftSidePanel = true;
        }
    }
}