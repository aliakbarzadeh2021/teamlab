using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;

namespace ASC.Web.Controls.FileUploader.HttpModule
{
    public class UploadProgressStatistic
    {

        private static Dictionary<string, UploadProgressStatistic> _statistics = new Dictionary<string, UploadProgressStatistic>();
        public const string UploadIdField = "__UixdId";

        internal UploadProgressStatistic()
        {
            TotalBytes = 0;
            IsFinished = false;
            CurrentFile = string.Empty;
            CurrentFileIndex = -1;
            ReturnCode = 0;
        }

        private string DateTimeSerializeToJSON(DateTime dateTime)
        {
            var value = new DateTimeOffset(dateTime);
            var kind = dateTime.Kind;

            string str;
            long num = ((value.ToUniversalTime().Ticks - new DateTime(0x7b2, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks) / 0x2710L);

            switch (kind)
            {
                case DateTimeKind.Unspecified:
                case DateTimeKind.Local:
                    {
                        TimeSpan offset = value.Offset;
                        str = offset.Hours.ToString("+00;-00", CultureInfo.InvariantCulture) + offset.Minutes.ToString("00;00", CultureInfo.InvariantCulture);
                        break;
                    }
                default:
                    str = string.Empty;
                    break;
            }
            return ("\"\\/Date(" + num.ToString(CultureInfo.InvariantCulture) + str + ")\\/\"");
        }

        public string ToJson()
        {
            return string.Format("{{\"TotalBytes\":{0},\"Progress\":{1},\"CurrentFileIndex\":{2},\"CurrentFile\":\"{3}\",\"UploadId\":{4},\"IsFinished\":{5},\"Swf\":false,\"ReturnCode\":{6}}}",
                TotalBytes, Progress.ToString().Replace(',', '.'), CurrentFileIndex, CurrentFile, UploadId ?? "null", (IsFinished ? "true" : "false"), ReturnCode);
        }

        public string UploadId { get; set; }

        public long TotalBytes { get; set; }
        public long UploadedBytes { get; set; }
        public bool IsFinished { get; set; }

        public string CurrentFile { get; set; }
        public int CurrentFileIndex { get; set; }
        public float Progress { get; set; }

        public int ReturnCode { get; set; }

        internal void AddUploadedBytes(int bytesCount)
        {
            UploadedBytes += bytesCount;
            Progress = (float)UploadedBytes / TotalBytes;
            Progress = Progress > 1 ? 1 : Progress;
        }

        public static UploadProgressStatistic GetStatistic(string id)
        {
            UploadProgressStatistic us;
            if (!_statistics.TryGetValue(id, out us))
                us = new UploadProgressStatistic();
            return us;
        }

        internal void EndUpload()
        {
            IsFinished = true;
        }

        internal void BeginFileUpload(string fileName)
        {
            CurrentFile = fileName;
            CurrentFileIndex++;
        }

        internal void AddFormField(string name, string value)
        {
            if (name == UploadIdField)
            {
                UploadId = value;

                if (!_statistics.ContainsKey(value))
                    _statistics.Add(value, this);
            }
        }
    }
}