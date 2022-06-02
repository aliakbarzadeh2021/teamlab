
namespace ASC.Projects.Core.DataInterfaces
{
    public interface ITagDao
    {
        string[] GetTags();

        string[] GetTags(string prefix);


        int[] GetTagProjects(string tag);


        string[] GetProjectTags(int projectId);

        string[] GetProjectRequestTags(int requestId);

        void SetProjectTags(int projectId, string[] tags);

        void SetProjectRequestTags(int requestId, string[] tags);
    }
}
