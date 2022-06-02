using System;
using System.Runtime.Serialization;

namespace ASC.Web.Files.Services.WCFService
{
    [DataContract(Name = "DataToImport", Namespace = "")]
    public class DataToImport
    {
        [DataMember(EmitDefaultValue = false, Name = "title", IsRequired = true)]
        public String Title
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Name = "content_link", IsRequired = true)]
        public String ContentLink
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Name = "create_by", IsRequired = false)]
        public string CreateBy
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Name = "create_on", IsRequired = false)]
        public String CreateOn
        {
            get;
            set;
        }
    }
}
