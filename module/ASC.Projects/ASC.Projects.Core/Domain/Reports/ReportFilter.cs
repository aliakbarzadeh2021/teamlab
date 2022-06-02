﻿using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;

namespace ASC.Projects.Core.Domain.Reports
{
    public class ReportFilter : ICloneable
    {
        public ReportTimeInterval TimeInterval
        {
            get;
            set;
        }

        public DateTime FromDate
        {
            get;
            set;
        }

        public DateTime ToDate
        {
            get;
            set;
        }

        public List<int> ProjectIds
        {
            get;
            internal set;
        }

        public List<ProjectStatus> ProjectStatuses
        {
            get;
            internal set;
        }

        public string ProjectTag
        {
            get;
            set;
        }

        public Guid UserId
        {
            get;
            set;
        }

        public Guid DepartmentId
        {
            get;
            set;
        }

        public List<MilestoneStatus> MilestoneStatuses
        {
            get;
            internal set;
        }

        public List<TaskStatus> TaskStatuses
        {
            get;
            internal set;
        }

        public int ViewType
        {
            get;
            set;
        }


        internal bool HasUserId
        {
            get { return UserId != default(Guid) || DepartmentId != default(Guid); }
        }

        internal bool HasProjectIds
        {
            get { return 0 < ProjectIds.Count; }
        }

        internal bool HasProjectStatuses
        {
            get { return 0 < ProjectStatuses.Count; }
        }

        internal bool HasMilestoneStatuses
        {
            get { return 0 < MilestoneStatuses.Count; }
        }

        internal bool HasTaskStatuses
        {
            get { return 0 < TaskStatuses.Count; }
        }


        public ReportFilter()
        {
            ToDate = DateTime.MaxValue;
            ProjectIds = new List<int>();
            ProjectStatuses = new List<ProjectStatus>();
            MilestoneStatuses = new List<MilestoneStatus>();
            TaskStatuses = new List<TaskStatus>();
        }


        public void SetProjectIds(IEnumerable<int> ids)
        {
            ProjectIds.Clear();
            if (0 < ids.Count()) ProjectIds.AddRange(ids);
            else ProjectIds.Add(-1);
        }


        public string ToXml()
        {
            return ReportFilterSerializer.ToXml(this);
        }

        public static ReportFilter FromXml(string xml)
        {
            return ReportFilterSerializer.FromXml(xml);
        }

        public string ToUri()
        {
            return ReportFilterSerializer.ToUri(this);
        }

        public static ReportFilter FromUri(string uri)
        {
            return ReportFilterSerializer.FromUri(uri);
        }

        public static ReportFilter FromUri(Uri uri)
        {
            return FromUri(uri.Query);
        }

        public object Clone()
        {
            return FromXml(ToXml());
        }


        internal List<string> GetUserIds()
        {
            var result = new List<string>();
            if (UserId != Guid.Empty)
            {
                result.Add(UserId.ToString());
            }
            else if (DepartmentId != Guid.Empty)
            {
                result.AddRange(CoreContext.UserManager.GetUsersByGroup(DepartmentId).Select(u => u.ID.ToString()));
            }
            return result;
        }

        internal DateTime GetFromDate()
        {
            return GetFromDate(false);
        }

        internal DateTime GetToDate()
        {
            return GetToDate(false);
        }

        public DateTime GetFromDate(bool toUtc)
        {
            var date = DateTime.MinValue;
            if (TimeInterval == ReportTimeInterval.Absolute)
            {
                date = FromDate;
            }
            else if (TimeInterval == ReportTimeInterval.Relative)
            {
                if (FromDate != DateTime.MinValue && FromDate != DateTime.MaxValue)
                {
                    date = TenantUtil.DateTimeNow();
                }
            }
            else
            {
                date = GetDate(true);
            }
            if (date != DateTime.MinValue && date != DateTime.MaxValue)
            {
                date = date.Date;
                if (toUtc) date = TenantUtil.DateTimeToUtc(date);
            }
            return date;
        }

        public DateTime GetToDate(bool toUtc)
        {
            var date = DateTime.MaxValue;
            if (TimeInterval == ReportTimeInterval.Absolute)
            {
                date = ToDate;
            }
            else if (TimeInterval == ReportTimeInterval.Relative)
            {
                if (FromDate != DateTime.MinValue && FromDate != DateTime.MaxValue && ToDate != DateTime.MinValue && ToDate != DateTime.MaxValue)
                {
                    date = TenantUtil.DateTimeNow().Add(ToDate - FromDate);
                }
            }
            else
            {
                date = GetDate(false);
            }
            if (date != DateTime.MinValue && date != DateTime.MaxValue)
            {
                date = date.Date.AddTicks(TimeSpan.TicksPerDay - 1);
                if (toUtc) date = TenantUtil.DateTimeToUtc(date);
            }
            return date;
        }

        private DateTime GetDate(bool start)
        {
            var date = TenantUtil.DateTimeNow();
            if (TimeInterval == ReportTimeInterval.Today)
            {
                return date;
            }
            else if (TimeInterval == ReportTimeInterval.Yesterday)
            {
                return date.AddDays(-1);
            }
            else if (TimeInterval == ReportTimeInterval.Tomorrow)
            {
                return date.AddDays(1);
            }
            else if (TimeInterval == ReportTimeInterval.CurrWeek || TimeInterval == ReportTimeInterval.NextWeek || TimeInterval == ReportTimeInterval.PrevWeek)
            {
                var diff = CoreContext.TenantManager.GetCurrentTenant().GetCulture().DateTimeFormat.FirstDayOfWeek - date.DayOfWeek;
                if (0 < diff) diff -= 7;
                date = date.AddDays(diff);
                if (TimeInterval == ReportTimeInterval.NextWeek) date = date.AddDays(7);
                if (TimeInterval == ReportTimeInterval.PrevWeek) date = date.AddDays(-7);
                if (!start) date = date.AddDays(7).AddDays(-1);
                return date;
            }
            else if (TimeInterval == ReportTimeInterval.CurrMonth || TimeInterval == ReportTimeInterval.NextMonth || TimeInterval == ReportTimeInterval.PrevMonth)
            {
                date = new DateTime(date.Year, date.Month, 1);
                if (TimeInterval == ReportTimeInterval.NextMonth) date = date.AddMonths(1);
                if (TimeInterval == ReportTimeInterval.PrevMonth) date = date.AddMonths(-1);
                if (!start) date = date.AddMonths(1).AddDays(-1);
                return date;
            }
            else if (TimeInterval == ReportTimeInterval.CurrYear || TimeInterval == ReportTimeInterval.NextYear || TimeInterval == ReportTimeInterval.PrevYear)
            {
                date = new DateTime(date.Year, 1, 1);
                if (TimeInterval == ReportTimeInterval.NextYear) date = date.AddYears(1);
                if (TimeInterval == ReportTimeInterval.PrevYear) date = date.AddYears(-1);
                if (!start) date = date.AddYears(1).AddDays(-1);
                return date;
            }
            throw new ArgumentOutOfRangeException("TimeInterval");
        }
    }
}
