using System;
using System.Collections.Generic;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Core.DataInterfaces
{
    public interface IMilestoneDao
    {
        List<Milestone> GetByProject(int projectId);

        List<Milestone> GetUpcomingMilestones(int offset, int max, params int[] projects);

        List<Milestone> GetLateMilestones(int offset, int max, params int[] projects);

        List<Milestone> GetByDeadLine(DateTime deadline);

        List<object[]> GetInfoForReminder(DateTime deadline);

        
        Milestone GetById(int id);

        bool IsExists(int id);

        Milestone Save(Milestone milestone);

        void Delete(int id);

        bool CanReadMilestones(int projectId, Guid userId);
    }
}
