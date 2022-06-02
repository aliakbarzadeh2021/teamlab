using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Files.Core;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.Web.Core.Subscriptions;
using ASC.Web.Files.Services.NotifyService;

namespace ASC.Web.Files.Classes
{
    public class SubscriptionManager : IProductSubscriptionManager
    {
        private List<SubscriptionType> subscriptionTypes;
        private List<SubscriptionGroup> subscriptionGroups;
        private Guid previousUserID;
        private Guid subscriptionTypeFollow = new Guid("{EAC597BD-9C2A-4470-898E-FE105C120222}");
        private Guid subscriptionGroupsFollow = new Guid("{45272EA9-3AB3-4ab4-B118-057B783DEFE7}");


        private void LoadSubscriptionTypes()
        {
            previousUserID = SecurityContext.CurrentAccount.ID;
            subscriptionTypes = new List<SubscriptionType>();

            subscriptionTypes.Add(new SubscriptionType()
            {
                ID = subscriptionTypeFollow,
                Name = Resources.FilesCommonResource.SignedForMe,
                NotifyAction = NotifyConstants.Event_DocumentInformer,
                Single = false,
                IsEmptySubscriptionType = IsEmptySubscriptionFollow,
                GetSubscriptionObjects = GetFilesForMe
            });
        }

        private void LoadSubscriptionGroups()
        {
            subscriptionGroups = new List<SubscriptionGroup>();
            subscriptionGroups.Add(new SubscriptionGroup()
            {
                ID = subscriptionGroupsFollow,
                Name = Resources.FilesCommonResource.Documents
            });
        }

        private bool IsEmptySubscriptionFollow(Guid productID, Guid moduleID, Guid typeID)
        {
            var type = subscriptionTypes.Find(t => t.ID.Equals(typeID));

            var objIDs = SubscriptionProvider.GetSubscriptions(type.NotifyAction, new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), ""));
            if (objIDs != null && objIDs.Length > 0)
                return false;

            return true;
        }

        private List<SubscriptionObject> GetFilesForMe(Guid productID, Guid moduleOrGroupID, Guid typeID)
        {
            var groupFollow = subscriptionGroups.Find(r => r.ID.Equals(subscriptionGroupsFollow));
            var typeFollow = subscriptionTypes.Find(r => r.ID.Equals(subscriptionTypeFollow));
            var subscriptionObjects = new List<SubscriptionObject>();

            using (var fileDao = Global.DaoFactory.GetFileDao())
            {
                var ids = NotifySource.Instance.GetSubscriptionProvider()
                    .GetSubscriptions(NotifyConstants.Event_DocumentInformer, NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()))
                    .Select(o => (o ?? string.Empty).Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries))
                    .Where(o => o.Length == 2)
                    .Select(o => Convert.ToInt32(o[1]))
                    .ToArray();

                Global.GetFilesSecurity().FilterRead(fileDao.GetFiles(ids))
                    .ToList()
                    .ForEach(file =>
                    {
                        subscriptionObjects.Add(new SubscriptionObject()
                        {
                            ID = file.UniqID,
                            Name = HttpUtility.HtmlDecode(file.Title),
                            URL = file.ViewUrl,
                            SubscriptionGroup = groupFollow,
                            SubscriptionType = typeFollow
                        });
                    });
            }

            return subscriptionObjects;
        }

        #region IProductSubscriptionManager Members

        public GroupByType GroupByType
        {
            get { return GroupByType.Groups; }
        }

        #endregion

        #region ISubscriptionManager Members

        public List<SubscriptionObject> GetSubscriptionObjects()
        {
            var objects = new List<SubscriptionObject>();
            objects.AddRange(GetFilesForMe(Guid.Empty, Guid.Empty, Guid.Empty));
            return objects;
        }

        public List<SubscriptionType> GetSubscriptionTypes()
        {
            if (subscriptionTypes == null || SecurityContext.CurrentAccount.ID != previousUserID)
            {
                LoadSubscriptionTypes();
            }

            return subscriptionTypes;
        }

        public ISubscriptionProvider SubscriptionProvider
        {
            get { return NotifySource.Instance.GetSubscriptionProvider(); }
        }

        #endregion

        #region IProductSubscriptionManager Members

        public List<SubscriptionGroup> GetSubscriptionGroups()
        {
            if (subscriptionGroups == null)
            {
                LoadSubscriptionGroups();
            }

            return subscriptionGroups;
        }

        #endregion
    }
}