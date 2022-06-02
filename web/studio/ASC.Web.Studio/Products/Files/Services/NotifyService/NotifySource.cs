using System;
using System.Collections.Generic;
using ASC.Notify.Messages;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Web.Files.Resources;
using NotifySourceBase = ASC.Core.Notify.NotifySource;

namespace ASC.Web.Files.Services.NotifyService
{
    public class NotifySource : NotifySourceBase
    {
        private static NotifySource instance = new NotifySource();

        public static NotifySource Instance
        {
            get { return instance; }
        }


        public NotifySource()
            : base(new Guid("6FE286A4-479E-4c25-A8D9-0156E332B0C0"))
        {
        }

        protected override IActionProvider CreateActionProvider()
        {
            return new ConstActionProvider(
                NotifyConstants.Event_DocumentInformer,
                //NotifyConstants.Event_UploadDocument,
                //NotifyConstants.Event_DeleteDocument,
                NotifyConstants.Event_ShareDocument,
                NotifyConstants.Event_ShareFolder,
                NotifyConstants.Event_UpdateDocument);
        }

        protected override IActionPatternProvider CreateActionPatternProvider()
        {
            return new XmlActionPatternProvider(
                GetType().Assembly,
                "ASC.Web.Files.Services.NotifyService.action_pattern.xml",
                ActionProvider,
                PatternProvider) { GetPatternMethod = ChoosePattern };
        }

        private IPattern ChoosePattern(INotifyAction action, string senderName, ASC.Notify.Engine.NotifyRequest request)
        {
            if (action == NotifyConstants.Event_DocumentInformer)
            {
                ITagValue tag = request.Arguments.Find((tv) => tv.Tag.Name == "EventType");
                if (tag != null)
                {
                    return ActionPatternProvider.GetPattern(new NotifyAction(Convert.ToString(tag.Value), ""), senderName);
                }
            }
            return null;
        }

        protected override IDependencyProvider CreateDependencyProvider()
        {
            return this;
        }

        protected override IPatternProvider CreatePatternsProvider()
        {
            return new XmlPatternProvider(FilesPatternResource.patterns);
        }
    }
}
