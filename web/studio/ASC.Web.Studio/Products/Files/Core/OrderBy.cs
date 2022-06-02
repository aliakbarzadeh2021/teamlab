using System.Runtime.Serialization;
using System.Diagnostics;

namespace ASC.Files.Core
{
    [DataContract(Name = "sorted_by_type", Namespace = "")]
    public enum SortedByType
    {
        [EnumMember]
        DateAndTime,

        [EnumMember]
        AZ,

        [EnumMember]
        Size,

        [EnumMember]
        Author
    }

    [DataContract(Name = "orderBy", Namespace = "")]
    [DebuggerDisplay("{SortedBy} {IsAsc}")]
    public class OrderBy
    {
        [DataMember(IsRequired = true, Name = "is_asc", EmitDefaultValue = false)]
        public bool IsAsc
        {
            get;
            set;
        }

        [DataMember(IsRequired = true, Name = "property", EmitDefaultValue = false)]
        public SortedByType SortedBy
        {
            get;
            set;
        }

        public OrderBy(SortedByType sortedByType, bool isAsc)
        {
            SortedBy = sortedByType;
            IsAsc = isAsc;
        }
    }
}