using System;

namespace ASC.Files.Core
{
    public static class FileConst
    {
        public static readonly string StorageModule = "files";

        public static readonly string StorageModuleTmp = "files_temp";

        public static readonly string DatabaseId = "files";

        public static readonly string UrlFileHandler = "~/products/files/filehandler.ashx";

        public static readonly string ParamsDownload = "?action=download&fileID={0}&version={1}";

        public static readonly string ParamsView = "?action=view&fileID={0}&version={1}";

        public static readonly string ParamsUpload = "?action=upload&folderID={0}&fileTitle={1}";

        public static readonly string ParamsSave = "?action=save&fileID={0}&version={1}&fileUri={2}";

        public static readonly string ParamsTrackActivity = "?action=useractivity&activityType={0}";


        public static readonly Guid GroupCategoryId = new Guid("{FFFF85E1-2704-4cf6-A1D7-6D29141248B9}");
    }
}
