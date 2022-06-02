using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Data.DAO
{
    class TemplateDao : BaseDao, ITemplateDao
    {
        private string projectTable = "projects_template_project";
        private string milestoneTable = "projects_template_milestone";
        private string messageTable = "projects_template_message";
        private string taskTable = "projects_template_task";
        private string[] projectColumns = new[] { "id", "title", "description", "responsible", "tags", "team", "create_by", "create_on", };
        private string[] milestoneColumns = new[] { "id", "title", "project_id", "duration", "flags", "create_by", "create_on", };
        private string[] messageColumns = new[] { "id", "title", "project_id", "text", "create_by", "create_on", };
        private string[] taskColumns = new[] { "id", "title", "project_id", "description", "milestone_id", "priority", "sort_order", "responsible", "create_by", "create_on", };


        public TemplateDao(string dbId, int tenant)
            : base(dbId, tenant)
        {

        }


        public List<TemplateProject> GetTemplateProjects()
        {
            var q = Query(projectTable)
                .Select(projectColumns)
                .Select(new SqlQuery(milestoneTable).SelectCount().Where(Exp.EqColumns("project_id", projectTable + ".id")))
                .Select(new SqlQuery(messageTable).SelectCount().Where(Exp.EqColumns("project_id", projectTable + ".id")))
                .Select(new SqlQuery(taskTable).SelectCount().Where(Exp.EqColumns("project_id", projectTable + ".id")))
                .OrderBy("title", true);
            return DbManager
                .ExecuteList(q)
                .ConvertAll(r => ToProject(r));
        }

        public List<TemplateMilestone> GetTemplateMilestones(int projectId)
        {
            return DbManager
                .ExecuteList(
                    Query(milestoneTable)
                    .Select(milestoneColumns)
                    .Select(new SqlQuery(taskTable).SelectCount().Where(Exp.EqColumns("milestone_id", milestoneTable + ".id")))
                    .Where("project_id", projectId))
                .ConvertAll(r => ToMilestone(r));
        }

        public List<TemplateMessage> GetTemplateMessages(int projectId)
        {
            return DbManager
                .ExecuteList(Query(messageTable).Select(messageColumns).Where("project_id", projectId))
                .ConvertAll(r => ToMessage(r));
        }

        public List<TemplateTask> GetTemplateTasks(int projectId)
        {
            return DbManager
                .ExecuteList(Query(taskTable).Select(taskColumns).Where("project_id", projectId)
										.OrderBy("sort_order", false)
										.OrderBy("priority", false)
										.OrderBy("create_on", false))
                .ConvertAll(r => ToTask(r));
        }

        public TemplateProject GetTemplateProject(int id)
        {
            var q = Query(projectTable)
                .Select(projectColumns)
                .Select(new SqlQuery(milestoneTable).SelectCount().Where(Exp.EqColumns("project_id", projectTable + ".id")))
                .Select(new SqlQuery(messageTable).SelectCount().Where(Exp.EqColumns("project_id", projectTable + ".id")))
                .Select(new SqlQuery(taskTable).SelectCount().Where(Exp.EqColumns("project_id", projectTable + ".id")))
                .Where("id", id);
            return DbManager
                .ExecuteList(q)
                .ConvertAll(r => ToProject(r))
                .SingleOrDefault();
        }

        public TemplateMilestone GetTemplateMilestone(int id)
        {
            return DbManager
                .ExecuteList(
                    Query(milestoneTable)
                    .Select(milestoneColumns)
                    .Select(new SqlQuery(taskTable).SelectCount().Where(Exp.EqColumns("milestone_id", milestoneTable + ".id")))
                    .Where("id", id))
                .ConvertAll(r => ToMilestone(r))
                .SingleOrDefault();
        }

        public TemplateMessage GetTemplateMessage(int id)
        {
            return DbManager
                .ExecuteList(Query(messageTable).Select(messageColumns).Where("id", id))
                .ConvertAll(r => ToMessage(r))
                .SingleOrDefault();
        }

        public TemplateTask GetTemplateTask(int id)
        {
            return DbManager
                .ExecuteList(Query(taskTable).Select(taskColumns).Where("id", id))
                .ConvertAll(r => ToTask(r))
                .SingleOrDefault();
        }

        public TemplateProject SaveTemplateProject(TemplateProject t)
        {
            if (t == null) throw new ArgumentNullException("template");

            var insert = Insert(projectTable)
                .InColumns(projectColumns)
                .Values(t.Id, t.Title, t.Description, t.Responsible.ToString(), t.Tags)
                .Values(string.Join(",", t.Team.Select(g => g.ToString()).ToArray()), t.CreateBy.ToString(), TenantUtil.DateTimeToUtc(t.CreateOn))
                .Identity(1, 0, true);
            t.Id = DbManager.ExecuteScalar<int>(insert);
            return t;
        }

        public TemplateMilestone SaveTemplateMilestone(TemplateMilestone t)
        {
            if (t == null) throw new ArgumentNullException("template");

            var insert = Insert(milestoneTable)
                .InColumns(milestoneColumns)
                .Values(t.Id, t.Title, t.ProjectId, t.DurationInDays, t.Flags, t.CreateBy.ToString(), TenantUtil.DateTimeToUtc(t.CreateOn))
                .Identity(1, 0, true);
            t.Id = DbManager.ExecuteScalar<int>(insert);
            return t;
        }

        public TemplateMessage SaveTemplateMessage(TemplateMessage t)
        {
            if (t == null) throw new ArgumentNullException("template");

            var insert = Insert(messageTable)
                .InColumns(messageColumns)
                .Values(t.Id, t.Title, t.ProjectId, t.Text, t.CreateBy.ToString(), TenantUtil.DateTimeToUtc(t.CreateOn))
                .Identity(1, 0, true);
            t.Id = DbManager.ExecuteScalar<int>(insert);
            return t;
        }

        public TemplateTask SaveTemplateTask(TemplateTask t)
        {
            if (t == null) throw new ArgumentNullException("template");

            var insert = Insert(taskTable)
                .InColumns(taskColumns)
                .Values(t.Id, t.Title, t.ProjectId, t.Description, t.MilestoneId, t.Priority, t.SortOrder, t.Responsible.ToString(), t.CreateBy.ToString(), TenantUtil.DateTimeToUtc(t.CreateOn))
                .Identity(1, 0, true);
            t.Id = DbManager.ExecuteScalar<int>(insert);
            return t;
        }

        public void RemoveTemplateProject(int id)
        {
            using (var tx = DbManager.BeginTransaction())
            {
                DbManager.ExecuteNonQuery(Delete(projectTable).Where("id", id));
                DbManager.ExecuteNonQuery(Delete(milestoneTable).Where("project_id", id));
                DbManager.ExecuteNonQuery(Delete(messageTable).Where("project_id", id));
                DbManager.ExecuteNonQuery(Delete(taskTable).Where("project_id", id));
                tx.Commit();
            }
        }

        public void RemoveTemplateMilestone(int id)
        {
            DbManager.ExecuteNonQuery(Delete(milestoneTable).Where("id", id));
        }

        public void RemoveTemplateMessage(int id)
        {
            DbManager.ExecuteNonQuery(Delete(messageTable).Where("id", id));
        }

        public void RemoveTemplateTask(int id)
        {
            DbManager.ExecuteNonQuery(Delete(taskTable).Where("id", id));
        }


        private TemplateProject ToProject(object[] r)
        {
            var p = new TemplateProject((string)r[1])
            {
                Id = Convert.ToInt32(r[0]),
                Description = (string)r[2],
                Tags = (string)r[4],
                Responsible = r[3] != null ? new Guid((string)r[3]) : Guid.Empty,
                CreateBy = new Guid((string)r[6]),
                CreateOn = TenantUtil.DateTimeFromUtc((DateTime)r[7]),
                MilestonesCount = Convert.ToInt32(r[8]),
                MessagesCount = Convert.ToInt32(r[9]),
                TasksCount = Convert.ToInt32(r[10]),
            };
            if (!string.IsNullOrEmpty((string)r[5]))
            {
                p.Team.AddRange(((string)r[5]).Split(',').Select(g => new Guid(g)));
            }
            return p;
        }

        private TemplateMilestone ToMilestone(object[] r)
        {
            return new TemplateMilestone(Convert.ToInt32(r[2]), (string)r[1])
            {
                Id = Convert.ToInt32(r[0]),
                DurationInDays = Convert.ToInt32(r[3]),
                Flags = Convert.ToInt32(r[4]),
                CreateBy = new Guid((string)r[5]),
                CreateOn = TenantUtil.DateTimeFromUtc((DateTime)r[6]),
                TasksCount = Convert.ToInt32(r[7]),
            };
        }

        private TemplateMessage ToMessage(object[] r)
        {
            return new TemplateMessage(Convert.ToInt32(r[2]), (string)r[1])
            {
                Id = Convert.ToInt32(r[0]),
                Text = (string)r[3],
                CreateBy = new Guid((string)r[4]),
                CreateOn = TenantUtil.DateTimeFromUtc((DateTime)r[5]),
            };
        }

        private TemplateTask ToTask(object[] r)
        {
            return new TemplateTask(Convert.ToInt32(r[2]), (string)r[1], Convert.ToInt32(r[4]))
            {
                Id = Convert.ToInt32(r[0]),
                Description = (string)r[3],
                MilestoneId = Convert.ToInt32(r[4]),
                Priority = (TaskPriority)Convert.ToInt32(r[5]),
                SortOrder = Convert.ToInt32(r[6]),
                Responsible = r[7] != null ? new Guid((string)r[7]) : Guid.Empty,
                CreateBy = new Guid((string)r[8]),
                CreateOn = TenantUtil.DateTimeFromUtc((DateTime)r[9]),
            };
        }

		#region Sort Orders
		public void SetTaskOrders(int? milestoneId, int taskID, int? prevTaskID, int? nextTaskID)
		{
			using (var tx = DbManager.BeginTransaction())
			{
				var query = Query(taskTable).Select(taskColumns).Where("milestone_id", milestoneId)
									.OrderBy("sort_order", false)
									.OrderBy("priority", false)
									.OrderBy("create_on", false);

				var tasks = DbManager
								.ExecuteList(query)
								.ConvertAll(r => ToTask(r));

				var sortTask = tasks.Find(t => t.Id == taskID);

				if (sortTask != null)
				{
					tasks.RemoveAll(t => t.Id == taskID);
					if (prevTaskID.HasValue)
					{
						var ind = tasks.FindIndex(t => t.Id == prevTaskID);
						if (ind != -1 && ind != tasks.Count - 1)
							tasks.Insert(ind + 1, sortTask);
						else
							tasks.Add(sortTask);
					}
					else if (nextTaskID.HasValue)
					{
						var ind = tasks.FindIndex(t => t.Id == nextTaskID);
						if (ind > 0)
							tasks.Insert(ind - 1, sortTask);
						else
							tasks.Insert(0, sortTask);
					}
				}

				for (int i = 0; i < tasks.Count; i++)
					DbManager.ExecuteNonQuery(new SqlUpdate(taskTable).Set("sort_order", tasks.Count - i)
												  .Where("id", tasks[i].Id));

				tx.Commit();
			}
		} 
		#endregion
    }
}
