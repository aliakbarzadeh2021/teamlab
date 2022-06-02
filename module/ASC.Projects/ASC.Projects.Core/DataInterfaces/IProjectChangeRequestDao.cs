using System.Collections.Generic;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Core.DataInterfaces
{
    public interface IProjectChangeRequestDao
    {
        List<ProjectChangeRequest> GetAll();

        ProjectChangeRequest GetById(int id);

        ProjectChangeRequest Save(ProjectChangeRequest request);

        void Delete(int id);
    }
}
