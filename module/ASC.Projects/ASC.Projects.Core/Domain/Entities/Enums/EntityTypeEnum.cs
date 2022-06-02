using System.ComponentModel;

namespace ASC.Projects.Core.Domain
{
    [TypeConverter(typeof(LocalizedEnumConverter))]
    public enum EntityType
    {
        Project,
        Milestone,
        Task,
        Team,
        Comment,
        Message,
        File,
        TimeSpend
    }
}
