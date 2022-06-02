using System;
using ASC.Data.Storage;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Data;

namespace ASC.Projects.Engine
{
    public class EngineFactory
    {
        private readonly IDataStore _projectsStore;
        private readonly IDaoFactory daoFactory;
        public bool DisableNotifications { get; set; }

        public EngineFactory(String dbId, int tenantID, IDataStore projectsStore)
        {
            _projectsStore = projectsStore;
            daoFactory = new DaoFactory(dbId, tenantID);
        }

        public FileEngine GetFileEngine()
        {
            return new FileEngine(daoFactory, _projectsStore);
        }

        public ProjectEngine GetProjectEngine()
        {
            return new CachedProjectEngine(daoFactory,this);
        }

        public MilestoneEngine GetMilestoneEngine()
        {
            return new MilestoneEngine(daoFactory,this);
        }

        public EventEngine GetEventEngine()
        {
            return new EventEngine(daoFactory);
        }

        public CommentEngine GetCommentEngine()
        {
            return new CommentEngine(daoFactory);
        }

        public SearchEngine GetSearchEngine()
        {
            return new SearchEngine(daoFactory);
        }

        public TaskEngine GetTaskEngine()
        {
            return new TaskEngine(daoFactory,this);
        }

        public MessageEngine GetMessageEngine()
        {
            return new MessageEngine(daoFactory,this);
        }

        public TimeTrackingEngine GetTimeTrackingEngine()
        {
            return new TimeTrackingEngine(daoFactory);
        }

        public ParticipantEngine GetParticipantEngine()
        {
            return new ParticipantEngine(daoFactory);
        }

        public TagEngine GetTagEngine()
        {
            return new TagEngine(daoFactory);
        }

        public ReportEngine GetReportEngine()
        {
            return new ReportEngine(daoFactory);
        }

        public TemplateEngine GetTemplateEngine()
        {
            return new TemplateEngine(daoFactory,this);
        }

        public IssuesEngine GetIssueEngine()
        {
            return new IssuesEngine(daoFactory);
        }
    }
}
