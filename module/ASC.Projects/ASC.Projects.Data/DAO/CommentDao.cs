using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Common.Data.Sql.Expressions;

namespace ASC.Projects.Data.DAO
{
    class CommentDao : BaseDao, ICommentDao
    {
        private readonly string[] columns = new[] { "id", "target_uniq_id", "content", "inactive", "create_by", "create_on", "parent_id" };


        public CommentDao(string dbId, int tenantID)
            : base(dbId, tenantID)
        {
        }


        public List<Comment> GetAll(DomainObject<int> target)
        {
            return DbManager
                .ExecuteList(
                    Query("projects_comments")
                    .Select(columns)
                    .Where("target_uniq_id", target.UniqID)
                    .OrderBy("create_on", true))
                .ConvertAll(r => ToComment(r));
        }

        public Comment GetById(Guid id)
        {
            return DbManager
                .ExecuteList(Query("projects_comments").Select(columns).Where("id", id.ToString()))
                .ConvertAll(r => ToComment(r))
                .SingleOrDefault();
        }

        public Comment GetLast(DomainObject<Int32> target)
        {
            return DbManager
                .ExecuteList(
                    Query("projects_comments")
                    .Select(columns)
                    .Where("target_uniq_id", target.UniqID)
                    .Where("inactive", false)
                    .OrderBy("create_on", false)
                    .SetMaxResults(1))
                .ConvertAll(r => ToComment(r))
                .SingleOrDefault();
        }

        public List<int> GetCommentsCount(List<ProjectEntity> targets)
        {
            var pairs =
                DbManager
                    .ExecuteList(
                        Query("projects_comments")
                        .Select("target_uniq_id", "count(*)")
                        .Where(Exp.In("target_uniq_id", targets.ConvertAll(target => target.UniqID)))
                        .Where("inactive", false)
                        .GroupBy("target_uniq_id")
                    ).ConvertAll(r => new object[] { Convert.ToString(r[0]), Convert.ToInt32(r[1]) });

            return targets.ConvertAll(
                        target =>
                        {
                            var pair = pairs.Find(p => String.Equals(Convert.ToString(p[0]), target.UniqID));
                            if (pair == null)
                                return 0;
                            else
                                return Convert.ToInt32(pair[1]);

                        });
        }



        public int Count(DomainObject<Int32> target)
        {
            return DbManager.ExecuteScalar<int>(
                Query("projects_comments")
                .SelectCount()
                .Where("target_uniq_id", target.UniqID)
                .Where("inactive", false));
        }


        public Comment Save(Comment comment)
        {
            if (comment.ID == default(Guid)) comment.ID = Guid.NewGuid();

            var insert = Insert("projects_comments")
                .InColumns(columns)
                .Values(
                    comment.ID,
                    comment.TargetUniqID,
                    comment.Content,
                    comment.Inactive,
                    comment.CreateBy.ToString(),
                    TenantUtil.DateTimeToUtc(comment.CreateOn),
                    comment.Parent.ToString());
            DbManager.ExecuteNonQuery(insert);
            return comment;
        }

        public void Delete(Guid id)
        {
            DbManager.ExecuteNonQuery(Delete("projects_comments").Where("id", id.ToString()));
        }


        private Comment ToComment(object[] r)
        {
            return new Comment()
            {
                ID = ToGuid(r[0]),
                TargetUniqID = (string)r[1],
                Content = (string)r[2],
                Inactive = Convert.ToBoolean(r[3]),
                CreateBy = ToGuid(r[4]),
                CreateOn = TenantUtil.DateTimeFromUtc((DateTime)r[5]),
                Parent = ToGuid(r[6]),
            };
        }
    }
}
