using System.ComponentModel;

namespace ASC.Projects.Core.Domain
{
    [TypeConverter(typeof(LocalizedEnumConverter))]
    public enum TaskStatus
    {
        NotAccept = 0,
        Open = 1,
        Closed = 2,
        Disable = 3,
        Unclassified = 4,
        NotInMilestone = 5
    }
}