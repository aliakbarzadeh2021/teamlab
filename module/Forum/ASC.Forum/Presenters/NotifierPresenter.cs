﻿using System;
using ASC.Forum.Module;
using ASC.Notify.Patterns;

namespace ASC.Forum
{
    internal class NotifierPresenter : PresenterTemplate<INotifierView>
    {
        protected override void RegisterView()
        {
            _view.SendNotify += new EventHandler<NotifyEventArgs>(SendNotifyHandler);
        }

        private void SendNotifyHandler(object sender, NotifyEventArgs e)
        {
            
            if (String.Equals(e.NotifyAction.ID, Constants.NewPostInTopic.ID, StringComparison.InvariantCultureIgnoreCase))
            {
                
                ForumNotifyClient.NotifyClient.SendNoticeAsync(Constants.NewPostInTopic, e.ObjectID, null,
                                                                new TagValue(Constants.TagDate, e.Date),
                                                                new TagValue(Constants.TagThreadTitle, e.ThreadTitle),
                                                                new TagValue(Constants.TagTopicTitle, e.TopicTitle),
                                                                new TagValue(Constants.TagTopicURL, e.TopicURL),
                                                                new TagValue(Constants.TagPostURL, e.PostURL),
                                                                new TagValue(Constants.TagThreadURL, e.ThreadURL),
                                                                new TagValue(Constants.TagPostText, e.PostText),
                                                                new TagValue(Constants.TagUserURL, e.UserURL),
                                                                new TagValue(Constants.TagUserName, e.Poster.ToString()));


            }


            else if (String.Equals(e.NotifyAction.ID, Constants.NewPostInThread.ID, StringComparison.InvariantCultureIgnoreCase))
            {   
                ForumNotifyClient.NotifyClient.SendNoticeAsync(Constants.NewPostInThread, e.ObjectID, null,
                                                                new TagValue(Constants.TagDate, e.Date),
                                                                new TagValue(Constants.TagThreadTitle, e.ThreadTitle),
                                                                new TagValue(Constants.TagTopicTitle, e.TopicTitle),
                                                                new TagValue(Constants.TagTopicURL, e.TopicURL),
                                                                new TagValue(Constants.TagPostURL, e.PostURL),
                                                                new TagValue(Constants.TagThreadURL, e.ThreadURL),
                                                                new TagValue(Constants.TagPostText, e.PostText),
                                                                new TagValue(Constants.TagUserURL, e.UserURL),
                                                                new TagValue(Constants.TagUserName, e.Poster.ToString()));


            }

            else if (String.Equals(e.NotifyAction.ID, Constants.NewPostByTag.ID, StringComparison.InvariantCultureIgnoreCase))
            {   
                ForumNotifyClient.NotifyClient.SendNoticeAsync(Constants.NewPostByTag, e.ObjectID, null,
                                                               new TagValue(Constants.TagDate, e.Date),
                                                                new TagValue(Constants.TagThreadTitle, e.ThreadTitle),
                                                                new TagValue(Constants.TagTopicTitle, e.TopicTitle),
                                                                new TagValue(Constants.TagTopicURL, e.TopicURL),
                                                                new TagValue(Constants.TagPostURL, e.PostURL),
                                                                new TagValue(Constants.TagThreadURL, e.ThreadURL),
                                                                new TagValue(Constants.TagPostText, e.PostText),
                                                                new TagValue(Constants.TagUserURL, e.UserURL),
                                                                new TagValue(Constants.TagUserName, e.Poster.ToString()));

            }

            else if (String.Equals(e.NotifyAction.ID, Constants.NewTopicInForum.ID, StringComparison.InvariantCultureIgnoreCase))
            {   
                ForumNotifyClient.NotifyClient.SendNoticeAsync(Constants.NewTopicInForum, e.ObjectID, null,
                                                                new TagValue(Constants.TagDate, e.Date),
                                                                new TagValue(Constants.TagThreadTitle, e.ThreadTitle),
                                                                new TagValue(Constants.TagTopicTitle, e.TopicTitle),
                                                                new TagValue(Constants.TagTopicURL, e.TopicURL),
                                                                new TagValue(Constants.TagPostURL, e.PostURL),
                                                                new TagValue(Constants.TagThreadURL, e.ThreadURL),
                                                                new TagValue(Constants.TagPostText, e.PostText),
                                                                new TagValue(Constants.TagTagName, e.TagName),
                                                                new TagValue(Constants.TagTagURL, e.TagURL),
                                                                new TagValue(Constants.TagUserURL, e.UserURL),
                                                                new TagValue(Constants.TagUserName, e.Poster.ToString()));


            }
        }
    }
}
