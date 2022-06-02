﻿using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.FullTextIndex;
using ASC.Notify;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Web.Community.News.Code.Module;
using ASC.Web.Controls;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Community.News.Code.DAO
{
    class DbFeedStorage : IFeedStorage
    {
        private DbManager dbManager = new DbManager(FeedStorageFactory.Id);

        private int tenant;

        private const int MinSearchLength = 3;


        public DbFeedStorage(int tenant)
        {
            this.tenant = tenant;
        }

        public List<FeedType> GetUsedFeedTypes()
        {
            return dbManager
                .ExecuteList(Query("events_feed").Select("FeedType").GroupBy(1))
                .ConvertAll<FeedType>(row => (FeedType)Convert.ToInt32(row[0]))
                .OrderBy(f => f)
                .ToList();
        }

        public List<Feed> GetFeeds(FeedType feedType, Guid userId, int count, int offset)
        {
            return SearchFeeds(null, feedType, userId, count, offset);
        }

        public long GetFeedsCount(FeedType feedType, Guid userId)
        {
            return SearchFeedsCount(null, feedType, userId);
        }

        public List<Feed> SearchFeeds(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length < MinSearchLength)
            {
                return new List<Feed>();
            }

            return dbManager
                .ExecuteList(Query("events_feed").Select("Id", "Caption", "Text").Where(GetWhere(s, FeedType.All, Guid.Empty)))
                .ConvertAll(r => Mappers.ToFeedFinded(r));
        }

        public long SearchFeedsCount(string s, FeedType feedType, Guid userId)
        {
            if (s != null && s.Length < MinSearchLength)
            {
                return 0;
            }

            return dbManager.ExecuteScalar<long>(Query("events_feed").SelectCount().Where(GetWhere(s, feedType, userId)));
        }

        public List<Feed> SearchFeeds(string s, FeedType feedType, Guid userId, int count, int offset)
        {
            if (s != null && s.Length < MinSearchLength)
            {
                return new List<Feed>();
            }

            var select = Query("events_feed")
                .Select("Id", "FeedType", "Caption", "Date", "Creator")
                .Select(GetFeedReadedQuery())
                .Where(GetWhere(s, feedType, userId))
                .OrderBy("Id", false)
                .SetFirstResult(offset)
                .SetMaxResults(count);

            return dbManager
                .ExecuteList(select)
                .ConvertAll(r => Mappers.ToFeed(r));
        }

        private Exp GetWhere(string s, FeedType feedType, Guid userId)
        {
            var where = Exp.Empty;

            if (!string.IsNullOrEmpty(s))
            {
                if (FullTextSearch.SupportModule(FullTextSearch.NewsModule))
                {
                    var ids = FullTextSearch
                        .Search(s, FullTextSearch.NewsModule)
                        .GetIdentifiers()
                        .Select(id => int.Parse(id))
                        .ToList();
                    where = where & Exp.In("Id", ids);
                }
                else
                {
                    where = where & (Exp.Like("lower(Caption)", s.ToLower()) | Exp.Like("lower(Text)", s.ToLower()));
                }
            }
            if (feedType == FeedType.AllNews)
            {
                where = where & !Exp.Eq("FeedType", FeedType.Poll);
            }
            else if (feedType != FeedType.All)
            {
                where = where & Exp.Eq("FeedType", feedType);
            }
            if (userId != Guid.Empty)
            {
                where = where & Exp.Eq("Creator", userId.ToString());
            }
            return where;
        }

        private SqlQuery GetFeedReadedQuery()
        {
            return Query("events_reader").SelectCount().Where(Exp.EqColumns("Feed", "Id") & Exp.Eq("Reader", SecurityContext.CurrentAccount.ID.ToString()));
        }

        public Feed GetFeed(long id)
        {
            using (var tx = dbManager.BeginTransaction())
            {
                var list = dbManager
                    .ExecuteList(Query("events_feed").Select("Id", "FeedType", "Caption", "Text", "Date", "Creator").Select(GetFeedReadedQuery()).Where("Id", id))
                    .ConvertAll(r => Mappers.ToNewsOrPoll(r));

                var feed = 0 < list.Count ? list[0] : null;
                if (feed == null || feed is FeedNews) return feed;

                var poll = (FeedPoll)feed;
                dbManager.ExecuteList(Query("events_poll").Select("PollType", "StartDate", "EndDate").Where("Id", id))
                    .ForEach(row =>
                    {
                        poll.PollType = (FeedPollType)Convert.ToInt32(row[0]);
                        poll.StartDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[1]));
                        poll.EndDate = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(row[2]));
                    });

                dbManager.ExecuteList(
                    Query("events_pollvariant v").Select("v.Id", "v.Name", "a.User")
                        .LeftOuterJoin("events_pollanswer a", Exp.EqColumns("v.Id", "a.Variant"))
                        .Where("v.Poll", id))
                    .ForEach(row =>
                    {
                        var variantId = Convert.ToInt64(row[0]);
                        if (!poll.Variants.Exists(v => v.ID == variantId))
                        {
                            poll.Variants.Add(new FeedPollVariant() { ID = variantId, Name = Convert.ToString(row[1]) });
                        }
                        if (row[2] != null)
                        {
                            poll.answers.Add(new FeedPollAnswer(variantId, Convert.ToString(row[2])));
                        }
                    });
                tx.Commit();

                return poll;
            }
        }

        public Feed SaveFeed(Feed feed, bool isEdit, FeedType type)
        {
            if (feed == null) throw new ArgumentNullException("feed");

            using (var tx = dbManager.BeginTransaction())
            {
                feed.Id = dbManager.ExecuteScalar<long>(
                    Insert("events_feed").InColumns("Id", "FeedType", "Caption", "Text", "Date", "Creator")
                    .Values(feed.Id, feed.FeedType, feed.Caption, feed.Text, TenantUtil.DateTimeToUtc(feed.Date), feed.Creator)
                    .Identity(1, 0L, true)
                );

                if (feed is FeedPoll)
                {
                    var poll = (FeedPoll)feed;

                    dbManager.ExecuteNonQuery(Insert("events_poll").InColumns("Id", "PollType", "StartDate", "EndDate")
                        .Values(poll.Id, poll.PollType, TenantUtil.DateTimeToUtc(poll.StartDate), TenantUtil.DateTimeToUtc(poll.EndDate)));

                    dbManager.ExecuteNonQuery(Delete("events_pollvariant").Where("Poll", poll.Id));
                    foreach (var variant in poll.Variants)
                    {
                        variant.ID = dbManager.ExecuteScalar<long>(
                            Insert("events_pollvariant").InColumns("Id", "Name", "Poll")
                            .Values(variant.ID, variant.Name, poll.Id)
                            .Identity(1, 0L, true));
                    }
                }
                tx.Commit();
            }


            NotifyFeed(feed, isEdit, type);

            return feed;
        }

        private void NotifyFeed(Feed feed, bool isEdit, FeedType type)
        {
            if (isEdit)
            {
                ActivityPublisher.EditPost(feed, SecurityContext.CurrentAccount.ID);
            }
            else
            {

                var initatorInterceptor = new InitiatorInterceptor(new DirectRecipient(feed.Creator, ""));
                try
                {
                    NewsNotifyClient.NotifyClient.AddInterceptor(initatorInterceptor);

                    if (type == FeedType.Poll && feed is FeedPoll)
                    {
                        NewsNotifyClient.NotifyClient.SendNoticeAsync(
                                NewsConst.NewFeed, null, null,
                                new TagValue(NewsConst.TagFEED_TYPE, "poll"),
                                new TagValue(NewsConst.TagAnswers, ((FeedPoll)feed).Variants.ConvertAll<string>(v => v.Name)),
                                new TagValue(NewsConst.TagCaption, feed.Caption),
                                new TagValue(NewsConst.TagText, HtmlUtility.GetFull(feed.Text, ASC.Web.Community.Product.CommunityProduct.ID)),
                                new TagValue(NewsConst.TagDate, feed.Date.ToShortString()),
                                new TagValue(NewsConst.TagURL, CommonLinkUtility.GetFullAbsolutePath("~/products/community/modules/news/?docid=" + feed.Id)),
                                new TagValue(NewsConst.TagUserName, DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID)),
                                new TagValue(NewsConst.TagUserUrl, CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetUserProfile(SecurityContext.CurrentAccount.ID, ASC.Web.Community.Product.CommunityProduct.ID)))
                            );
                    }
                    else
                    {
                        NewsNotifyClient.NotifyClient.SendNoticeAsync(
                            NewsConst.NewFeed, null, null,
                            new TagValue(NewsConst.TagFEED_TYPE, "feed"),
                            new TagValue(NewsConst.TagCaption, feed.Caption),
                            new TagValue(NewsConst.TagText,
                                         HtmlUtility.GetFull(feed.Text, ASC.Web.Community.Product.CommunityProduct.ID)),
                            new TagValue(NewsConst.TagDate, feed.Date.ToShortString()),
                            new TagValue(NewsConst.TagURL,
                                         CommonLinkUtility.GetFullAbsolutePath(
                                             "~/products/community/modules/news/?docid=" + feed.Id)),
                            new TagValue(NewsConst.TagUserName,
                                         DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID)),
                            new TagValue(NewsConst.TagUserUrl,
                                         CommonLinkUtility.GetFullAbsolutePath(
                                             CommonLinkUtility.GetUserProfile(SecurityContext.CurrentAccount.ID,
                                                                              ASC.Web.Community.Product.CommunityProduct.ID)))
                            );
                    }
                    ActivityPublisher.AddFeed(feed);
                }
                finally
                {
                    NewsNotifyClient.NotifyClient.RemoveInterceptor(initatorInterceptor.Name);

                }
            }
        }

        public void ReadFeed(long feedId, string reader)
        {
            dbManager.ExecuteNonQuery(Insert("events_reader").InColumns("Feed", "Reader").Values(feedId, reader));
        }

        public void PollVote(string userId, ICollection<long> variantIds)
        {
            using (var tx = dbManager.BeginTransaction())
            {
                foreach (var variantId in variantIds)
                {
                    if (variantId == 0) continue;
                    dbManager.ExecuteNonQuery(Insert("events_pollanswer").InColumns("Variant", "User").Values(variantId, userId));
                }
                tx.Commit();
            }
        }

        public void RemoveFeed(long id)
        {
            using (var tx = dbManager.BeginTransaction())
            {
                dbManager.ExecuteNonQuery(Delete("events_pollanswer").Where(Exp.In("Variant", Query("events_pollvariant").Select("Id").Where(Exp.Eq("Poll", id)))));
                dbManager.ExecuteNonQuery(Delete("events_pollvariant").Where("Poll", id));
                dbManager.ExecuteNonQuery(Delete("events_poll").Where("Id", id));
                dbManager.ExecuteNonQuery(Delete("events_comment").Where("Feed", id));
                dbManager.ExecuteNonQuery(Delete("events_reader").Where("Feed", id));
                dbManager.ExecuteNonQuery(Delete("events_feed").Where("Id", id));
                tx.Commit();
            }
        }

        public List<FeedComment> GetFeedComments(long feedId)
        {
            return dbManager
                .ExecuteList(GetFeedCommentQuery(Exp.Eq("Feed", feedId)))
                .ConvertAll(r => Mappers.ToFeedComment(r));
        }

        public FeedComment GetFeedComment(long commentId)
        {
            var list = dbManager
                .ExecuteList(GetFeedCommentQuery(Exp.Eq("Id", commentId)))
                .ConvertAll(r => Mappers.ToFeedComment(r));
            return 0 < list.Count ? list[0] : null;
        }

        private SqlQuery GetFeedCommentQuery(Exp where)
        {
            return Query("events_comment").Select("Id", "Feed", "Comment", "Parent", "Date", "Creator", "Inactive").Where(where);
        }

        public FeedComment SaveFeedComment(Feed feed, FeedComment comment)
        {
            SaveFeedComment(comment);
            if (feed != null)
            {
                NotifyNewComment(comment, feed);
            }
            return comment;
        }

        public void RemoveFeedComment(FeedComment comment)
        {
            SaveFeedComment(comment);
            ActivityPublisher.DeleteFeedComment(GetFeed(comment.FeedId), SecurityContext.CurrentAccount.ID);
        }

        public void UpdateFeedComment(FeedComment comment)
        {
            SaveFeedComment(comment);
            ActivityPublisher.EditFeedComment(GetFeed(comment.FeedId), SecurityContext.CurrentAccount.ID);
        }

        private void SaveFeedComment(FeedComment comment)
        {
            if (comment == null) throw new ArgumentNullException("comment");

            comment.Id = dbManager.ExecuteScalar<long>(
                Insert("events_comment").InColumns("Id", "Feed", "Comment", "Parent", "Date", "Creator", "Inactive")
                    .Values(comment.Id, comment.FeedId, comment.Comment, comment.ParentId, TenantUtil.DateTimeToUtc(comment.Date), comment.Creator, comment.Inactive)
                    .Identity(1, 0L, true)
                );
        }

        private void NotifyNewComment(FeedComment comment, Feed feed)
        {
            ActivityPublisher.AddFeedComment(comment, feed, SecurityContext.CurrentAccount.ID);
            var feedType = feed.FeedType == FeedType.Poll ? "poll" : "feed";

            var initatorInterceptor = new InitiatorInterceptor(new DirectRecipient(comment.Creator, ""));
            try
            {
                NewsNotifyClient.NotifyClient.AddInterceptor(initatorInterceptor);
                NewsNotifyClient.NotifyClient.SendNoticeAsync(
                    NewsConst.NewComment, feed.Id.ToString(),
                    null,
                    new TagValue(NewsConst.TagFEED_TYPE, feedType),
                    //new TagValue(NewsConst.TagAnswers, feed.Variants.ConvertAll<string>(v => v.Name)),
                    new TagValue(NewsConst.TagCaption, feed.Caption),
                    new TagValue("CommentBody", HtmlUtility.GetFull(comment.Comment, ASC.Web.Community.Product.CommunityProduct.ID)),
                    new TagValue(NewsConst.TagDate, comment.Date.ToShortString()),
                    new TagValue(NewsConst.TagURL, CommonLinkUtility.GetFullAbsolutePath("~/products/community/modules/news/?docid=" + feed.Id)),
                    new TagValue("CommentURL", CommonLinkUtility.GetFullAbsolutePath("~/products/community/modules/news/?docid=" + feed.Id + "#" + comment.Id.ToString())),
                    new TagValue(NewsConst.TagUserName, DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID)),
                    new TagValue(NewsConst.TagUserUrl, CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetUserProfile(SecurityContext.CurrentAccount.ID, ASC.Web.Community.Product.CommunityProduct.ID)))
                    );
            }
            finally
            {
                NewsNotifyClient.NotifyClient.RemoveInterceptor(initatorInterceptor.Name);
            }
        }

        public void RemoveFeedComment(long commentId)
        {
            dbManager.ExecuteNonQuery(Delete("events_comment").Where("Id", commentId));
        }

        public void Dispose()
        {
            dbManager.Dispose();
        }


        private SqlQuery Query(string table)
        {
            return new SqlQuery(table).Where(GetTenantColumnName(table), tenant);
        }

        private SqlDelete Delete(string table)
        {
            return new SqlDelete(table).Where(GetTenantColumnName(table), tenant);
        }

        private SqlInsert Insert(string table)
        {
            return new SqlInsert(table, true).InColumns(GetTenantColumnName(table)).Values(tenant);
        }

        private string GetTenantColumnName(string table)
        {
            var tenant = "Tenant";
            if (!table.Contains(" ")) return tenant;
            return table.Substring(table.IndexOf(" ")).Trim() + "." + tenant;
        }
    }
}