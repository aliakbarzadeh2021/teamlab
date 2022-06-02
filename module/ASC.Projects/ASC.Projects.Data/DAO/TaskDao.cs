using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Data.DAO
{
    internal class CachedTaskDao : TaskDao
    {
        private readonly HttpRequestDictionary<Task> _taskCache = new HttpRequestDictionary<Task>("task");

        public CachedTaskDao(string dbId, int tenantID)
            : base(dbId, tenantID)
        {
        }

        public override void Delete(int id)
        {
            ResetCache(id);
            base.Delete(id);
        }

        public override void SetTaskOrders(int? milestoneId, int taskID, int? prevTaskID, int? nextTaskID)
        {
            ResetCache(taskID);
            base.SetTaskOrders(milestoneId, taskID, prevTaskID, nextTaskID);
        }

        public override Task GetById(int id)
        {
            return _taskCache.Get(id.ToString(), () => GetBaseById(id));
        }

        private Task GetBaseById(int id)
        {
            return base.GetById(id);
        }

        public override Task Save(Task task)
        {
            if (task != null)
            {
                ResetCache(task.ID);
            }
            return base.Save(task);
        }

        private void ResetCache(int taskId)
        {
            _taskCache.Reset(taskId.ToString());
        }
    }


    class TaskDao : BaseDao, ITaskDao
    {
        private readonly string table = "projects_tasks";


        public TaskDao(string dbId, int tenantID)
            : base(dbId, tenantID)
        {
        }


        public List<Task> GetByProject(int projectId, TaskStatus? status, Guid participant)
        {
            var query = CreateQuery()
                .LeftOuterJoin("projects_milestones m", Exp.EqColumns("m.id", "t.milestone_id"))
                .Where("t.project_id", projectId)
                .OrderBy("t.sort_order", false)
                .OrderBy("m.status", true)
                .OrderBy("m.deadLine", true)
                .OrderBy("m.id", true)
                .OrderBy("t.status", true)
                .OrderBy("t.priority", true)
                .OrderBy("t.create_on", true);
            if (status != null)
            {
                query.Where("t.status", status);
            }
            if (participant != Guid.Empty)
            {
                query.Where("t.responsible_id", participant.ToString());
            }

            return DbManager
                .ExecuteList(query)
                .ConvertAll(r => ToTask(r));
        }

        public List<Task> GetByResponsible(Guid responsibleId, IEnumerable<TaskStatus> statuses)
        {
            var q = CreateQuery()
                .Where("t.responsible_id", responsibleId.ToString())
                .OrderBy("t.sort_order", false)
                .OrderBy("t.status", true)
                .OrderBy("t.priority", true)
                .OrderBy("t.create_on", true);

            if (statuses != null && 0 < statuses.Count())
            {
                q.Where(Exp.In("t.status", statuses.ToList()));
            }

            return DbManager
                .ExecuteList(q)
                .ConvertAll(r => ToTask(r));
        }

        public List<Task> GetLastTasks(Guid participant, int max, params int[] projects)
        {
            var query = CreateQuery()
                .Where("t.responsible_id", participant.ToString())
                .Where("p.status", ProjectStatus.Open)
                .Where(!Exp.Eq("t.status", TaskStatus.Closed))
                .SetMaxResults(max)
                .OrderBy("t.create_on", false);
            if (projects != null && 0 < projects.Length)
            {
                query.Where(Exp.In("t.project_id", projects));
            }
            return DbManager
                .ExecuteList(query)
                .ConvertAll(r => ToTask(r));
        }

        public List<Task> GetMilestoneTasks(int milestoneId)
        {
            var query = CreateQuery()
                .Where("t.milestone_id", milestoneId)
                .OrderBy("t.sort_order", false)
                .OrderBy("t.status", true)
                .OrderBy("t.priority", false)
                .OrderBy("t.create_on", false);

            return DbManager
                .ExecuteList(query)
                .ConvertAll(r => ToTask(r));
        }

        public int GetTaskCount(int milestoneId, params  TaskStatus[] statuses)
        {
            var query = Query(table)
                .SelectCount()
                .Where("milestone_id", milestoneId);
            if (statuses != null && 0 < statuses.Length)
            {
                query.Where(Exp.In("status", statuses));
            }
            return DbManager.ExecuteScalar<int>(query);
        }

        public List<Task> GetById(ICollection<int> ids)
        {
            return DbManager
                .ExecuteList(CreateQuery().Where(Exp.In("t.id", ids.ToArray())))
                .ConvertAll(r => ToTask(r));
        }
        public virtual Task GetById(int id)
        {
            return DbManager
                .ExecuteList(CreateQuery().Where("t.id", id))
                .ConvertAll(r => ToTask(r))
                .SingleOrDefault();
        }

        public bool IsExists(int id)
        {
            var count = DbManager.ExecuteScalar<long>(Query(table).SelectCount().Where("id", id));
            return 0 < count;
        }

        public List<object[]> GetTasksForReminder(DateTime deadline)
        {
            var q = new SqlQuery("projects_tasks")
                .Select("tenant_id", "id", "deadline")
                .Where(Exp.Between("deadline", deadline.Date.AddDays(-1), deadline.Date.AddDays(1)))
                .Where(!Exp.Eq("status", TaskStatus.Unclassified) & !Exp.Eq("status", TaskStatus.Closed));

            return DbManager
                .ExecuteList(q)
                .ConvertAll(r => new object[] { Convert.ToInt32(r[0]), Convert.ToInt32(r[1]), Convert.ToDateTime(r[2]) });
        }


        public virtual void SetTaskOrders(int? milestoneId, int taskID, int? prevTaskID, int? nextTaskID)
        {
            using (var tr = DbManager.Connection.BeginTransaction())
            {
                var projID = DbManager.ExecuteScalar<int>(new SqlQuery(table).Select("project_id").Where("id", taskID));

                var query = CreateQuery()
                .Where(Exp.Eq("t.milestone_id", milestoneId) & Exp.Eq("t.project_id", projID))
                .OrderBy("t.sort_order", false)
                .OrderBy("t.status", true)
                .OrderBy("t.priority", false)
                .OrderBy("t.create_on", false);

                var tasks = DbManager
                    .ExecuteList(query)
                    .ConvertAll(r => ToTask(r));

                var sortTask = tasks.Find(t => t.ID == taskID);

                if (sortTask != null)
                {
                    tasks.RemoveAll(t => t.ID == taskID);
                    if (prevTaskID.HasValue)
                    {
                        var ind = tasks.FindIndex(t => t.ID == prevTaskID);
                        if (ind != -1 && ind != tasks.Count - 1)
                            tasks.Insert(ind + 1, sortTask);
                        else
                            tasks.Add(sortTask);
                    }
                    else if (nextTaskID.HasValue)
                    {
                        var ind = tasks.FindIndex(t => t.ID == nextTaskID);
                        if (ind > 0)
                            tasks.Insert(ind - 1, sortTask);
                        else
                            tasks.Insert(0, sortTask);
                    }
                }

                for (int i = 0; i < tasks.Count; i++)
                    DbManager.ExecuteNonQuery(new SqlUpdate(table).Set("sort_order", tasks.Count - i)
                                                  .Where("id", tasks[i].ID));

                tr.Commit();
            }
        }

        public virtual Task Save(Task task)
        {
            using (var tr = DbManager.Connection.BeginTransaction())
            {
                if (task.SortOrder == 0)
                {
                    task.SortOrder = DbManager.ExecuteScalar<int>(
                        new SqlQuery(table)
                        .SelectMax("sort_order")
                        .Where("project_id", task.Project != null ? task.Project.ID : 0)
                        .Where("milestone_id", task.Milestone));
                    task.SortOrder++;
                }

                var insert = Insert(table)
                    .InColumnValue("id", task.ID)
                    .InColumnValue("project_id", task.Project != null ? task.Project.ID : 0)
                    .InColumnValue("title", task.Title)
                    .InColumnValue("create_by", task.CreateBy.ToString())
                    .InColumnValue("create_on", TenantUtil.DateTimeToUtc(task.CreateOn))
                    .InColumnValue("last_modified_by", task.LastModifiedBy.ToString())
                    .InColumnValue("last_modified_on", TenantUtil.DateTimeToUtc(task.LastModifiedOn))
                    .InColumnValue("description", task.Description)
                    .InColumnValue("responsible_id", task.Responsible.ToString())
                    .InColumnValue("priority", task.Priority)
                    .InColumnValue("status", task.Status)
                    .InColumnValue("milestone_id", task.Milestone)
                    .InColumnValue("sort_order", task.SortOrder)
                    .InColumnValue("deadline", task.Deadline.Date)
                    .Identity(1, 0, true);
                task.ID = DbManager.ExecuteScalar<int>(insert);

                tr.Commit();
            }
            return task;
        }

        public virtual void Delete(int id)
        {
            using (var tx = DbManager.BeginTransaction())
            {
                DbManager.ExecuteNonQuery(Delete("projects_comments").Where("target_uniq_id", ProjectEntity.BuildUniqId<Task>(id)));
                DbManager.ExecuteNonQuery(new SqlDelete("projects_review_entity_info").Where("entity_uniqID", "Task_" + id));
                DbManager.ExecuteNonQuery(Delete("projects_tasks_trace").Where("task_id", id));
                DbManager.ExecuteNonQuery(Delete(table).Where("id", id));

                tx.Commit();
            }
        }

        public void TaskTrace(int target, Guid owner, DateTime date, TaskStatus status)
        {
            var insert = Insert("projects_tasks_trace")
                .InColumnValue("task_id", target)
                .InColumnValue("action_owner_id", owner.ToString())
                .InColumnValue("action_date", TenantUtil.DateTimeToUtc(date))
                .InColumnValue("status", status);
            DbManager.ExecuteNonQuery(insert);
        }


        private SqlQuery CreateQuery()
        {
            return new SqlQuery(table + " t")
                .LeftOuterJoin(ProjectDao.PROJECT_TABLE + " p", Exp.EqColumns("t.project_id", "p.id"))
                .Select(ProjectDao.PROJECT_COLUMNS.Select(c => "p." + c).ToArray())
                .Select("t.id", "t.title", "t.create_by", "t.create_on", "t.last_modified_by", "t.last_modified_on")
                .Select("t.description", "t.responsible_id", "t.priority", "t.status", "t.milestone_id", "t.sort_order", "t.deadline")
                .Where("t.tenant_id", Tenant)
                .Where("p.tenant_id", Tenant);
        }

        private Task ToTask(object[] r)
        {
            var offset = ProjectDao.PROJECT_COLUMNS.Length;
            return new Task()
            {
                Project = r[0] != null ? ProjectDao.ToProject(r) : null,
                ID = Convert.ToInt32(r[0 + offset]),
                Title = (string)r[1 + offset],
                CreateBy = ToGuid(r[2 + offset]),
                CreateOn = TenantUtil.DateTimeFromUtc((DateTime)r[3 + offset]),
                LastModifiedBy = ToGuid(r[4 + offset]),
                LastModifiedOn = TenantUtil.DateTimeFromUtc((DateTime)r[5 + offset]),
                Description = (string)r[6 + offset],
                Responsible = ToGuid(r[7 + offset]),
                Priority = (TaskPriority)Convert.ToInt32(r[8 + offset]),
                Status = (TaskStatus)Convert.ToInt32(r[9 + offset]),
                Milestone = r[10 + offset] == null ? 0 : Convert.ToInt32(r[10 + offset]),
                SortOrder = Convert.ToInt32(r[11 + offset]),
                Deadline = r[12 + offset] != null ? (DateTime)r[12 + offset] : default(DateTime),
            };
        }
    }
}
