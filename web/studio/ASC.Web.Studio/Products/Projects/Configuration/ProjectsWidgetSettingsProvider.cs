using System;
using System.Collections.Generic;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Projects.Controls.Dashboard;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Dashboard.Settings;

namespace ASC.Web.Projects.Configuration
{
    public class ProjectsWidgetSettingsProvider : IWidgetSettingsProvider
    {
        public bool Check(List<WidgetSettings> settings, Guid widgetID, Guid userID, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (widgetID.Equals(MessagesWidget.WidgetID))
            {
                if (settings == null || settings.Count != 2) return false;

                var data1 = settings[0].ConvertToNumber();
                if (data1.Value > 0 && data1.Value <= 30) return true;

                errorMessage = ProjectsCommonResource.MessagesWidget_ErrorMessage;
                return false;
            }

            if (widgetID.Equals(RecentActivityWidget.WidgetID))
            {
                if (settings == null || settings.Count != 2) return false;

                var data = settings[0].ConvertToNumber();
                if (data.Value > 0 && data.Value <= 30) return true;

                errorMessage = ProjectsCommonResource.RecentActivityWidget_ErrorMessage;
                return false;
            }

            if (widgetID.Equals(MyNewTasksWidget.WidgetID))
            {
                if (settings == null || settings.Count != 1) return false;

                var data = settings[0].ConvertToNumber();
                if (data.Value > 0 && data.Value <= 30) return true;

                errorMessage = ProjectsCommonResource.MyTasksWidget_ErrorMessage;
                return false;
            }

            if (widgetID.Equals(UpcomingMilestonesWidget.WidgetID))
            {
                if (settings == null || settings.Count != 2) return false;

                var data = settings[0].ConvertToNumber();
                if (data.Value > 0 && data.Value <= 30) return true;

                errorMessage = ProjectsCommonResource.MilestonesWidget_ErrorMessage;
                return false;
            }

            if (widgetID.Equals(LateMilestonesWidget.WidgetID))
            {
                if (settings == null || settings.Count != 2) return false;

                var data = settings[0].ConvertToNumber();
                if (data.Value > 0 && data.Value <= 30) return true;

                errorMessage = ProjectsCommonResource.MilestonesWidget_ErrorMessage;
                return false;
            }

            if (widgetID.Equals(MyProjectsWidget.WidgetID))
            {
                if (settings == null || settings.Count != 1) return false;

                var data = settings[0].ConvertToNumber();
                if (data.Value > 0 && data.Value <= 30) return true;

                errorMessage = ProjectsCommonResource.MyProjectsWidget_ErrorMessage;
                return false;
            }

            if (widgetID.Equals(NewProjectsWidget.WidgetID))
            {
                if (settings == null || settings.Count != 1) return false;

                var data = settings[0].ConvertToNumber();
                if (data.Value > 0 && data.Value <= 30) return true;

                errorMessage = ProjectsCommonResource.NewProjectsWidget_ErrorMessage;
                return false;
            }

            if (widgetID.Equals(FollowingProjectsWidget.WidgetID))
            {
                if (settings == null || settings.Count != 1) return false;

                var data = settings[0].ConvertToNumber();
                if (data.Value > 0 && data.Value <= 30) return true;

                errorMessage = ProjectsCommonResource.FollowingProjectsWidget_ErrorMessage;
                return false;
            }

            return false;
        }

