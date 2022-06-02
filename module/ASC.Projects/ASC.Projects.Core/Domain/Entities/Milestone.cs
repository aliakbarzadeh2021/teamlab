using System;
using System.Diagnostics;

namespace ASC.Projects.Core.Domain
{
    [DebuggerDisplay("Milestone: ID = {ID}, Title = {Title}, DeadLine = {DeadLine}")]
    public class Milestone : ProjectEntity
    {
        public MilestoneStatus Status { get; set; }

        public bool IsNotify { get; set; }

        public bool IsKey { get; set; }

        public DateTime DeadLine { get; set; }

        public bool CurrentUserHasTasks { get; set; }
    }
}