using System;
using ASC.Core.Users;
using System.Runtime.Serialization;
using ASC.Files.Core.Security;

namespace ASC.Web.Files.Services.WCFService
{
    [DataContract(Name = "group_info", Namespace = "")]
    public class GroupInfoWrapper
    {
        [DataMember(Name = "id")]
        public string ID
        {
            get;
            set;
        }

        [DataMember(Name = "name")]
        public string Name
        {
            get;
            set;
        }

        [DataMember(Name = "is_system")]
        public bool IsSystem
        {
            get;
            set;
        }

        public GroupInfoWrapper()
        {

        }

        public GroupInfoWrapper(GroupInfo g, bool system)
        {
            ID = g.ID.ToString();
            Name = g.Name;
            IsSystem = system;
        }
    }



    [DataContract(Name = "ace_wrapper", Namespace = "")]
    public class AceWrapper
    {
        [DataMember(Name = "id", Order = 1)]
        public Guid SubjectId { get; set; }

        [DataMember(Name = "title", Order = 2)]
        public string SubjectName { get; set; }

        [DataMember(Name = "is_group", Order = 3)]
        public bool SubjectGroup { get; set; }

        [DataMember(Name = "owner", Order = 4)]
        public bool Owner { get; set; }

        [DataMember(Name = "ace_status", Order = 5)]
        public FileShare Share { get; set; }

        [DataMember(Name = "object_ids", Order = 6)]
        public ItemList<String> ObjectIDs { get; set; }

        [DataMember(Name = "locked", Order = 7)]
        public bool LockedRights { get; set; }
    }
}