using System;
using System.Collections.Generic;
using ASC.Common.Security;

namespace ASC.Core
{
    public interface IAzManagerClient
    {
        IEnumerable<AzRecord> GetAces(Guid subjectID, Guid actionID, ISecurityObjectId objectId);

        IEnumerable<AzRecord> GetAcesWithInherits(Guid subjectID, Guid actionID, ISecurityObjectId objectId, ISecurityObjectProvider secObjProvider);

        void AddAce(AzRecord azRecord);

        void RemoveAce(AzRecord azRecord);
    }
}