using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Data.DAO
{
    class CachedMilestoneDao : MilestoneDao
    {
        private readonly HttpRequestDictionary<Milestone> _projectCache = new HttpRequestDictionary<Milestone>("milestone");


        public CachedMilestoneDao(string dbId, int tenant)
            : base(dbId, tenant)
        {
        }

        public override Milestone GetById(int id)
        {
            return _projectCache.Get(id.ToString(), () => GetBaseById(id));
        }

        private Milestone GetBaseById(int id)
        {
            return base.GetById(id);
        }

        public override Milestone Save(Milestone milestone)
        {
            if (milestone != null)
            {
                ResetCache(milestone.ID);
            }
            return base.Save(milestone);
        }

        public override void Delete(int id)
        {
            ResetCache(id);
            base.Delete(id);
        }

        private void ResetCache(int milestoneId)
        {
            _projectCache.Reset(milestoneId.ToString());
        }
    }

    class MilestoneDao : BaseDao, IMilestoneDao
    {
        private readonly string table = "projects_milestones";


        public MilestoneDao(string dbId, int tenant)
            : base(dbId, tenant)
        {
        }


        public List<Milestone> GetByProject(int projectId)
        {
            return DbManager
                .ExecuteList(CreateQuery().Where("t.project_id", projectId))
                .ConvertAll(r => ToMilestone(r));
        }

        public List<Milestone> GetUpcomingMilestones(int offset, int max, params int[] projects)
        {
            var query = CreateQuery()
                .SetFirstResult(offset)
                .Where("p.status", ProjectStatus.Open)
                .Where(Exp.Ge("t.deadline", TenantUtil.DateTimeNow().Date))
                .Where("t.status", MilestoneStatus.Open)
                .SetMaxResults(max)
                .OrderBy("t.deadline", true);
            if (projects != null && 0 < projects.Length)
            {
                query.Where(Exp.In("p.id", projects));
            }
            return DbManager
                .ExecuteList(query)
                .ConvertAll(r => ToMilestone(r));
        }
        public List<Milestone> GetLateMilestones(int offset, int max, params int[] projects)
        {
            var query = CreateQuery()
                .SetFirstResult(offset)
                .Where("p.status", ProjectStatus.Open)
                .Where(!Exp.Eq("t.status", MilestoneStatus.Closed))
                .Where(Exp.Le("t.deadline", TenantUtil.DateTimeNow().Date.AddDays(-1)))
                .SetMaxResults(max)
                .OrderBy("t.deadline", true);
            if (projects != null && 0 < projects.Length)
            {
                query.Where(Exp.In("p.id", projects));
            }
            return DbManager
                .ExecuteList(query)
                .ConvertAll(r => ToMilestone(r));
        }

        public List<Milestone> GetByDeadLine(DateTime deadline)
        {
            return DbManager
                .ExecuteList(CreateQuery().Where("t.deadline", deadline.Date).OrderBy("t.deadline", true))
                .ConvertAll(r => ToMilestone(r));
        }

        public virtual Milestone GetById(int id)
        {
            return DbManager
                .ExecuteList(CreateQuery().Where("t.id", id))
                .ConvertAll(r => ToMilestone(r))
                .SingleOrDefault();
        }

        public bool IsExists(int id)
        {
            var count = DbManager.ExecuteScalar<long>(Query(table).SelectCount().Where("id", id));
            return 0 < count;
        }

        public List<object[]> GetInfoForReminder(DateTime deadline)
        {
            var q = new SqlQuery("projects_milestones")
                .Select("tenant_id", "id", "deadline")
                .Where(Exp.Between("deadline", deadline.Date.AddDays(-1), deadline.Date.AddDays(1)))
                .Where("status", MilestoneStatus.Open)
                .Where("is_notify", 1);

            return DbManager
                .ExecuteList(q)
                .ConvertAll(r => new object[] { Convert.ToInt32(r[0]), Convert.ToInt32(r[1]), Convert.ToDateTime(r[2]) });
        }

        
        public virtual Milestone Save(Milestone milestone)
        {
            var insert = Insert(table)
                .InColumnValue("id", milestone.ID)
                .InColumnValue("project_id", milestone.Project != null ? milestone.Project.ID : 0)
                .InColumnValue("title", milestone.Title)
                .InColumnValue("create_by", milestone.CreateBy.ToString())
                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(milestone.CreateOn))
                .InColumnValue("last_modified_by", milestone.LastModifiedBy.ToString())
                .InColumnValue("last_modified_on", TenantUtil.DateTimeToUtc(milestone.LastModifiedOn))
                .InColumnValue("deadline", milestone.DeadLine.Date)
                .InColumnValue("status", milestone.Status)
                .InColumnValue("is_notify", milestone.IsNotify)
                .InColumnValue("is_key", milestone.IsKey)
                .Identity(1, 0, true);
            milestone.ID = DbManager.ExecuteScalar<int>(insert);
            return milestone;
        }

        public virtual void Delete(int id)
        {
            using (var tx = DbManager.BeginTransaction())
            {
                DbManager.ExecuteNonQuery(Delete("projects_comments").Where("target_uniq_id", ProjectEntity.BuildUniqId<Milestone>(id)));
                DbManager.ExecuteNonQuery(new SqlDelete("projects_review_entity_info").Where("entity_uniqID", "Milestone_" + id));
                DbManager.ExecuteNonQuery(Update("projects_tasks").Set("milestone_id", 0).Where("milestone_id", id));
                DbManager.ExecuteNonQuery(Delete(table).Where("id", id));

                tx.Commit();
            }
        }

        public bool CanReadMilestones(int projectId, Guid userId)
        {
            return 0 < DbManager.ExecuteScalar<int>(Query("projects_tasks").SelectCount().Where("project_id", projectId).Where(!Exp.Eq("milestone_id", 0)).Where("responsible_id", userId.ToString()));
        }


        private SqlQuery CreateQuery()
        {
            return new SqlQuery(table + " t")
                .LeftOuterJoin(ProjectDao.PROJECT_TABLE + " p", Exp.EqColumns("t.project_id", "p.id"))
                .Select(ProjectDao.PROJECT_COLUMNS.Select(c => "p." + c).ToArray())
                .Select("t.id", "t.title", "t.create_by", "t.create_on", "t.last_modified_by", "t.last_modified_on")
                .Select("t.deadline", "t.status", "t.is_notify", "t.is_key")
                .Select(new SqlQuery("projects_tasks k").SelectCount().Where(Exp.EqColumns("k.milestone_id", "t.id")).Where("k.responsible_id", SecurityContext.CurrentAccount.ID.ToString()))
                .Where("t.tenant_id", Tenant)
                .Where("p.tenant_id", Tenant);
        }

        private Milestone ToMilestone(object[] r)
        {
            var offset = ProjectDao.PROJECT_COLUMNS.Length;
            return new Milestone()
            {
                Project = r[0] != null ? ProjectDao.ToProject(r) : null,
                ID = Convert.ToInt32(r[0 + offset]),
                Title = (string)r[1 + offset],
                CreateBy = ToGuid(r[2 + offset]),
                CreateOn = TenantUtil.DateTimeFromUtc((DateTime)r[3 + offset]),
                LastModifiedBy = ToGuid(r[4 + offset]),
                LastModifiedOn = TenantUtil.DateTimeFromUtc((DateTime)r[5 + offset]),
                DeadLine = (DateTime)r[6 + offset],
                Status = (MilestoneStatus)Convert.ToInt32(r[7 + offset]),
                IsNotify = Convert.ToBoolean(r[8 + offset]),
                IsKey = Convert.ToBoolean(r[9 + offset]),
                CurrentUserHasTasks = Convert.ToBoolean(r[10 + offset]),
            };
        }
    }
}
