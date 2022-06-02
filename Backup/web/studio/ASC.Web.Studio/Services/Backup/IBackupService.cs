using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using Microsoft.ServiceModel.Web;

namespace ASC.Web.Studio.Services.Backup
{
    [ServiceContract]
    public interface IBackupService
    {
        [WebHelp(Comment = "Returns list of backups")]
        [WebGet(UriTemplate = UriTemplates.BackupListTemplate, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        ItemList<BackupRequest> GetBackupList();

        [WebHelp(Comment = "Requests backup of system")]
        [WebGet(UriTemplate = UriTemplates.BackupRequestTemplate, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        BackupRequest RequestBackup();

        [WebHelp(Comment = "Returns list of backups")]
        [WebGet(UriTemplate = UriTemplates.BackupInfoTemplate, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        BackupRequest GetBackupStatus(string id);

    }


    [CollectionDataContract(Name = "{0}List", Namespace = "", ItemName = "entry")]
    public class ItemList<TItem> : List<TItem>
    {
        public ItemList()
            : base()
        {
        }

        public ItemList(IEnumerable<TItem> items)
            : base(items)
        {
        }
    }

    [CollectionDataContract(Name = "{1}Hash", Namespace = "", ItemName = "entry", KeyName = "key", ValueName = "value")]
    public class ItemDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public ItemDictionary()
            : base()
        {
        }

        public ItemDictionary(IDictionary<TKey,TValue> items):base(items)
        {
        }
    }


}