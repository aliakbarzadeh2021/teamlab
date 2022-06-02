using System.Runtime.Serialization;

namespace ASC.Files.Core.Security
{
    [DataContract(Name = "fileShare", Namespace = "")]
    public enum FileShare
    {
        [EnumMember]
        None,

        [EnumMember]
        ReadWrite,

        [EnumMember]
        Read,

        [EnumMember]
        Restrict,
    }
}
