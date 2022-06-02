﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace ASC.Projects.Core.Domain.Reports
{
    static class ReportFilterSerializer
    {
        private const string LIST_SEP = "|";


        public static string ToXml(ReportFilter filter)
        {
            var doc = new XDocument();
            var root = new XElement("filter");

            var date = new XElement("date");
            root.Add(date);
            date.Add(new XAttribute("interval", filter.TimeInterval));
            if (HasDate(filter.FromDate)) date.Add(new XAttribute("from", filter.FromDate.ToString("yyyyMMdd")));
            if (HasDate(filter.ToDate)) date.Add(new XAttribute("to", filter.ToDate.ToString("yyyyMMdd")));

            if (filter.HasProjectIds || filter.HasProjectStatuses || !string.IsNullOrEmpty(filter.ProjectTag))
            {
                var projects = new XElement("projects");
                root.Add(projects);

                foreach (var id in filter.ProjectIds)
                {
                    projects.Add(new XElement("id", id));
                }
                foreach (var status in filter.ProjectStatuses)
                {
                    projects.Add(new XElement("status", (int)status));
                }
                if (!string.IsNullOrEmpty(filter.ProjectTag))
                {
                    projects.Add(new XElement("tag", filter.ProjectTag));
                }
            }
            if (filter.HasUserId)
            {
                var users = new XElement("users");
                root.Add(users);
                if (filter.DepartmentId != default(Guid))
                {
                    users.Add(new XAttribute("dep", filter.DepartmentId.ToString("N")));
                }
                if (filter.UserId != default(Guid))
                {
                    users.Add(new XElement("id", filter.UserId.ToString("N")));
                }
            }
            if (filter.HasMilestoneStatuses)
            {
                var milestone = new XElement("milestones");
                root.Add(milestone);

                foreach (var status in filter.MilestoneStatuses)
                {
                    milestone.Add(new XElement("status", (int)status));
                }
            }
            if (filter.HasTaskStatuses)
            {
                var tasks = new XElement("tasks");
                root.Add(tasks);

                foreach (var status in filter.TaskStatuses)
                {
                    tasks.Add(new XElement("status", (int)status));
                }
            }
            if (filter.ViewType != 0)
            {
                root.Add(new XAttribute("view", filter.ViewType));
            }

            doc.AddFirst(root);
            return doc.ToString(SaveOptions.DisableFormatting);
        }

        public static ReportFilter FromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml)) throw new ArgumentNullException("xml");

            var filter = new ReportFilter();
            var root = XDocument.Parse(xml).Element("filter");

            var date = root.Element("date");
            if (date != null)
            {
                var attribute = date.Attribute("from");
                if (attribute != null) filter.FromDate = DateTime.ParseExact(attribute.Value, "yyyyMMdd", null);
                attribute = date.Attribute("to");
                if (attribute != null) filter.ToDate = DateTime.ParseExact(attribute.Value, "yyyyMMdd", null);
                attribute = date.Attribute("interval");
                if (attribute != null) filter.TimeInterval = (ReportTimeInterval)Enum.Parse(typeof(ReportTimeInterval), attribute.Value, true);
            }

            var projects = root.Element("projects");
            if (projects != null)
            {
                foreach (var id in projects.Elements("id"))
                {
                    filter.ProjectIds.Add(int.Parse(id.Value));
                }
                foreach (var status in projects.Elements("status"))
                {
                    filter.ProjectStatuses.Add((ProjectStatus)int.Parse(status.Value));
                }
                foreach (var tag in projects.Elements("tag"))
                {
                    filter.ProjectTag = tag.Value;
                }
            }

            var tasks = root.Element("tasks");
            if (tasks != null)
            {
                foreach (var status in tasks.Elements("status"))
                {
                    filter.TaskStatuses.Add((TaskStatus)int.Parse(status.Value));
                }
            }

            var users = root.Element("users");
            if (users != null)
            {
                if (users.Attribute("dep") != null)
                {
                    filter.DepartmentId = new Guid(users.Attribute("dep").Value);
                }
                foreach (var id in users.Elements("id"))
                {
                    filter.UserId = new Guid(id.Value);
                }
            }

            var milestones = root.Element("milestones");
            if (milestones != null)
            {
                foreach (var status in milestones.Elements("status"))
                {
                    filter.MilestoneStatuses.Add((MilestoneStatus)int.Parse(status.Value));
                }
            }

            var view = root.Attribute("view");
            if (view != null)
            {
                filter.ViewType = int.Parse(view.Value);
            }

            return filter;
        }

        public static string ToUri(ReportFilter filter)
        {
            var uri = new StringBuilder();

            uri.AppendFormat("&ftime={0}", filter.TimeInterval);
            if (HasDate(filter.FromDate))
            {
                uri.AppendFormat("&ffrom={0}", filter.FromDate.ToString("yyyyMMdd"));
            }
            if (HasDate(filter.ToDate))
            {
                uri.AppendFormat("&fto={0}", filter.ToDate.ToString("yyyyMMdd"));
            }
            if (filter.HasProjectIds)
            {
                uri.AppendFormat("&fpid={0}", string.Join(LIST_SEP, filter.ProjectIds.Select(id => id.ToString()).ToArray()));
            }
            if (filter.HasProjectStatuses)
            {
                uri.AppendFormat("&fps={0}", string.Join(LIST_SEP, filter.ProjectStatuses.Select(s => s.ToString()).ToArray()));
            }
            if (!string.IsNullOrEmpty(filter.ProjectTag))
            {
                uri.AppendFormat("&fpt={0}", filter.ProjectTag);
            }
            if (filter.UserId != default(Guid))
            {
                uri.AppendFormat("&fu={0:N}", filter.UserId);
            }
            if (filter.DepartmentId != default(Guid))
            {
                uri.AppendFormat("&fd={0:N}", filter.DepartmentId);
            }
            if (filter.HasMilestoneStatuses)
            {
                uri.AppendFormat("&fms={0}", string.Join(LIST_SEP, filter.MilestoneStatuses.Select(s => s.ToString()).ToArray()));
            }
            if (filter.HasTaskStatuses)
            {
                uri.AppendFormat("&fts={0}", string.Join(LIST_SEP, filter.TaskStatuses.Select(s => s.ToString()).ToArray()));
            }
            if (filter.ViewType != 0)
            {
                uri.AppendFormat("&fv={0}", filter.ViewType);
            }
            return uri.ToString().Trim('&').ToLower();
        }

        public static ReportFilter FromUri(string uri)
        {
            if (string.IsNullOrEmpty(uri)) throw new ArgumentNullException("uri");

            var filter = new ReportFilter();

            var p = GetParameterFromUri(uri, "ftime");
            if (!string.IsNullOrEmpty(p)) filter.TimeInterval = (ReportTimeInterval)Enum.Parse(typeof(ReportTimeInterval), p, true);

            p = GetParameterFromUri(uri, "ffrom");
            if (!string.IsNullOrEmpty(p)) filter.FromDate = DateTime.ParseExact(p, "yyyyMMdd", null);

            p = GetParameterFromUri(uri, "fto");
            if (!string.IsNullOrEmpty(p)) filter.ToDate = DateTime.ParseExact(p, "yyyyMMdd", null);

            p = GetParameterFromUri(uri, "fu");
            if (!string.IsNullOrEmpty(p)) filter.UserId = new Guid(p);

            p = GetParameterFromUri(uri, "fd");
            if (!string.IsNullOrEmpty(p)) filter.DepartmentId = new Guid(p);

            p = GetParameterFromUri(uri, "fpid");
            if (!string.IsNullOrEmpty(p)) filter.ProjectIds = Split(p).Select(v => int.Parse(v)).ToList();

            p = GetParameterFromUri(uri, "fps");
            if (!string.IsNullOrEmpty(p)) filter.ProjectStatuses = Split(p).Select(v => (ProjectStatus)Enum.Parse(typeof(ProjectStatus), v, true)).ToList();

            p = GetParameterFromUri(uri, "fpt");
            if (!string.IsNullOrEmpty(p)) filter.ProjectTag = p;

            p = GetParameterFromUri(uri, "fms");
            if (!string.IsNullOrEmpty(p)) filter.MilestoneStatuses = Split(p).Select(v => (MilestoneStatus)Enum.Parse(typeof(MilestoneStatus), v, true)).ToList();

            p = GetParameterFromUri(uri, "fts");
            if (!string.IsNullOrEmpty(p)) filter.TaskStatuses = Split(p).Select(v => (TaskStatus)Enum.Parse(typeof(TaskStatus), v, true)).ToList();

            p = GetParameterFromUri(uri, "fv");
            if (!string.IsNullOrEmpty(p)) filter.ViewType = int.Parse(p);

            return filter;
        }

        private static string GetParameterFromUri(string uri, string paramName)
        {
            foreach (var parameter in uri.Split(new[] { '?', '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = HttpUtility.UrlDecode(parameter).Split('=');
                if (parts.Length == 2 && string.Compare(parts[0], paramName, true) == 0) return parts[1];
            }
            return null;
        }

        private static IEnumerable<string> Split(string value)
        {
            return value.Split(new[] { LIST_SEP }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static bool HasDate(DateTime dateTime)
        {
            return dateTime != DateTime.MinValue && dateTime != DateTime.MaxValue;
        }
    }
}
