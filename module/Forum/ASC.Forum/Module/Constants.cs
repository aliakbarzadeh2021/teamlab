﻿using System;
using ASC.Common.Security.Authorizing;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using Action = ASC.Common.Security.Authorizing.Action;

namespace ASC.Forum.Module
{
    public sealed class Constants
    {
        public static readonly Action TopicCreateAction = new Action(
                                                        new Guid("{49AE8915-2B30-4348-AB74-B152279364FB}"),
                                                        "Add topic"
                                                    );

        public static readonly Action PollCreateAction = new Action(
                                                        new Guid("{13E30B51-5B4D-40a5-8575-CB561899EEB1}"),
                                                        "Add poll"
                                                    );

        public static readonly Action TopicStickyAction = new Action(
                                                        new Guid("{6D50336A-0418-41c0-AF56-282F8AF39C59}"),
                                                        "Attach/Detach topic"
                                                    );
        public static readonly Action TopicCloseAction = new Action(
                                                       new Guid("{C333B9B9-EE06-4b11-8ED0-322B8D2ADCB6}"),
                                                       "Open/Close topic"
                                                   );
        public static readonly Action TopicEditAction = new Action(
                                                       new Guid("{43C7CB5E-38D0-495d-ABD7-E50CFEEA7DD2}"),
                                                       "Edit topic"
                                                   );
        public static readonly Action TopicDeleteAction = new Action(
                                                       new Guid("{EA5CC1C4-AFE1-42d1-94C7-6A45154FC2B7}"),
                                                       "Remove topic"
                                                   );
        public static readonly Action PollVoteAction = new Action(
                                                       new Guid("{E37239BD-C5B5-4f1e-A9F8-3CEEAC209615}"),
                                                       "Vote"
                                                   );
        public static readonly Action PostCreateAction = new Action(
                                                       new Guid("{63E9F35F-6BB5-4fb1-AFAA-E4C2F4DEC9BD}"),
                                                       "Add post"
                                                   );
        public static readonly Action ReadPostsAction = new Action(
                                                       new Guid("{E0759A42-47F0-4763-A26A-D5AA665BEC35}"),
                                                       "Read post"
                                                   );

        public static readonly Action PostEditAction = new Action(
                                                       new Guid("{D7CDB020-288B-41e5-A857-597347618533}"),
                                                       "Edit post"
                                                   );
        public static readonly Action PostDeleteAction = new Action(
                                                       new Guid("{662F3DB7-9BC8-42cf-84DA-2765F563E9B0}"),
                                                       "Remove post"
                                                   );
        public static readonly Action PostApproveAction = new Action(
                                                       new Guid("{D11EBCB9-0E6E-45e6-A6D0-99C41D687598}"),
                                                       "Confirm post"
                                                   );
        public static readonly Action TagCreateAction = new Action(
                                                      new Guid("{9018C001-24C2-44bf-A1DB-D1121A570E74}"),
                                                      "Add tag"
                                                  );
        public static readonly Action AttachmentCreateAction = new Action(
                                                      new Guid("{D1F3B53D-D9E2-4259-80E7-D24380978395}"),
                                                      "Attach file"
                                                  );
        public static readonly Action AttachmentDeleteAction = new Action(
                                                      new Guid("{C62A9E8D-B24C-4513-90AA-7FF0F8BA38EB}"),
                                                      "Remove attached file"
                                                  );

        public static readonly Action ForumManagementAction = new Action(
                                                      new Guid("{73E324F2-F2A6-4548-BF3F-5884F3713E9C}"),
                                                      "Forum administration"
                                                  );

        /// <summary>
        /// admin
        /// </summary>
        public static readonly Action TagManagementAction = new Action(
                                                      new Guid("{1F3BB856-CDF3-479b-B9B9-651871F25105}"),
                                                      "Manage tags"
                                                  );
        public static readonly AuthCategory BaseAuthCategory = new AuthCategory(
                                                        "Basic",
                                                        new[]{
                                                            ReadPostsAction,
                                                            TopicCreateAction,
                                                            PollCreateAction,
                                                            PollVoteAction,
                                                            PostCreateAction,
                                                            AttachmentCreateAction,
                                                            TagCreateAction
                                                        }
                                                    );
        public static readonly AuthCategory ModeratorAuthCategory = new AuthCategory(
                                                        "Moderation",
                                                        new[]{
                                                           TopicStickyAction,
                                                           TopicCloseAction,
                                                           TopicEditAction,
                                                           TopicDeleteAction,
                                                           PostEditAction,
                                                           PostDeleteAction,
                                                           PostApproveAction,
                                                           AttachmentDeleteAction
                                                        }
                                                    );
        public static readonly AuthCategory AdminAuthCategory = new AuthCategory(
                                                        "Administration",
                                                        new[]{
                                                          ForumManagementAction,
                                                          TagManagementAction
                                                        }
                                                    );
        public static readonly AuthCategory[] AuthorizingCategories = new[]{
                                        BaseAuthCategory,
                                        ModeratorAuthCategory,
                                        AdminAuthCategory
                                    };


        public static INotifyAction NewPostInTopic = new NotifyAction("new post in topic", "new post in topic");

        public static INotifyAction NewPostInThread = new NotifyAction("new post in thread", "new post in thread");

        public static INotifyAction NewPostByTag = new NotifyAction("new post by tag", "new post by tag");

        public static INotifyAction NewTopicInForum = new NotifyAction("new topic in forum", "new topic in forum");

        public static ITag TagTopicTitle = new ASC.Notify.Patterns.Tag("TopicTitle");

        public static ITag TagThreadTitle = new ASC.Notify.Patterns.Tag("ThreadTitle");

        public static ITag TagTopicURL = new ASC.Notify.Patterns.Tag("TopicURL");

        public static ITag TagPostURL = new ASC.Notify.Patterns.Tag("PostURL");

        public static ITag TagThreadURL = new ASC.Notify.Patterns.Tag("ThreadURL");

        public static ITag TagPostText = new ASC.Notify.Patterns.Tag("PostText");

        public static ITag TagUserName = new ASC.Notify.Patterns.Tag("UserName");

        public static ITag TagTagName = new ASC.Notify.Patterns.Tag("TagName");

        public static ITag TagTagURL = new ASC.Notify.Patterns.Tag("TagURL");

        public static ITag TagDate = new ASC.Notify.Patterns.Tag("Date");

        public static ITag TagUserURL = new ASC.Notify.Patterns.Tag("UserURL");
    }
}
