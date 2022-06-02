using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Web.Files.Api;
using log4net;

namespace ASC.Projects.Engine
{
    [DebuggerDisplay("SearchItem: EntityType = {EntityType}, ID = {ID}, Title = {Title}")]
    public class SearchItem
    {
        public EntityType EntityType { get; set; }
        public String ID { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public DateTime CreateOn { get; set; }
    }

    [DebuggerDisplay("SearchGroup: ID = {ProjectID}, Title = {ProjectTitle}")]
    public class SearchGroup
    {
        public string ProjectTitle
        {
            get;
            private set;
        }

        public int ProjectID
        {
            get;
            private set;
        }

        public List<SearchItem> Items
        {
            get;
            private set;
        }

        public SearchGroup(int id, string title)
        {
            ProjectID = id;
            ProjectTitle = title;
            Items = new List<SearchItem>();
        }
    }

    public class SearchEngine
    {
        private readonly ISearchDao searchDAO;
        private readonly IProjectDao projDao;
        private readonly IDaoFactory daoFactory;


        public SearchEngine(IDaoFactory daoFactory)
        {
            searchDAO = daoFactory.GetSearchDao();
            projDao = daoFactory.GetProjectDao();
            this.daoFactory = daoFactory;
        }

        public List<SearchGroup> Search(String searchText, int projectId)
        {
            var queryResult = searchDAO.Search(searchText, projectId);

            var groups = new Dictionary<int, SearchGroup>();
            foreach (object[] r in queryResult)
            {
                var id = Convert.ToInt32(r[3]);
                if (!groups.ContainsKey(id))
                {
                    groups[id] = new SearchGroup(id, (string)r[4]);
                }
                var item = new SearchItem
                {
                    EntityType = (EntityType)Convert.ToInt32(r[0]),
                    ID = Convert.ToString(r[1]),
                    Title = (string)r[2],
                    Description = (string)r[5],
                    CreateOn = r[6] != null ? (DateTime)r[6] : DateTime.MinValue,
                };
                groups[id].Items.Add(item);
            }

            try
            {
                // search in files
                var fileEntries = new List<ASC.Files.Core.FileEntry>();
                using (var folderdao = FilesIntegration.GetFolderDao())
                using (var filedao = FilesIntegration.GetFileDao())
                {
                    fileEntries.AddRange(folderdao.Search(searchText, ASC.Files.Core.FolderType.BUNCH).Cast<ASC.Files.Core.FileEntry>());
                    fileEntries.AddRange(filedao.Search(searchText, ASC.Files.Core.FolderType.BUNCH).Keys.Cast<ASC.Files.Core.FileEntry>());

                    var projectIds = projectId != 0 ?
                        new List<int>() { projectId } :
                        fileEntries.GroupBy(f => f.RootFolderId)
                            .Select(g => folderdao.GetFolder(g.Key))
                            .Select(f => f != null && !string.IsNullOrEmpty(f.Title) ? f.Title.Split('/').LastOrDefault() : null)
                            .Where(s => !string.IsNullOrEmpty(s))
                            .Select(s => int.Parse(s));

                    var rootProject = projectIds.ToDictionary(id => FilesIntegration.RegisterBunch("projects", "project", id.ToString()));
                    fileEntries.RemoveAll(f => !rootProject.ContainsKey(f.RootFolderId));

                    var security = FilesIntegration.GetFileSecurity();
                    fileEntries.RemoveAll(f => !security.CanRead(f));

                    foreach (var f in fileEntries)
                    {
                        var id = rootProject[f.RootFolderId];
                        if (!groups.ContainsKey(projectId))
                        {
                            var project = projDao.GetById(id);
                            if (project != null)
                            {
                                groups[id] = new SearchGroup(id, project.Title);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        var item = new SearchItem
                        {
                            EntityType = EntityType.File,
                            ID = f is ASC.Files.Core.File ? ((ASC.Files.Core.File)f).ViewUrl : string.Format("{0}tmdocs.aspx?prjID={1}#{2}", VirtualPathUtility.ToAbsolute("~/products/projects/"), id, f.ID),
                            Title = f.Title,
                            CreateOn = f.CreateOn,
                        };
                        groups[id].Items.Add(item);
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.GetLogger("ASC.Web").Error(err);
            }
            return new List<SearchGroup>(groups.Values);
        }
    }
}