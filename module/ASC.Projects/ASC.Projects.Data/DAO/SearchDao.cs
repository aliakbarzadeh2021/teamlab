using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.FullTextIndex;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Data.DAO
{
    class SearchDao : BaseDao, ISearchDao
    {
        public SearchDao(string dbId, int tenant)
            : base(dbId, tenant)
        {
        }


        /// <summary>
        ///  entity_type, 
        ///  id,
        ///  title, 
        ///  project_id, 
        ///  project_title,
        ///  description
        ///  create_on
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="projectID"></param>
        /// <returns></returns>
        public IList Search(String text, int projectId)
        {
            SqlQuery query = null;

            if (FullTextSearch.SupportModule(FullTextSearch.ProjectsModule))
            {
                query = GetFullTextIndexQuery(text, projectId);
            }
            else
            {
                var keywords = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(k => 3 <= k.Trim().Length)
                    .ToArray();
                if (keywords.Length == 0) return new ArrayList();

                query = GetSearchQuery(keywords, projectId);
            }

            return DbManager
                .ExecuteList(query)
                .ConvertAll(r => new object[] { Convert.ToInt32(r[0]), Convert.ToString(r[1]), (string)r[2], Convert.ToInt32(r[3]), (string)r[4], (string)r[5], TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[6])) })
                .OrderBy(r => r[4])
                .OrderByDescending(r => r[6])
                .ToList();
        }

        private SqlQuery GetSearchQuery(string[] keywords, int projectId)
        {
            var projects = Query("projects_projects")
                .Select(EntityType.Project.ToString("d"), "cast(id as char)", "title", "id", "title", "description", "create_on")
                .Where(BuildLike(new[] { "title", "description" }, keywords));
            if (0 < projectId) projects.Where("id", projectId);

            var milestones = new SqlQuery("projects_messages t")
                .LeftOuterJoin("projects_projects p", Exp.EqColumns("t.project_id", "p.id"))
                .Select(EntityType.Message.ToString("d"), "cast(t.id as char)", "t.title", "p.id", "p.title", "t.content", "t.create_on")
                .Where("p.tenant_id", Tenant)
                .Where("t.tenant_id", Tenant)
                .Where(BuildLike(new[] { "t.title", "t.content" }, keywords));
            if (0 < projectId) milestones.Where("p.id", projectId);

            var tasks = new SqlQuery("projects_tasks t")
                .LeftOuterJoin("projects_projects p", Exp.EqColumns("t.project_id", "p.id"))
                .Select(EntityType.Task.ToString("d"), "cast(t.id as char)", "t.title", "p.id", "p.title", "t.description", "t.create_on")
                .Where("p.tenant_id", Tenant)
                .Where("t.tenant_id", Tenant)
                .Where(BuildLike(new[] { "t.title", "t.description" }, keywords));
            if (0 < projectId) tasks.Where("p.id", projectId);

            var messages = new SqlQuery("projects_milestones t")
                .LeftOuterJoin("projects_projects p", Exp.EqColumns("t.project_id", "p.id"))
                .Select(EntityType.Milestone.ToString("d"), "cast(t.id as char)", "t.title", "p.id", "p.title", "''", "t.create_on")
                .Where("p.tenant_id", Tenant)
                .Where("t.tenant_id", Tenant)
                .Where(BuildLike(new[] { "t.title" }, keywords));
            if (0 < projectId) messages.Where("p.id", projectId);


            return projects
                .Union(milestones)
                .Union(messages)
                .Union(tasks);
        }

        private Exp BuildLike(string[] columns, string[] keywords)
        {
            return BuildLike(columns, keywords, true);
        }

        private Exp BuildLike(string[] columns, string[] keywords, bool startWith)
        {
            var like = Exp.Empty;
            foreach (var keyword in keywords)
            {
                var keywordLike = Exp.Empty;
                foreach (var column in columns)
                {
                    keywordLike = keywordLike | Exp.Like(column, keyword, startWith ? SqlLike.StartWith : SqlLike.EndWith) | Exp.Like(column, ' ' + keyword);
                }
                like = like & keywordLike;
            }
            return like;
        }


        private SqlQuery GetFullTextIndexQuery(string text, int projectId)
        {
            var result = FullTextSearch.Search(text, FullTextSearch.ProjectsModule);

            var projects = Query("projects_projects")
                .Select(EntityType.Project.ToString("d"), "cast(id as char)", "title", "id", "title", "description", "create_on")
                .Where(Exp.In("id", GetIdentifiers(result, projectId, EntityType.Project)));

            var milestones = new SqlQuery("projects_milestones t")
                .LeftOuterJoin("projects_projects p", Exp.EqColumns("t.project_id", "p.id"))
                .Select(EntityType.Milestone.ToString("d"), "cast(t.id as char)", "t.title", "p.id", "p.title", "''", "t.create_on")
                .Where(Exp.In("t.id", GetIdentifiers(result, projectId, EntityType.Milestone)));

            var messages = new SqlQuery("projects_messages t")
                .LeftOuterJoin("projects_projects p", Exp.EqColumns("t.project_id", "p.id"))
                .Select(EntityType.Message.ToString("d"), "cast(t.id as char)", "t.title", "p.id", "p.title", "t.content", "t.create_on")
                .Where(Exp.In("t.id", GetIdentifiers(result, projectId, EntityType.Message)));

            var tasks = new SqlQuery("projects_tasks t")
                .LeftOuterJoin("projects_projects p", Exp.EqColumns("t.project_id", "p.id"))
                .Select(EntityType.Task.ToString("d"), "cast(t.id as char)", "t.title", "p.id", "p.title", "t.description", "t.create_on")
                .Where(Exp.In("t.id", GetIdentifiers(result, projectId, EntityType.Task)));

            return projects
                .Union(milestones)
                .Union(tasks)
                .Union(messages);
        }

        private object[] GetIdentifiers(TextSearchResult searchResult, int projectId, EntityType entryType)
        {
            var result = new List<object>() { -1 };
            var ids = new List<string>();

            if (projectId != 0)
            {
                ids.AddRange(searchResult.GetIdentifierDetails(projectId.ToString()));
            }
            else
            {
                ids.AddRange(searchResult.GetIdentifiers());
                foreach (var id in searchResult.GetIdentifiers())
                {
                    ids.AddRange(searchResult.GetIdentifierDetails(id));
                }
            }

            foreach (var id in ids)
            {
                if (entryType == EntityType.Project && char.IsDigit(id, id.Length - 1))
                {
                    result.Add(int.Parse(id));
                }
                else if (entryType == EntityType.Milestone && id.EndsWith("s"))
                {
                    result.Add(int.Parse(id.TrimEnd('s')));
                }
                else if (entryType == EntityType.Task && id.EndsWith("t"))
                {
                    result.Add(int.Parse(id.TrimEnd('t')));
                }
                else if (entryType == EntityType.Message && id.EndsWith("m"))
                {
                    result.Add(int.Parse(id.TrimEnd('m')));
                }
                else if (entryType == EntityType.File && id.EndsWith("f"))
                {
                    result.Add(id.TrimEnd('f'));
                }
            }

            return result.ToArray();
        }
    }
}
