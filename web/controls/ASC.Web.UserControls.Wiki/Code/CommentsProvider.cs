using System;
using System.Collections.Generic;
using System.Web;
using ASC.Web.UserControls.Wiki.Data;


namespace ASC.Web.UserControls.Wiki
{
    public class CommentsProvider : Provider
    {


        public static WikiComments RemoveComment(Guid commentID, int tenantId)
        {
            WikiComments comment = GetCommentById(commentID, tenantId);
            if(comment != null)
            {
                comment.Inactive = true;
                SaveComment(comment, tenantId);
            }

            return comment;
        }

        public static WikiComments GetCommentById(Guid commentID, int tenantId)
        {
            using(WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.CommentsGetById(commentID);
            }
        }

        public static WikiComments SaveComment(WikiComments comment, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.CommentsSave(comment);
            }
        }

        public static List<WikiComments> GetAllComments(string pageName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.CommentsGetAllByPageName(pageName);
            }
        }

        public static int GetAllCommentsCount(string pageName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.CommentsGetCountAllByPageName(pageName);
            }
        }


        public static List<WikiComments> GetTopCommentsByPageName(string pageName, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.CommentsGetTopByPageName(pageName);
            }
        }

        public static List<WikiComments> GetTopCommentsByParentId(Guid ParentId, int tenantId)
        {
            using (WikiDAO wikiDAO = new WikiDAO())
            {
                InitWikiDao(wikiDAO, tenantId);
                return wikiDAO.CommentsGetTopByParentId(ParentId);
            }
        }

        
    }
}
