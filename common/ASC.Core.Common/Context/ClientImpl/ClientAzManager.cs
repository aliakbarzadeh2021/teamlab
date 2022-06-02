using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;

namespace ASC.Core
{
    class ClientAzManager : IAzManagerClient
    {
        private readonly IAzService service;


        public ClientAzManager(IAzService service)
        {
            this.service = service;
        }


        public IEnumerable<AzRecord> GetAces(Guid subjectId, Guid actionId, ISecurityObjectId objectId)
        {
            var fullObjectId = AzObjectIdHelper.GetFullObjectId(objectId);
            return GetAcesInternal()
                .Where(a => a.SubjectId == subjectId && a.ActionId == actionId && a.ObjectId == fullObjectId);
        }

        public IEnumerable<AzRecord> GetAcesWithInherits(Guid subjectId, Guid actionId, ISecurityObjectId objectId, ISecurityObjectProvider secObjProvider)
        {
            if (objectId == null)
            {
                return GetAces(subjectId, actionId, null);
            }

            var result = new List<AzRecord>();
            var aces = GetAcesInternal()
                .Where(a => a.SubjectId == subjectId && a.ActionId == actionId);

            var fullId = AzObjectIdHelper.GetFullObjectId(objectId);
            result.AddRange(aces.Where(a => a.ObjectId == fullId));

            var inherits = new List<AzRecord>();
            var secObjProviderHelper = new AzObjectSecurityProviderHelper(objectId, secObjProvider);
            while (secObjProviderHelper.NextInherit())
            {
                fullId = AzObjectIdHelper.GetFullObjectId(secObjProviderHelper.CurrentObjectId);
                inherits.AddRange(aces.Where(a => a.ObjectId == fullId));
            }

            inherits.AddRange(aces.Where(a => a.ObjectId == null));

            result.AddRange(DistinctAces(inherits));
            return result;
        }

        public void AddAce(AzRecord r)
        {
            service.SaveAce(CoreContext.TenantManager.GetCurrentTenant().TenantId, r);
        }

        public void RemoveAce(AzRecord r)
        {
            service.RemoveAce(CoreContext.TenantManager.GetCurrentTenant().TenantId, r);
        }


        private IEnumerable<AzRecord> GetAcesInternal()
        {
            return service.GetAces(CoreContext.TenantManager.GetCurrentTenant().TenantId, default(DateTime));
        }

        private IEnumerable<AzRecord> DistinctAces(IEnumerable<AzRecord> inheritAces)
        {
            var aces = new Dictionary<string, AzRecord>();
            foreach (var a in inheritAces)
            {
                var key = string.Format("{0}{1}{2:D}", a.SubjectId, a.ActionId, a.Reaction);
                aces[string.Format("{0}{1}{2:D}", a.SubjectId, a.ActionId, a.Reaction)] = a;
            }
            return aces.Values;
        }
    }
}