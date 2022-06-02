using System;
using System.Collections.Generic;

namespace ASC.Files.Core
{
    /// <summary>
    ///    Интерфейс инкапсулирующий доступ к файлам
    /// </summary>
    public interface IFileDao : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        Dictionary<File, String> Search(String text, FolderType folderType);


        String GetUniqFileDirectory(int fileID);

        /// <summary>
        ///     Получает полный путь к файлу в цифровом виде (через id)
        /// </summary>
        /// <param name="fileID">Идентификатор файла</param>
        /// <param name="version">Номер версии</param>
        /// <returns></returns>
        String GetUniqFilePath(File file);


        /// <summary>
        ///     Получает файл(-ы) по идентификатору(-ам)
        /// </summary>
        /// <param name="fileID">Идентификатор файла</param>
        /// <returns></returns>
        List<File> GetFiles(int[] fileID);

        /// <summary>
        ///     Проверяет существует или нет файл
        /// </summary>
        /// <param name="title">Название файла</param>
        /// <param name="folderID">Идентификатор папки</param>
        /// <returns>Возвращает true если такой файл существует, в противном случае false</returns>
        bool IsExist(String title, int folderID);

        /// <summary>
        ///   Проверяет существует или нет файл
        /// </summary>
        /// <param name="fileID">Идентификатор файла</param>
        /// <param name="fileVersion">Версия файла</param>
        /// <returns></returns>
        bool IsExist(int fileID, int fileVersion);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentFolderID">идентификатор папки</param>
        /// <param name="title">Название файла</param>
        /// <returns>
        ///   Инстанс на файл
        /// </returns>
        File GetFile(int parentFolderID, String title);

        /// <summary>
        ///     Получение файла
        /// </summary>
        /// <param name="fileID">Идентификатор файла</param>
        /// <param name="fileVersion">Версия файла</param>
        /// <returns></returns>
        File GetFile(int fileID, int fileVersion);


        /// <summary>
        ///     Получает по индентификатору текущую версию файла
        /// </summary>
        /// <param name="fileID">Идентификатор файла</param>
        /// <returns></returns>
        File GetFile(int fileID);

        /// <summary>
        ///  Возвращает все версии файла
        /// </summary>
        /// <param name="fileID"></param>
        /// <returns></returns>
        List<File> GetFileHistory(int fileID);


        /// <summary>
        ///  Сохраняет/обновляет версию файла 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <remarks>
        /// Обновляет файл, если:
        /// - файл приходит с заданным id 
        /// - файл с таким именем в данной папке/контейнере существует
        /// 
        /// сохранет во всех остальных случаях
        /// </remarks>
        File SaveFile(File file);

        /// <summary>
        ///   Перемещает файл или набор файлов в папку
        /// </summary>
        /// <param name="filesID">Идентификатор файлов</param>
        /// <param name="destFolderID">Идентификатор папки назначения</param>
        void MoveFile(int fileId, int to);

        /// <summary>
        ///  Копировать список файлов в папку
        /// </summary>
        /// <param name="filesID">Список файлов</param>
        /// <param name="destFolderID">Папка назначения</param>
        int CopyFile(int fileId, int to);

        /// <summary>
        ///   Переименовать файл
        /// </summary>
        /// <param name="fileID">Идентификатор файла</param>
        /// <param name="newTitle">Новое название</param>
        void FileRename(int fileID, String newTitle);

        /// <summary>
        ///   Удаляет файл включая все предыдущие версии
        /// </summary>
        /// <param name="filesID">Идентификатор файла</param>
        void DeleteFile(int id);
    }
}
