using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using ASC.Files.Core;
using File = ASC.Files.Core.File;

namespace ASC.Web.Files.Services.WCFService
{
    [ServiceContract]
    public interface IFileStorageService
    {
        #region Folder Manager

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLSubFoldersTemplate, Method = "POST")]
        ItemList<Folder> GetFolders(String id, String from, String count, OrderBy orderBy);

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLGetFolderTemplate)]
        Folder GetFolder(String id);

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLCreateNewFolderTemplate)]
        Folder CreateNewFolder(String title, String parentFolderID);

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLFolderRenameTemplate)]
        String FolderRename(String id, String title);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLGetFolderItemsTemplate, Method = "POST")]
        DataWrapper GetFolderItems(String id, String from, String count, String filter, OrderBy orderBy, String subjectID, String setThumbnailURLBool);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.JSONCheckMoveFilesTemplate, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemDictionary<int, String> MoveOrCopyFilesCheck(ItemList<String> items, String destFolderID);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.JSONMoveItemsTemplate, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> MoveOrCopyItems(ItemList<String> items, String destFolderID, String overwriteFiles, String isCopyOperation);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLDeleteItemsTemplate, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> DeleteItems(ItemList<String> items);

        #endregion


        #region File Manager

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLLastFileVersionTemplate)]
        File GetLastFileVersion(String fileID, String setThumbnailURLBool);

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLCreateNewFileTemplate)]
        File CreateNewFile(String folderID, String fileTitle);

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLGetFileStreamForEditingTemplate)]
        Stream GetFileStreamForEditing();

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLUploadFileTemplate, Method = "POST")]
        void SaveEditedFile(Stream fileStream);

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLFileRenameTemplate)]
        String FileRename(String id, String title);

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLUpdateToVersionTemplate)]
        File UpdateToVersion(String id, String version);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLGetFileHistoryTemplate)]
        ItemList<File> GetFileHistory(String id);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLGetSiblingsFileTemplate, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemDictionary<Int32, String> GetSiblingsFile(String fileID, OrderBy orderBy);

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLMakeEditFileTemplate, ResponseFormat = WebMessageFormat.Json)]
        string MakeEditFile(String fileID);

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLTrackEditFileTemplate)]
        void TrackEditFile(String fileID, string docKey, bool isFinish);

        #endregion


        #region Favorites Manager

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLGetFavoritesTemplate)]
        ItemList<Favorite> GetFavorites();

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLAddToFavoritesTemplate)]
        ItemList<Favorite> AddToFavorites(String folderPath, String title);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLDeleteFavoriteTemplate, Method = "DELETE")]
        void DeleteFavorite(String id);

        #endregion


        #region Utils

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.JSONBulkDownloadTemplate, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> BulkDownload(ItemList<String> items);

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.JSONGetTasksStatusesTemplate, ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> GetTasksStatuses();

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLEmptyTrashTemplate, ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> EmptyTrash();

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.JSONTerminateTasks, ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> TerminateTasks();

        #endregion


        #region Import

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLIsZohoAuthentificatedTemplate)]
        bool IsZohoAuthentificated(String login, String pass);

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLGetBoxNetDocsTemplate)]
        ItemList<DataToImport> GetBoxNetDocs(String token);

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLGetZohoDocsTemplate)]
        ItemList<DataToImport> GetZohoDocs(String login, String pass);

        [OperationContract]
        [WebGet(UriTemplate = UriTemplates.XMLGetGoogleDocsTemplate)]
        ItemList<DataToImport> GetGoogleDocs(String token);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLExecBoxNetDocsImportTemplate, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> ExecImportFromBoxNet(String token, String tofolder, String ignoreCoincidenceFiles, List<DataToImport> dataToImport);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLExecGetZohoDocsImportTemplate, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> ExecImportFromZoho(String login, String pass, String tofolder, String ignoreCoincidenceFiles, List<DataToImport> dataToImport);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLExecGoogleDocsImportTemplate, Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        ItemList<FileOperationResult> ExecImportFromGoogleDocs(String token, String tofolder, String ignoreCoincidenceFiles, List<DataToImport> dataToImport);

        #endregion


        #region Group Manager

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLGetGroupsTemplate, Method = "GET")]
        ItemList<GroupInfoWrapper> GetGroups();

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLSaveGroupTemplate, Method = "POST")]
        ItemList<GroupInfoWrapper> SaveGroup(string id, string name, ItemList<Guid> usersID);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLDeleteGroupTemplate, Method = "DELETE")]
        void DeleteGroup(string id);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLGetUsersByGroupTemplate, Method = "GET", ResponseFormat = WebMessageFormat.Json)]
        ItemList<Guid> GetUsersByGroup(string id);


        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLGetSharedInfoTemplate, Method = "POST")]
        ItemList<AceWrapper> GetSharedInfo(ItemList<String> items);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLSetAceObjectTemplate, Method = "POST")]
        void SetAceObject(ItemList<AceWrapper> aceWrappers);

        [OperationContract]
        [WebInvoke(UriTemplate = UriTemplates.XMLRemoveAceTemplate, Method = "GET")]
        void RemoveAce(string id);

        #endregion
    }
}