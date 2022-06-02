using System.Collections.Generic;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Users.Activity;

namespace ASC.Projects.Engine
{
    public class TimeTrackingEngine
    {
        private readonly ITimeSpendDao timeSpendDao;
        private readonly IProjectDao projectDao;
        private readonly ITaskDao taskDao;


        public TimeTrackingEngine(IDaoFactory daoFactory)
        {
            timeSpendDao = daoFactory.GetTimeSpendDao();
            projectDao = daoFactory.GetProjectDao();
            taskDao = daoFactory.GetTaskDao();
        }


        public List<TimeSpend> GetByTask(int taskId)
        {
            return timeSpendDao.GetByTask(taskId);
        }

        public List<TimeSpend> GetByProject(int projectId)
        {
            return timeSpendDao.GetByProject(projectId);
        }

        public TimeSpend GetByID(int id)
        {
            return timeSpendDao.GetById(id);
        }

        public bool HasTime(int taskId)
        {
            return timeSpendDao.HasTime(taskId);
        }

        public Dictionary<int, bool> HasTime(params int[] tasks)
        {
            return timeSpendDao.HasTime(tasks);
        }

        public TimeSpend SaveOrUpdate(TimeSpend timeSpend)
        {
            return SaveOrUpdate(timeSpend, false);
        }
        
        public TimeSpend SaveOrUpdate(TimeSpend timeSpend, bool isImport)
        {
            if (isImport)
            {
                TimeLinePublisher.TimeSpend(timeSpend, projectDao.GetById(timeSpend.Project), taskDao.GetById(timeSpend.RelativeTask), EngineResource.ActionText_Imported, UserActivityConstants.ActivityActionType, UserActivityConstants.SmallActivity);
            }
            else
            {
                TimeLinePublisher.TimeSpend(timeSpend, projectDao.GetById(timeSpend.Project), taskDao.GetById(timeSpend.RelativeTask), EngineResource.ActionText_Add, UserActivityConstants.ActivityActionType, UserActivityConstants.SmallActivity);
            }
            return timeSpendDao.Save(timeSpend);
        }

        public void Delete(int id)
        {
            timeSpendDao.Delete(id);
        }
    }
}
