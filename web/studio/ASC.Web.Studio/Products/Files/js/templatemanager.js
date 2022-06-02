window.ASC.Files.TemplateManager = (function($) {
    var 
    isInit = false,
    templatesDirPath = "",
    tempatesHandlerPath = "",

    xslTemplates = {},

    supportedCustomEvents = {
        CreateNewFile: 'createnewfile',
        CreateNewFilePlugin: 'createnewfileplugin',
        CreateFolder: 'createfolder',
        
        CheckEditFile: 'checkeditfile',

        GetTreeSubFolders: 'gettreesubfolders',
        GetFolderInfo: 'getfolderinfo',
        GetFolderItems: 'getfolderitems',

//        GetGroups: 'getgroups',
//        GetUsersByGroup: 'getusersbygroup',
//        SaveGroup: 'savegroup',
//        DeleteGroup: 'deletegroup',

        GetSharedInfo: 'getsharedinfo',
        SetAceObject: 'setaceobject',
        UnSubscribeMe: 'unsubscribeme',

        GetNewItems: 'getnewitems',

        FolderRename: 'folderrename',
        FileRename: 'filerename',

        DeleteItem: 'deleteitem',
        EmptyTrash: 'emptytrash',

        GetFileHistory: 'getfilehistory',
        GetFile: 'getFile',
        ReplaceVersion: 'replaceversion',
        SetCurrentVersion: 'setcurrentversion',

        GetFavorites: 'getfavorites',
        SaveFavorite: 'savefavorite',
        RemoveFavorite: 'removefavorite',

        IsZohoAuthentificated: 'iszohoauthentificated',
        GetImportData: 'getImportData',
        ExecImportData: 'execImportData',
        GetImportStatus: 'getimportstatus',

        Download: 'download',
        GetTasksStatuses: 'getTasksStatuses',
        TerminateTasks: 'terminatetasks',

        MoveFilesCheck: 'movefilescheck',
        MoveItems: 'moveitems',

        GetSiblingsFile: 'getsiblingsfile'
    },

    xslTemplatesName = {
        getNewInShare: 'getchangedfile',
        getSharedInfo: 'getsharedinfo',
        getGroups: 'getgroups',
        addGroup: 'addgroup',
        getFavorite: 'getfavorite',
        getFileHistory: 'getfilehistory',
        getFoldersTree: 'getfolderstree',
        getFolderInfo: 'getfolderinfo',
        getFolderItems: 'getfolderitems',
        getFolderItems_thumb: 'getfolderitems_thumb',
        getFolderItem: 'getfolderitem',
        getFolderItem_thumb: 'getfolderitem_thumb',
        getImportData: 'getimportdata'
    };

    var init = function(templatesHandler, templatesDir) {
        if (isInit === false) {
            isInit = true;

            templatesDirPath = templatesDir;
            tempatesHandlerPath = templatesHandler;
        }
    };

    var getTemplate = function(name) {
        if (typeof name !== 'string' || name.length === 0) {
            return undefined;
        }
        if (xslTemplates.hasOwnProperty(name)) {
            return xslTemplates[name];
        }
        var xslTemplate = ASC.Controls.XSLTManager.loadXML(tempatesHandlerPath + '?id=' + templatesDirPath + "&name=" + name);
        if (xslTemplate && typeof xslTemplate === "object") {
            xslTemplates[name] = xslTemplate;
            return xslTemplates[name];
        }
        return undefined;
    };

    return {
        events: supportedCustomEvents,
        templates: xslTemplatesName,

        getTemplate: getTemplate,

        init: init
    };
})(jQuery);