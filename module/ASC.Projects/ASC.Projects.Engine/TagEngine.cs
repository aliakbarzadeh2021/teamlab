using System.Linq;
using ASC.Projects.Core.DataInterfaces;

namespace ASC.Projects.Engine
{
    public class TagEngine
    {
        private readonly ITagDao tagDao;


        public TagEngine(IDaoFactory daoFactory)
        {
            tagDao = daoFactory.GetTagDao();
        }


        public string[] GetTags()
        {
            return tagDao.GetTags().OrderBy(t => t).ToArray();
        }

        public string[] GetTags(string prefix)
        {
            return tagDao.GetTags(prefix).OrderBy(t => t).ToArray();
        }

        
        public int[] GetTagProjects(string tag)
        {
            return tagDao.GetTagProjects(tag);
        }

        
        public string[] GetProjectTags(int projectId)
        {
            return tagDao.GetProjectTags(projectId).OrderBy(t => t).ToArray();
        }

        public string[] GetProjectRequestTags(int requestId)
        {
            return tagDao.GetProjectRequestTags(requestId).OrderBy(t => t).ToArray();
        }

        public void SetProjectTags(int projectId, string tags)
        {
            tagDao.SetProjectTags(projectId, FromString(tags));
        }

        public void SetProjectRequestTags(int requestId, string tags)
        {
            tagDao.SetProjectRequestTags(requestId, FromString(tags));
        }

        private string[] FromString(string tags)
        {
            return (tags ?? string.Empty)
                .Split(',', ';')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToArray();
        }
    }
}
