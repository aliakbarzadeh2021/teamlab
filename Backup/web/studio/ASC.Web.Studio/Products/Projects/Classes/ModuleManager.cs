using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Web.Configuration;
using ASC.Projects.Core.Domain;
using ASC.Web.Core;
using ASC.Web.Projects.Resources;

namespace ASC.Web.Projects.Classes
{
    public class ModuleManager
    {
        private Dictionary<string, NavigationWebItem> sysNames;


        public List<IModule> Modules
        {
            get;
            private set;
        }


        public ModuleManager()
        {
            var modules = new List<NavigationWebItem>()
                { 
                    new NavigationWebItemNameFromResource(ProjectResource.ResourceManager, "ProjectOverview")
                    {
                        _ModuleID = new Guid("{52F1574D-8656-4bf1-B494-F5CBE64EF327}"),
                        _StartURL = String.Concat(PathProvider.BaseAbsolutePath, "projects.aspx?prjID={0}"),
                        ModuleSysName = "Projects",
                        DisplayedAlways = true,
                        _Context = new ModuleContext() { DefaultSortOrder = 0 }
                    },

                    new NavigationWebItemNameFromResource(MilestoneResource.ResourceManager, "Milestones")
                    {
                        _ModuleID = new Guid("{AF4AFD50-5553-47f3-8F91-651057BC930B}"),
                        _StartURL = String.Concat(PathProvider.BaseAbsolutePath, "milestones.aspx?prjID={0}"),
                        ModuleSysName = "Milestones",
                        DisplayedAlways = true,
                        _Context = new ModuleContext() { DefaultSortOrder = 1 }
                    },

                    new NavigationWebItemNameFromResource(TaskResource.ResourceManager, "Tasks")
                    {
                        _ModuleID = new Guid("{04339423-70E6-4b81-A2DF-3C31C723BD90}"),
                        _StartURL = String.Concat(PathProvider.BaseAbsolutePath, "tasks.aspx?prjID={0}"),
                        ModuleSysName = "Tasks",
                        DisplayedAlways = true,
                        _Context = new ModuleContext() { DefaultSortOrder = 2 }
                    },

                    new NavigationWebItemNameFromResource(MessageResource.ResourceManager, "Messages")
                    {
                        _ModuleID = new Guid("{9FF0FADE-6CFA-44ee-901F-6185593E4594}"),
                        _StartURL = String.Concat(PathProvider.BaseAbsolutePath, "messages.aspx?prjID={0}"),
                        ModuleSysName = "Messages",
                        DisplayedAlways = true,
                        _Context = new ModuleContext() { DefaultSortOrder = 3 }
                    },


                    new NavigationWebItemNameFromResource(IssueTrackerResource.ResourceManager, "Issues")
                    {
                        _ModuleID = new Guid("{517F98F4-99C4-437f-8FD9-B9E2DDBEADF4}"),
                        _StartURL = String.Concat(PathProvider.BaseAbsolutePath, "issueTracker.aspx?prjID={0}"),
                        ModuleSysName = "IssueTracker",
                        _Context = new ModuleContext() { DefaultSortOrder = 4 }
                    },

                    new NavigationWebItemNameFromResource(ProjectsCommonResource.ResourceManager, "TimeTracking")
                    {
                        _ModuleID = new Guid("{57E87DA0-D59B-443d-99D1-D9ABCAB31084}"),
                        _StartURL = String.Concat(PathProvider.BaseAbsolutePath, "timetracking.aspx?prjID={0}"),
                        ModuleSysName = "TimeTracking",
                        _Context = new ModuleContext() { DefaultSortOrder = 7 }
                    },
                     new NavigationWebItemNameFromResource(ProjectResource.ResourceManager, "Settings")
                    {
                        _ModuleID = new Guid("{977CBEEB-B166-4a80-8BE2-0133CCF2276F}"),
                        _StartURL = String.Concat(PathProvider.BaseAbsolutePath, "projectSettings.aspx?prjID={0}"),
                        ModuleSysName = "ProjectSettings",
                        DisplayedAlways = true,
                        _Context = new ModuleContext() { DefaultSortOrder = 8, SortDisabled = true }
                    },
                    new NavigationWebItemNameFromResource(ProjectsCommonResource.ResourceManager, "History")
                    {
                        _ModuleID = new Guid("{85E7CE26-46F6-4c3c-B25D-4BDE833C9742}"),
                        _StartURL = String.Concat(PathProvider.BaseAbsolutePath, "history.aspx?prjID={0}"),
                        ModuleSysName = "History",
                        DisplayedAlways = true,
                        _Context = new ModuleContext() { DefaultSortOrder = 9, SortDisabled = true }
                    },
                     new NavigationWebItemNameFromResource(ProjectResource.ResourceManager, "ProjectTeam")
                    {
                        _ModuleID = new Guid("{C42F993E-5D22-497e-AC26-1E9592515898}"),
                        _StartURL = String.Concat(PathProvider.BaseAbsolutePath, "projectTeam.aspx?prjID={0}"),
                        ModuleSysName = "ProjectTeam",
                        DisplayedAlways = true,
                        _Context = new ModuleContext() { DefaultSortOrder = 10 , SortDisabled = true}

                    },
                    new NavigationWebItemNameFromResource(ProjectsFileResource.ResourceManager, "Documents")
                    {
                        _ModuleID = new Guid("{81402440-557D-401d-9EE1-D570748F426D}"),
                        _StartURL = String.Concat(PathProvider.BaseAbsolutePath, "tmdocs".ToLower() + ".aspx?prjID={0}"),
                        ModuleSysName = "TMDocs",
                        _Context = new ModuleContext { DefaultSortOrder = 6 }
                    }
                };

            sysNames = new Dictionary<string, NavigationWebItem>(modules.Count);
            Modules = new List<IModule>();

            modules.ForEach(m =>
            {
                if (WebItemManager.Instance.RegistryItem(m))
                {
                    Modules.Add(m);
                    sysNames.Add(m.ModuleSysName, m);
                }
            });
        }

        public bool IsVisible(ModuleType type)
        {
            NavigationWebItem wi = null;
            sysNames.TryGetValue(type.ToString(), out wi);
            return wi != null ? !wi.IsDisabled() : false;
        }

        public IList<IWebItem> GetSortedVisibleItems()
        {
            return WebItemManager.Instance.GetItems(ASC.Web.Core.WebZones.WebZoneType.All)
                .OfType<NavigationWebItemNameFromResource>()
                .Cast<IWebItem>()
                .ToList();
        }

        [DebuggerDisplay("Name = {ModuleName}, SystemName = {ModuleSysName}")]
        private class NavigationWebItemNameFromResource : NavigationWebItem
        {
            private ResourceManager resourceManager;
            private string resourceName;

            public override string ModuleName
            {
                get { return resourceManager.GetString(resourceName); }
            }

            public NavigationWebItemNameFromResource(ResourceManager resourceManager, string resourceName)
                : base(false)
            {
                this.resourceManager = resourceManager;
                this.resourceName = resourceName;
            }
        }
    }
}
