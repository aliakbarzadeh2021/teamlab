using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;

namespace ASC.Projects.Engine
{
    public class TemplateEngine
    {
        private readonly EngineFactory _factory;
        private readonly ITemplateDao dao;



		public TemplateEngine(IDaoFactory daoFactory, EngineFactory factory)
		{
		    _factory = factory;
		    dao = daoFactory.GetTemplateDao();			
		}

        public List<TemplateProject> GetTemplateProjects()
        {
            return dao.GetTemplateProjects();
        }

        public List<TemplateMilestone> GetTemplateMilestones(int projectId)
        {
            return dao.GetTemplateMilestones(projectId);
        }

        public List<TemplateMessage> GetTemplateMessages(int projectId)
        {
            return dao.GetTemplateMessages(projectId);
        }

        public List<TemplateTask> GetTemplateTasks(int projectId)
        {
            return dao.GetTemplateTasks(projectId);
        }

        public TemplateProject GetTemplateProject(int id)
        {
            return dao.GetTemplateProject(id);
        }

        public TemplateMilestone GetTemplateMilestone(int id)
        {
            return dao.GetTemplateMilestone(id);
        }

        public TemplateMessage GetTemplateMessage(int id)
        {
            return dao.GetTemplateMessage(id);
        }

        public TemplateTask GetTemplateTask(int id)
        {
            return dao.GetTemplateTask(id);
        }

        public TemplateProject SaveTemplateProject(TemplateProject template)
        {
            SetCreateValues(template);
            return dao.SaveTemplateProject(template);
        }

        public TemplateMilestone SaveTemplateMilestone(TemplateMilestone template)
        {
            SetCreateValues(template);
            var milestone=  dao.SaveTemplateMilestone(template);
            return milestone;
        }

        public TemplateMessage SaveTemplateMessage(TemplateMessage template)
        {
            SetCreateValues(template);
            return dao.SaveTemplateMessage(template);
        }

        public TemplateTask SaveTemplateTask(TemplateTask template)
        {
            SetCreateValues(template);
            return dao.SaveTemplateTask(template);
        }

        public void RemoveTemplateProject(int id)
        {
            dao.RemoveTemplateProject(id);
        }

        public void RemoveTemplateMilestone(int id)
        {
            dao.RemoveTemplateMilestone(id);
        }

        public void RemoveTemplateMessage(int id)
        {
            dao.RemoveTemplateMessage(id);
        }

        public void RemoveTemplateTask(int id)
        {
            dao.RemoveTemplateTask(id);
        }

		#region Create Project From Template
		public Project CreateProjectFromTemplate(int templateId)
		{
			var template = dao.GetTemplateProject(templateId);
			return CreateProjectFromTemplate(templateId, template.Title, template.Responsible, template.Description, template.Tags, false, template);
		}

		public Project CreateProjectFromTemplate(int templateId, string title, Guid responsible, string description, string tags, bool isPrivate)
		{
			var template = dao.GetTemplateProject(templateId);
			return CreateProjectFromTemplate(templateId, title, responsible, description, tags, isPrivate, template);
		}

		private Project CreateProjectFromTemplate(int templateId, string title, Guid responsible, string description, string tags, bool isPrivate, TemplateProject template)
		{
			var project = new Project()
			{
				Title = title,
				Description = string.IsNullOrEmpty(description) ? string.Empty : description,
				Responsible = responsible,
                Private = isPrivate
			};

			_factory.GetProjectEngine().SaveOrUpdate(project, true);
            _factory.GetProjectEngine().AddToTeam(project, new Participant(responsible), true);
            template.Team.ForEach(u => _factory.GetProjectEngine().AddToTeam(project, new Participant(u), true));
            _factory.GetTagEngine().SetProjectTags(project.ID, tags);

			foreach (var m in dao.GetTemplateMessages(templateId))
			{
				var message = new Message()
				{
					Project = project,
					Title = m.Title,
					Content = m.Text,
				};
                _factory.GetMessageEngine().SaveOrUpdate(message, true, new[] { responsible }, null);//NOTE: adding responsible itsef as participant
			}

			var milestoneIdMap = new Dictionary<int, int>();
			foreach (var m in dao.GetTemplateMilestones(templateId))
			{
				var milestone = new Milestone()
				{
					DeadLine = GetMilestoneDeadLine(m.DurationInDays),
					IsKey = m.IsKey,
					IsNotify = m.IsNotify,
					Project = project,
					Title = m.Title,
				};
				var newMilestone = _factory.GetMilestoneEngine().SaveOrUpdate(milestone);
				milestoneIdMap[m.Id] = newMilestone.ID;
			}

			foreach (var t in dao.GetTemplateTasks(templateId))
			{
				var task = new Task()
				{
					Description = t.Description,
					Milestone = milestoneIdMap.ContainsKey(t.MilestoneId) ? milestoneIdMap[t.MilestoneId] : 0,
					Priority = t.Priority,
					Project = project,
					Responsible = t.Responsible,
					SortOrder = t.SortOrder,
					Title = t.Title,
				};
				_factory.GetTaskEngine().SaveOrUpdate(task, null, true);
			}

			return project;
		} 
		#endregion

		#region Milestone DeadLine
		private static DateTime GetMilestoneDeadLine(int days)
		{
            var weekNum = (int)(days / 10);
            var dayNum = days - weekNum * 10;
            var today = TenantUtil.DateTimeNow().Date;
            var todayDayOfWeek = (int)today.DayOfWeek;
            var firstDayOfWeek = (int)System.Globalization.DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek;

            if (firstDayOfWeek == 0)
            {
                today = today.AddDays(7 - todayDayOfWeek);//set next week first day
                return today.AddDays((weekNum - 1) * 7 + dayNum);
            }
            else
            {
                var tmp = 7 + firstDayOfWeek - todayDayOfWeek;
                if (tmp > 7) tmp = tmp - 7;
                today = today.AddDays(tmp);//set next week first day
                tmp = dayNum - 1;
                if (tmp < 0) tmp = tmp + 7;
                return today.AddDays((weekNum - 1) * 7 + tmp);
            }
		} 
		#endregion


        private void SetCreateValues(TemplateBase template)
        {
            if (template == null) throw new ArgumentNullException("template");

            if (template.CreateBy == default(Guid)) template.CreateBy = SecurityContext.CurrentAccount.ID;
            if (template.CreateOn == default(DateTime)) template.CreateOn = TenantUtil.DateTimeNow();
        }

		public void SetTaskOrders(int? milestoneId, int taskID, int? prevTaskID, int? nextTaskID)
		{
			dao.SetTaskOrders(milestoneId, taskID, prevTaskID, nextTaskID);
		}
    }
}
