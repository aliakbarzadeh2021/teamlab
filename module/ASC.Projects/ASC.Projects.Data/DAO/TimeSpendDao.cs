using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Data.DAO
{
    class TimeSpendDao : BaseDao, ITimeSpendDao
    {
        private readonly string[] columns = new[] { "id", "note", "date", "hours", "relative_task_id", "person_id", "project_id" };


        public TimeSpendDao(string dbId, int tenantID) : base(dbId, tenantID) { }


        public List<TimeSpend> GetByProject(int projectId)
        {
            return DbManager
                .ExecuteList(
                    Query("projects_time_tracking")
                    .Select(columns)
                    .Where("project_id", projectId)
                )
                .ConvertAll(r => ToTimeSpend(r));
        }

        public List<TimeSpend> GetByTask(int taskId)
        {
            return DbManager
                .ExecuteList(
                    Query("projects_time_tracking")
                    .Select(columns)
                    .Where("relative_task_id", taskId)
                )
                .ConvertAll(r => ToTimeSpend(r));
        }

        public TimeSpend GetById(int id)
        {
            return DbManager
                .ExecuteList(
                    Query("projects_time_tracking")
                    .Select(columns)
                    .Where("id", id)
                )
                .ConvertAll(r => ToTimeSpend(r))
                .SingleOrDefault();
        }


        public bool HasTime(int taskId)
        {
            var count = DbManager.ExecuteScalar<int>(Query("projects_time_tracking").SelectCount().Where("relative_task_id", taskId));
            return count != 0;
        }

        public Dictionary<int, bool> HasTime(params int[] tasks)
        {
            var result = new Dictionary<int, bool>();
            if (tasks == null || 0 == tasks.Length) return result;

            tasks.ToList().ForEach(id => result[id] = false);

            DbManager
                .ExecuteList(
                    Query("projects_time_tracking")
                    .Select("relative_task_id")
                    .SelectCount()
                    .Where(Exp.In("relative_task_id", tasks))
                    .GroupBy(1)
                )
                .ForEach(r => result[Convert.ToInt32(r[0])] = (long)r[1] != 0);

            return result;
        }


        public TimeSpend Save(TimeSpend timeSpend)
        {
            var insert = Insert("projects_time_tracking")
                .InColumns(columns)
                .Values(
                    timeSpend.ID,
                    timeSpend.Note,
                    TenantUtil.DateTimeToUtc(timeSpend.Date),
                    timeSpend.Hours,
                    timeSpend.RelativeTask.ToString(),
                    timeSpend.Person.ToString(),
                    timeSpend.Project
                )
                .Identity(1, 0, true);
            timeSpend.ID = DbManager.ExecuteScalar<int>(insert);
            return timeSpend;
        }

        public void Delete(int id)
        {
            DbManager.ExecuteNonQuery(Delete("projects_time_tracking").Where("id", id));
        }


        private TimeSpend ToTimeSpend(object[] r)
        {
            return new TimeSpend()
            {
                ID = Convert.ToInt32(r[0]),
                Note = (string)r[1],
                Date = TenantUtil.DateTimeFromUtc((DateTime)r[2]),
                Hours = Convert.ToSingle(r[3]),
                RelativeTask = Convert.ToInt32(r[4]),
                Person = ToGuid(r[5]),
                Project = Convert.ToInt32(r[6])
            };
        }
    }
}
