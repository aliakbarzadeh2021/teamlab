using System;

namespace ASC.Web.Files.Services.WCFService
{
    class UriTemplates
    {
        #region Favorites Template

        public const String XMLGetFavoritesTemplate = "favorites";
        public const String XMLDeleteFavoriteTemplate = "favorites/{id}";
        public const String XMLAddToFavoritesTemplate = "favorites/add?title={title}&folderPath={folderPath}";

        #endregion


        #region Folder Template

        public const String XMLCreateNewFolderTemplate = "folders/{parentFolderID}/create?title={title}";
        public const String XMLSubFoldersTemplate = "folders/{id}/subfolders?from={from}&count={count}";
        public const String XMLFolderRenameTemplate = "folders/{id}/rename?title={title}";
        public const String XMLGetFolderTemplate = "folders/{id}";

        public const String XMLGetFolderItemsTemplate = "folders/{id}?from={from}&count={count}&filter={filter}&subjectID={subjectID}&setThumbnailURLBool={setThumbnailURLBool}";

        #endregion


        #region File Template

        public const String XMLCreateNewFileTemplate = "folders/{folderID}/files/createfile?title={fileTitle}";
        public const String XMLFileRenameTemplate = "folders/files/{id}/rename?title={title}";
        public const String XMLGetFileHistoryTemplate = "folders/files/{id}/history";
        public const String XMLDeleteItemsTemplate = "folders/files?action=delete";
        public const String XMLGetFileStreamForEditingTemplate = "folders/files";
        public const String XMLUploadFileTemplate = "folders/files";
        public const String XMLGetSiblingsFileTemplate = "folders/files/{fileID}/siblings";
        public const String XMLUpdateToVersionTemplate = "folders/files/{id}/updateToVersion?version={version}";
        public const String JSONCheckMoveFilesTemplate = "folders/files/{destFolderID}/moveOrCopyFilesCheck";
        public const String XMLLastFileVersionTemplate = "folders/files/{fileID}/lastversion?setThumbnailURLBool={setThumbnailURLBool}";

        #endregion


        #region Utils Template

        public const String XMLGetGoogleDocsTemplate = "import?source=google&token={token}";
        public const String XMLGetBoxNetDocsTemplate = "import?source=boxnet&token={token}";
        public const String XMLGetZohoDocsTemplate = "import?source=zoho&login={login}&pass={pass}";
        public const String XMLIsZohoAuthentificatedTemplate = "import/isauth?source=zoho&login={login}&pass={pass}";
        public const String XMLExecGoogleDocsImportTemplate = "import/exec?source=google&token={token}&tofolder={tofolder}&ignoreCoincidenceFiles={ignoreCoincidenceFiles}";
        public const String XMLExecBoxNetDocsImportTemplate = "import/exec?source=boxnet&token={token}&tofolder={tofolder}&ignoreCoincidenceFiles={ignoreCoincidenceFiles}";
        public const String XMLExecGetZohoDocsImportTemplate = "import/exec?source=zoho&login={login}&pass={pass}&tofolder={tofolder}&ignoreCoincidenceFiles={ignoreCoincidenceFiles}";
        public const String JSONGetTasksStatusesTemplate = "tasks/statuses";
        public const String JSONBulkDownloadTemplate = "bulkdownload";
        public const String JSONTerminateTasks = "tasks?terminate";
        public const String XMLMakeEditFileTemplate = "makeeditfile?fileID={fileID}";
        public const String XMLTrackEditFileTemplate = "trackeditfile?fileID={fileID}&docKey={docKey}&isFinish={isFinish}";
        public const String JSONMoveItemsTemplate = "moveorcopy?dfID={destFolderID}&ow={overwriteFiles}&ic={isCopyOperation}";
        public const String XMLEmptyTrashTemplate = "emptytrash";
        #endregion
        

        #region Group Tempate

        public const String XMLGetGroupsTemplate = "groups";
        public const String XMLSaveGroupTemplate = "groups/{id}?name={name}";
        public const String XMLDeleteGroupTemplate = "groups/{id}";
        public const String XMLGetUsersByGroupTemplate = "groups/{id}/users";

        public const String XMLGetSharedInfoTemplate = "sharedinfo";
        public const String XMLSetAceObjectTemplate = "setaceobject";
        public const String XMLRemoveAceTemplate = "removeace/{id}";

        #endregion
       
    }
}