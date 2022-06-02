using System;
using System.Runtime.Serialization;

namespace ASC.Web.Studio.Services.Backup
{
    [DataContract(Namespace = "")]
    public enum BackupRequestStatus
    {
        [EnumMember(Value = "started")]
        Started,
        [EnumMember(Value = "working")]
        Working,
        [EnumMember(Value = "uploading")]
        Uploading,
        [EnumMember(Value = "done")]
        Done,
        [EnumMember(Value = "expired")]
        Expired,
		[EnumMember(Value = "error")]
		Error,
	}

    [DataContract(Namespace = "",Name = "backuprequest")]
    public class BackupRequest
    {
        public int TennantId { get; set; }
        public Guid UserId { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "status")]
        public BackupRequestStatus Status { get; set; }

        [DataMember(Name = "percentdone")]
        public int Percentdone { get; set; }

        [DataMember(Name = "completed")]
        public bool Completed { get; set; }

        [DataMember(Name = "link")]
        public string FileLink { get; set; }

        [DataMember(Name = "info")]
        public string Info { get; set; }

        [DataMember(Name = "size")]
        public long Size { get; set; }

        [DataMember(Name = "created")]
        public DateTime Created { get; set; }

        [DataMember(Name = "expires")]
        public DateTime Availible { get; set; }

		internal string BackupFile { get; set; }
    }
}