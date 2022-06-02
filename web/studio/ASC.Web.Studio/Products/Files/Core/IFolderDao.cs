using System;
using System.Collections.Generic;

namespace ASC.Files.Core
{
    public interface IFolderDao : IDisposable
    {
        /// <summary>
        ///     Получение списка  папок.
        /// </summary>
        List<Folder> GetFolders(int parentId, int from, int count, OrderBy orderBy);

        /// <summary>
        ///     Получение списка  папок.
        /// </summary>
        List<Folder> GetFolders(int[] ids);

        /// <summary>
        ///     Получает файл по идентификатору
        /// </summary>
        /// <param name="id">идентификатор папки</param>
        /// <returns>Файл по индентификатору</returns>
        Folder GetFolder(int id);

        /// <summary>
        ///     Проверяет существует или нет папка
        /// </summary>
        /// <param name="id">идентификатор папки</param>
        /// <returns>true - если такой файл существует, в противном случае false</returns>
        bool IsExist(int id);

        /// <summary>
        ///     Возвращает папку с заданным именем и индентифакатором корневой папки
        /// </summary>
        /// <param name="title"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        Folder GetFolder(String title, int parentId);

        IEnumerable<Folder> Search(string text, FolderType folderType);


        /// <summary>
        ///    Получает корневую папку
        /// </summary>
        /// <param name="folderID">Идентификатор папки</param>
        /// <returns>Идентификатор корневой папки</returns>
        Folder GetRootFolder(int folderID);

        /// <summary>
        ///    Получает корневую папку
        /// </summary>
        /// <param name="fileID">Идентификатор файлы</param>
        /// <returns>Идентификатор корневой папки</returns>
        Folder GetRootFolderByFile(int fileID);


        int GetFolderID(string module, string bunch, string data, bool createIfNotExists);

        /// <summary>
        ///  Возвращает индентификатор папки "Общие документы"
        /// </summary>
        /// <returns></returns>
        int GetFolderIDCommon(bool createIfNotExists);

        /// <summary>
        ///  Возвращает индентификатор папки пользователя
        /// </summary>
        /// <returns></returns>
        int GetFolderIDUser(bool createIfNotExists);

        int GetFolderIDShare(bool createIfNotExists);

        /// <summary>
        /// Возвращает индентификатор папки "Корзина"
        /// </summary>
        /// <returns></returns>
        int GetFolderIDTrash(bool createIfNotExists);


        /// <summary>
        ///     Переименовывает папку
        /// </summary>
        /// <param name="folderID">Индентификатор папки</param>
        /// <param name="newTitle">Новое название папки</param>
        void RenameFolder(int id, String newTitle);

        /// <summary>
        ///     Сохраняет или обновляет папку
        /// </summary>
        /// <param name="folder">Инстанс папки</param>
        /// <returns></returns>
        int SaveFolder(Folder folder);

        /// <summary>
        ///     Удаляет папку
        /// </summary>
        /// <param name="folderID">идентификатор папки</param>
        void DeleteFolder(int id);
                
        /// <summary>
        ///  Перемещает папки в подпапку
        /// </summary>
        /// <param name="folderID">Индентификатор папки изменяемой папки</param>
        /// <param name="destFolderID">Индентификатор подпапки, куда перемещается основная папка</param>
        void MoveFolder(int folderId, int to);
    
        /// <summary>
        ///     Копировать папки в подпапку
        /// </summary>
        /// <param name="foldersID">Идентификатор</param>
        /// <param name="destFolderID"></param>
        /// <returns>
        /// Возвращает пары
        /// id старого файла 
        /// id нового файла
        /// </returns>
        int CopyFolder(int folderId, int to);

        /// <summary>
        /// Проверяет корректность операции переноса каталогов в другой каталог.
        /// </summary>
        /// <param name="filesID"></param>
        /// <param name="destFolderID"></param>
        /// <returns>
        /// Возвращает пары идентификатор файла, название файла, у которых совпадают названия.
        /// </returns>
        IDictionary<int, string> CanMoveOrCopy(int[] folders, int to);

        
        /// <summary>
        ///     Получает файлы в папке
        /// </summary>
        /// <param name="folderID">идентификатор папки</param>
        /// <param name="from"></param>
        /// <param name="count"></param>
        /// <param name="orderBy"></param>
        /// <param name="subjectID">Идентификатор отдела или пользователя</param>
        /// <param name="filterType">параметр фильтрации</param>
        /// <returns>Возвращает список файлов</returns>
        /// <remarks>
        ///    Возвращает только последние версии фалов в папке
        /// </remarks>
        List<File> GetFiles(int folderID, int from, int count, OrderBy orderBy, FilterType filterType, Guid? subjectID);

        List<int> GetFiles(int folderId, bool withSubfolders);

        /// <summary>
        ///    Получает кол-во файлов и папок для данного контейнера в папке 
        /// </summary>
        /// <param name="folderID">идентификатор папки</param>
        /// <param name="enumFilterTypeStr">параметр фильтрации</param>
        /// <param name="subjectID">Идентификатор отдела или пользователя</param>
        /// <param name="includeSubFolder">Учитывать в подсчете вложенные папки или нет</param>
        /// <returns></returns>
        int GetItemsCount(int folderID, FilterType enumFilterTypeStr, Guid? subjectID, bool includeSubFolder);

        
        /// <summary>
        ///     
        /// </summary>
        /// <param name="folderID">Индентификатор папки</param>
        /// <returns>
        /// Возвращает пары:
        /// {
        ///   id - Индентификатор папки,
        ///   value - название папки
        /// }
        /// </returns>
        List<Folder> GetParentFolders(int folderId);
    }
}