        public void Save(List<WidgetSettings> settings, Guid widgetID, Guid userID)
        {
            if (widgetID.Equals(MessagesWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<MessagesWidgetSettings>(userID);
                var data1 = settings[0].ConvertToNumber();
                var data2 = settings[1].ConvertToBool();

                widgetSettings.MesagesCount = data1.Value;
                widgetSettings.ShowOnlyItemsInFollowingProjects = data2.Value;

                SettingsManager.Instance.SaveSettingsFor(widgetSettings, userID);
            }

            if (widgetID.Equals(RecentActivityWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<RecentActivityWidgetSettings>(userID);
                var data1 = settings[0].ConvertToNumber();
                var data2 = settings[1].ConvertToBool();

                widgetSettings.ActivityCount = data1.Value;
                widgetSettings.ShowOnlyItemsInFollowingProjects = data2.Value;

                SettingsManager.Instance.SaveSettingsFor(widgetSettings, userID);
            }

            if (widgetID.Equals(MyNewTasksWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<MyNewTasksWidgetSettings>(userID);
                var data = settings[0].ConvertToNumber();
                widgetSettings.TasksCount = data.Value;
                SettingsManager.Instance.SaveSettingsFor(widgetSettings, userID);
            }

            if (widgetID.Equals(UpcomingMilestonesWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<UpcomingMilestonesWidgetSettings>(userID);
                var data1 = settings[0].ConvertToNumber();
                var data2 = settings[1].ConvertToBool();

                widgetSettings.MilestonesCount = data1.Value;
                widgetSettings.ShowOnlyItemsInFollowingProjects = data2.Value;

                SettingsManager.Instance.SaveSettingsFor(widgetSettings, userID);
            }

            if (widgetID.Equals(LateMilestonesWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<LateMilestonesWidgetSettings>(userID);
                var data1 = settings[0].ConvertToNumber();
                var data2 = settings[1].ConvertToBool();

                widgetSettings.MilestonesCount = data1.Value;
                widgetSettings.ShowOnlyItemsInFollowingProjects = data2.Value;

                SettingsManager.Instance.SaveSettingsFor(widgetSettings, userID);
            }

            if (widgetID.Equals(MyProjectsWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<MyProjectsWidgetSettings>(userID);
                var data = settings[0].ConvertToNumber();
                widgetSettings.ProjectsCount = data.Value;
                SettingsManager.Instance.SaveSettingsFor(widgetSettings, userID);
            }

            if (widgetID.Equals(NewProjectsWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<NewProjectsWidgetSettings>(userID);
                var data = settings[0].ConvertToNumber();
                widgetSettings.ProjectsCount = data.Value;
                SettingsManager.Instance.SaveSettingsFor(widgetSettings, userID);
            }

            if (widgetID.Equals(FollowingProjectsWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<FollowingProjectsWidgetSettings>(userID);
                var data = settings[0].ConvertToNumber();
                widgetSettings.ProjectsCount = data.Value;
                SettingsManager.Instance.SaveSettingsFor(widgetSettings, userID);
            }
        }

        public List<WidgetSettings> Load(Guid widgetID, Guid userID)
        {
            var settings = new List<WidgetSettings>();

            if (widgetID.Equals(MessagesWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<MessagesWidgetSettings>(userID);

                settings.Add(new NumberWidgetSettings()
                {
                    Title = ProjectsCommonResource.MessagesWidget_Title,
                    Value = widgetSettings.MesagesCount,
                    Description = ""
                });

                settings.Add(new BoolWidgetSettings()
                {
                    Title = ProjectsCommonResource.ShowOnlyForMyProjects,
                    Value = widgetSettings.ShowOnlyItemsInFollowingProjects,
                    Description = ""
                });
            }

            if (widgetID.Equals(RecentActivityWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<RecentActivityWidgetSettings>(userID);

                settings.Add(new NumberWidgetSettings()
                {
                    Title = ProjectsCommonResource.RecentActivityWidget_Title,
                    Value = widgetSettings.ActivityCount,
                    Description = ""
                });

                settings.Add(new BoolWidgetSettings()
                {
                    Title = ProjectsCommonResource.ShowOnlyForMyProjects,
                    Value = widgetSettings.ShowOnlyItemsInFollowingProjects,
                    Description = ""
                });
            }

            if (widgetID.Equals(MyNewTasksWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<MyNewTasksWidgetSettings>(userID);
                settings.Add(new NumberWidgetSettings()
                {
                    Title = ProjectsCommonResource.MyTasksWidget_Title,
                    Value = widgetSettings.TasksCount,
                    Description = ""
                });
            }

            if (widgetID.Equals(UpcomingMilestonesWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<UpcomingMilestonesWidgetSettings>(userID);

                settings.Add(new NumberWidgetSettings()
                {
                    Title = ProjectsCommonResource.MilestonesWidget_CountMilestones_Title,
                    Value = widgetSettings.MilestonesCount,
                    Description = ""
                });

                settings.Add(new BoolWidgetSettings()
                {
                    Title = ProjectsCommonResource.ShowOnlyForMyProjects,
                    Value = widgetSettings.ShowOnlyItemsInFollowingProjects,
                    Description = ""
                });
            }

            if (widgetID.Equals(LateMilestonesWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<LateMilestonesWidgetSettings>(userID);

                settings.Add(new NumberWidgetSettings()
                {
                    Title = ProjectsCommonResource.MilestonesWidget_CountMilestones_Title,
                    Value = widgetSettings.MilestonesCount,
                    Description = ""
                });

                settings.Add(new BoolWidgetSettings()
                {
                    Title = ProjectsCommonResource.ShowOnlyForMyProjects,
                    Value = widgetSettings.ShowOnlyItemsInFollowingProjects,
                    Description = ""
                });
            }

            if (widgetID.Equals(MyProjectsWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<MyProjectsWidgetSettings>(userID);
                settings.Add(new NumberWidgetSettings()
                {
                    Title = ProjectsCommonResource.MyProjectsWidget_Title,
                    Value = widgetSettings.ProjectsCount,
                    Description = ""
                });
            }

            if (widgetID.Equals(NewProjectsWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<NewProjectsWidgetSettings>(userID);
                settings.Add(new NumberWidgetSettings()
                {
                    Title = ProjectsCommonResource.NewProjectsWidget_Title,
                    Value = widgetSettings.ProjectsCount,
                    Description = ""
                });
            }

            if (widgetID.Equals(FollowingProjectsWidget.WidgetID))
            {
                var widgetSettings = SettingsManager.Instance.LoadSettingsFor<FollowingProjectsWidgetSettings>(userID);
                settings.Add(new NumberWidgetSettings()
                {
                    Title = ProjectsCommonResource.FollowingProjectsWidget_Title,
                    Value = widgetSettings.ProjectsCount,
                    Description = ""
                });
            }

            return settings;
        }
    }
}
