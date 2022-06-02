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
    internal class CachedMessageDao : MessageDao
    {
        private readonly HttpRequestDictionary<Message> _messageCache = new HttpRequestDictionary<Message>("message");

        public CachedMessageDao(string dbId, int tenant) : base(dbId, tenant)
        {
        }

        public override Message GetById(int id)
        {
            return _messageCache.Get(id.ToString(), () => GetBaseById(id));
        }

        private Message GetBaseById(int id)
        {
            return base.GetById(id);
        }

        public override Message Save(Message msg)
        {
            if (msg != null)
            {
                ResetCache(msg.ID);
            }
            return base.Save(msg);
        }

        public override void Delete(int id)
        {
            ResetCache(id);
            base.Delete(id);
        }

        private void ResetCache(int messageId)
        {
            _messageCache.Reset(messageId.ToString());
        }
    }

    class MessageDao : BaseDao, IMessageDao
    {
        private readonly string table = "projects_messages";


        public MessageDao(string dbId, int tenant)
            : base(dbId, tenant)
        {
        }


        public List<Message> GetByProject(int projectId)
        {
            return DbManager
                .ExecuteList(CreateQuery().Where("t.project_id", projectId).OrderBy("t.create_on", false))
                .ConvertAll(r => ToMessage(r));
        }

        public List<Message> GetMessages(int startIndex, int max)
        {
            var query = CreateQuery()
                .OrderBy("t.create_on", false)
                .SetFirstResult(startIndex)
                .SetMaxResults(max);
            return DbManager
                .ExecuteList(query)
                .ConvertAll(r => ToMessage(r));
        }

        public List<Message> GetRecentMessages(int offset, int max, params int[] projects)
        {
            var query = CreateQuery()
                .SetFirstResult(offset)
                .OrderBy("t.create_on", false)
                .SetMaxResults(max);
            if (projects != null && 0 < projects.Length)
            {
                query.Where(Exp.In("t.project_id", projects));
            }
            return DbManager
                .ExecuteList(query)
                .ConvertAll(r => ToMessage(r));
        }

        public virtual Message GetById(int id)
        {
            return DbManager
                .ExecuteList(CreateQuery().Where("t.id", id))
                .ConvertAll(r => ToMessage(r))
                .SingleOrDefault();
        }

        public bool IsExists(int id)
        {
            var count = DbManager.ExecuteScalar<long>(Query(table).SelectCount().Where("id", id));
            return 0 < count;
        }

        public virtual Message Save(Message msg)
        {
            var insert = Insert(table)
                .InColumnValue("id", msg.ID)
                .InColumnValue("project_id", msg.Project != null ? msg.Project.ID : 0)
                .InColumnValue("title", msg.Title)
                .InColumnValue("create_by", msg.CreateBy.ToString())
                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(msg.CreateOn))
                .InColumnValue("last_modified_by", msg.LastModifiedBy.ToString())
                .InColumnValue("last_modified_on", TenantUtil.DateTimeToUtc(msg.LastModifiedOn))
                .InColumnValue("content", msg.Content)
                .Identity(1, 0, true);
            msg.ID = DbManager.ExecuteScalar<int>(insert);
            return msg;
        }

        public virtual void Delete(int id)
        {
            using (var tx = DbManager.BeginTransaction())
            {
                DbManager.ExecuteNonQuery(Delete("projects_comments").Where("target_uniq_id", ProjectEntity.BuildUniqId<Message>(id)));
                DbManager.ExecuteNonQuery(new SqlDelete("projects_review_entity_info").Where("entity_uniqID", "Message_" + id));
                DbManager.ExecuteNonQuery(Delete(table).Where("id", id));

                tx.Commit();
            }
        }


        private SqlQuery CreateQuery()
        {
            return new SqlQuery(table + " t")
                .LeftOuterJoin(ProjectDao.PROJECT_TABLE + " p", Exp.EqColumns("t.project_id", "p.id"))
                .Select(ProjectDao.PROJECT_COLUMNS.Select(c => "p." + c).ToArray())
                .Select("t.id", "t.title", "t.create_by", "t.create_on", "t.last_modified_by", "t.last_modified_on")
                .Select("t.content")
                .Where("t.tenant_id", Tenant)
                .Where("p.tenant_id", Tenant);
        }

        private Message ToMessage(object[] r)
        {
            var offset = ProjectDao.PROJECT_COLUMNS.Length;
            return new Message()
            {
                Project = r[0] != null ? ProjectDao.ToProject(r) : null,
                ID = Convert.ToInt32(r[0 + offset]),
                Title = (string)r[1 + offset],
                CreateBy = ToGuid(r[2 + offset]),
                CreateOn = TenantUtil.DateTimeFromUtc((DateTime)r[3 + offset]),
                LastModifiedBy = ToGuid(r[4 + offset]),
                LastModifiedOn = TenantUtil.DateTimeFromUtc((DateTime)r[5 + offset]),
                Content = (string)r[6 + offset],
            };
        }
    }
}
