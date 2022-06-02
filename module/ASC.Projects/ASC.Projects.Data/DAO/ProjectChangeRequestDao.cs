using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Data.DAO
{
    class ProjectChangeRequestDao : BaseDao, IProjectChangeRequestDao
    {
        private readonly string[] columns = new[] { "id", "project_status", "is_edit_request", "project_id", "title", "description", "responsible_id", "create_by", "create_on", "template_id", "private" };


        public ProjectChangeRequestDao(string dbId, int tenantID) : base(dbId, tenantID) { }


        public List<ProjectChangeRequest> GetAll()
        {
            return DbManager
                .ExecuteList(Query("projects_project_change_request").Select(columns).OrderBy("create_on", true))
                .ConvertAll(r => ToRequest(r));
        }

        public ProjectChangeRequest GetById(int id)
        {
            return DbManager
                .ExecuteList(Query("projects_project_change_request").Select(columns).Where("id", id))
                .ConvertAll(r => ToRequest(r))
                .SingleOrDefault();
        }

        public ProjectChangeRequest Save(ProjectChangeRequest request)
        {
            var insert = Insert("projects_project_change_request")
                .InColumns(columns)
                .Values(
                    request.ID,
                    request.Status,
                    (int)request.RequestType,
                    request.ProjectID,
                    request.Title,
                    request.Description,
                    request.Responsible.ToString(),
                    request.CreateBy.ToString(),
                    TenantUtil.DateTimeToUtc(request.CreateOn),
                    request.TemplateId,
                    request.Private)
                .Identity(1, 0, true);
            request.ID = DbManager.ExecuteScalar<int>(insert);
            return request;
        }

        public void Delete(int id)
        {
            using (var tx = DbManager.BeginTransaction())
            {
                DbManager.ExecuteNonQuery(new SqlDelete("projects_project_tag_change_request").Where("project_id", id));
                DbManager.ExecuteNonQuery(Delete("projects_project_change_request").Where("id", id));

                tx.Commit();
            }
        }


        private ProjectChangeRequest ToRequest(object[] r)
        {
            return new ProjectChangeRequest()
            {
                ID = Convert.ToInt32(r[0]),
                Status = (ProjectStatus)Convert.ToInt32(r[1]),
                RequestType = (ProjectRequestType)Convert.ToInt32(r[2]),
                ProjectID = Convert.ToInt32(r[3]),
                Title = (string)r[4],
                Description = (string)r[5],
                Responsible = ToGuid(r[6]),
                CreateBy = ToGuid(r[7]),
                CreateOn = TenantUtil.DateTimeFromUtc((DateTime)r[8]),
                TemplateId = Convert.ToInt32(r[9]),
                Private = Convert.ToBoolean(r[10]),
            };
        }
    }
}
