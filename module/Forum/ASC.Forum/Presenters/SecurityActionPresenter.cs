using System;
using ASC.Common.Security;
using ASC.Core;
using ASC.Forum.Module;

namespace ASC.Forum
{   

    internal class SecurityActionPresenter : PresenterTemplate<ISecurityActionView>
    {
        protected override void RegisterView()
        {
            _view.ValidateAccess+=new EventHandler<SecurityAccessEventArgs>(ValidateAccessHandler);
        }

        private void ValidateAccessHandler(object sender, SecurityAccessEventArgs e)
        {
            ISecurityObject securityObject = null;
            if (e.TargetObject is ISecurityObject)
                securityObject = (ISecurityObject)e.TargetObject;

            switch (e.Action)
            {
                case ForumAction.ReadPosts:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.ReadPostsAction);
                    break;

                case ForumAction.PostCreate:
                    
                    Topic topic = (Topic)e.TargetObject;    
                    if (SecurityContext.CheckPermissions(topic, Constants.PostCreateAction))
                    {   
                        if(!topic.Closed)
                            _view.IsAccessible = true;

                        else if (topic.Closed && SecurityContext.CheckPermissions(topic, Constants.TopicCloseAction))
                            _view.IsAccessible = true;

                        else
                            _view.IsAccessible = false;
                    }
                    else
                        _view.IsAccessible = false;

                    break;
                    
                case ForumAction.ApprovePost:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.PostApproveAction);
                    break;

                case ForumAction.PostEdit:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.PostEditAction);
                    break;

                case ForumAction.PostDelete:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.PostDeleteAction);
                    break;

                case ForumAction.TopicCreate:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.TopicCreateAction);
                    break;

                case ForumAction.PollCreate:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.PollCreateAction);
                    break;

                case ForumAction.TopicClose:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.TopicCloseAction);
                    break;

                case ForumAction.TopicSticky:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.TopicStickyAction);
                    break;

                case ForumAction.TopicEdit:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.TopicEditAction);
                    break;

                case ForumAction.TopicDelete:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.TopicDeleteAction);
                    break;

                case ForumAction.PollVote:

                    Question question = (Question)e.TargetObject;
                    _view.IsAccessible = SecurityContext.CheckPermissions(new Topic() { ID = question.TopicID}, Constants.PollVoteAction);
                    
                    //if (SecurityContext.CheckPermissions(new Topic() { ID = question.TopicID}, Constants.PollVoteAction))
                    //{

                    //    if (!question.TopicPoll.Closed)
                    //        _view.IsAccessible = !CheckUserPollAnswer(question);

                    //    else if (question.TopicPoll.Closed && SecurityContext.CheckPermissions(question.TopicPoll, Constants.TopicCloseAction))
                    //        _view.IsAccessible = !CheckUserPollAnswer(question);

                    //    else
                    //        _view.IsAccessible = false;

                    //}
                    //else
                    //    _view.IsAccessible = false;

                    break;


                case ForumAction.TagCreate:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.TagCreateAction);
                    break;
                
                case ForumAction.AttachmentCreate:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.AttachmentCreateAction);
                    break;

                case ForumAction.AttachmentDelete:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.AttachmentDeleteAction);
                    break;
               
                case ForumAction.GetAccessForumEditor:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.ForumManagementAction);
                    break;

                case ForumAction.GetAccessTagEditor:
                    _view.IsAccessible = SecurityContext.CheckPermissions(securityObject, Constants.TagManagementAction);
                    break;
            }
        }
    }
}
