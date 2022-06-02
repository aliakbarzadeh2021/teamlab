#if DEBUG
namespace ASC.Data.Storage.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using ASC.Data.Storage;
    using NUnit.Framework;

    [TestFixture]
    public class DataStorageTest
    {
        private readonly IDataStore store;
        private const string defaultmodule = "forum";
        private const string defauldomain = "forum";
        private const string defaultfile = "test.txt";


        public DataStorageTest()
        {
            store = StorageFactory.GetStorage(null, 23.ToString(), defaultmodule, null, null);
        }


        private Stream GetDataStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes("unit test generated file"));
        }

        [Test]
        public void SslLinkGeneration()
        {
            var uri = StorageFactory.GetStorage(null, 23.ToString(), "photo", null, null).Save("", defaultfile, GetDataStream());
        }

        [Test]
        public void TestFile()
        {
            var uri = store.Save(defauldomain, defaultfile, GetDataStream());
            var files = store.ListFiles(defauldomain, "", "*.*", true);
            Assert.IsNotNull(files);
            Assert.IsNotNull(files.Where(x => x.ToString().Equals(uri.ToString())).SingleOrDefault());
            var size = store.GetFileSize(defauldomain, defaultfile);
            Assert.AreEqual(size, GetDataStream().Length);
            var moved = store.Move(defauldomain, defaultfile, "", "testmoved.txt");
            files = store.ListFiles("", "testmoved.txt", "*.*", true);
            Assert.IsNotNull(files);
            Assert.IsNotNull(files.Where(x => x.ToString().Equals(moved.ToString())).SingleOrDefault());

            store.Delete("", "testmoved.txt");
            files = store.ListFiles(defauldomain, "", "*.*", true);
            Assert.IsNotNull(files);
            Assert.IsNull(files.Where(x => x.ToString().Equals(uri.ToString())).SingleOrDefault());
        }


        [Test]
        public void Test2()
        {
            var storage = StorageFactory.GetStorage("0", "fckuploaders");
            var list = storage.ListFiles("forum", "40105221-fb0c-4943-bccd-baa635a016f7/", "*.*", true);
            var listRel = storage.ListFilesRelative("forum", "40105221-fb0c-4943-bccd-baa635a016f7/", "*.*", true);
            storage.DeleteFiles("forum", "40105221-fb0c-4943-bccd-baa635a016f7/", "*.*", true);
        }
    }
}
#endif