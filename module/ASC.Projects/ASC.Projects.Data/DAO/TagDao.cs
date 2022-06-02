using System;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Projects.Core.DataInterfaces;

namespace ASC.Projects.Data.DAO
{
    class TagDao : BaseDao, ITagDao
    {
        public TagDao(string dbId, int tenantID)
            : base(dbId, tenantID)
        {
        }

        public string[] GetTags()
        {
            return DbManager
                .ExecuteList(GetTagQuery(Exp.Empty))
                .ConvertAll(r => ToTag(r))
                .ToArray();
        }

        public string[] GetTags(string prefix)
        {
            return DbManager
                .ExecuteList(GetTagQuery(Exp.Like("title", prefix, SqlLike.StartWith)))
                .ConvertAll(r => ToTag(r))
                .ToArray();
        }


        public int[] GetTagProjects(string tag)
        {
            //select project_id from projects_tags, projects_project_tag
            //where tenant_id = 0 and id = tag_id and title = ?
            return DbManager
                .ExecuteList(Query("projects_tags").From("projects_project_tag").Where(Exp.EqColumns("id", "tag_id") & Exp.Eq("lower(title)", tag.ToLower())).Select("project_id"))
                .ConvertAll(r => Convert.ToInt32(r[0]))
                .ToArray();
        }


        public string[] GetProjectTags(int projectId)
        {
            //select title from projects_tags, projects_project_tag
            //where tenant_id = 0 and id = tag_id and project_id = ?
            return DbManager
                .ExecuteList(GetTagQuery(Exp.EqColumns("id", "tag_id") & Exp.Eq("project_id", projectId)).From("projects_project_tag"))
                .ConvertAll(r => ToTag(r))
                .ToArray();
        }

        public string[] GetProjectRequestTags(int requestId)
        {
            //select title from projects_tags, projects_project_tag_change_request
            //where tenant_id = 0 and id = tag_id and project_id = ?
            return DbManager
                .ExecuteList(GetTagQuery(Exp.EqColumns("id", "tag_id") & Exp.Eq("project_id", requestId)).From("projects_project_tag_change_request"))
                .ConvertAll(r => ToTag(r))
                .ToArray();
        }

        public void SetProjectTags(int projectId, string[] tags)
        {
            using (var tx = DbManager.BeginTransaction())
            {
                DbManager.ExecuteNonQuery(new SqlDelete("projects_project_tag").Where("project_id", projectId));
                foreach (var tag in tags)
                {
                    var tagId = DbManager.ExecuteScalar<int>(Query("projects_tags").Select("id").Where("lower(title)", tag.ToLower()));
                    if (tagId == 0)
                    {
                        tagId = DbManager.ExecuteScalar<int>(
                            Insert("projects_tags")
                            .InColumnValue("id", 0)
                            .InColumnValue("title", tag)
                            .InColumnValue("last_modified_by", DateTime.UtcNow)
                            .Identity(1, 0, true));
                    }
                    DbManager.ExecuteNonQuery(new SqlInsert("projects_project_tag").InColumnValue("tag_id", tagId).InColumnValue("project_id", projectId));
                }
                tx.Commit();
            }
        }

        public void SetProjectRequestTags(int requestId, string[] tags)
        {
            using (var tx = DbManager.BeginTransaction())
            {
                DbManager.ExecuteNonQuery(new SqlDelete("projects_project_tag_change_request").Where("project_id", requestId));
                foreach (var tag in tags)
                {
                    var tagId = DbManager.ExecuteScalar<int>(Query("projects_tags").Select("id").Where("lower(title)", tag.ToLower()));
                    if (tagId == 0)
                    {
                        tagId = DbManager.ExecuteScalar<int>(
                            Insert("projects_tags")
                            .InColumnValue("id", 0)
                            .InColumnValue("title", tag)
                            .InColumnValue("last_modified_by", DateTime.UtcNow)
                            .Identity(1, 0, true));
                    }
                    DbManager.ExecuteNonQuery(new SqlInsert("projects_project_tag_change_request").InColumnValue("tag_id", tagId).InColumnValue("project_id", requestId));
                }
                tx.Commit();
            }
        }


        private SqlQuery GetTagQuery(Exp where)
        {
            return Query("projects_tags")
                .Select("title")
                .Where(where);
        }

        private string ToTag(object[] r)
        {
            return (string)r[0];
        }
    }
}
