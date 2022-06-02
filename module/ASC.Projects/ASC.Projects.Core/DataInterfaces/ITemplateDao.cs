using System.Collections.Generic;
using ASC.Projects.Core.Domain;

namespace ASC.Projects.Core.DataInterfaces
{
    public interface ITemplateDao
    {
        List<TemplateProject> GetTemplateProjects();

        List<TemplateMilestone> GetTemplateMilestones(int projectId);

        List<TemplateMessage> GetTemplateMessages(int projectId);

        List<TemplateTask> GetTemplateTasks(int projectId);

        
        TemplateProject GetTemplateProject(int id);

        TemplateMilestone GetTemplateMilestone(int id);

        TemplateMessage GetTemplateMessage(int id);

        TemplateTask GetTemplateTask(int id);


        TemplateProject SaveTemplateProject(TemplateProject template);

        TemplateMilestone SaveTemplateMilestone(TemplateMilestone template);

        TemplateMessage SaveTemplateMessage(TemplateMessage template);

        TemplateTask SaveTemplateTask(TemplateTask template);


        void RemoveTemplateProject(int id);

        void RemoveTemplateMilestone(int id);

        void RemoveTemplateMessage(int id);

        void RemoveTemplateTask(int id);

		void SetTaskOrders(int? milestoneId, int taskID, int? prevTaskID, int? nextTaskID);
	}
}
