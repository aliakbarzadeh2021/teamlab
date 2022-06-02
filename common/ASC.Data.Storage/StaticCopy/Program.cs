using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using ASC.Common.Web;

namespace StaticCopy
{
    class UploadFile
    {
        public string FileName { get; set; }
        public string AmazonKey { get; set; }
    }

    class Program
    {
        private static string bucketname;
        private static string acesskey;
        private static string secretkey;
        private static string version;
        private static bool keepCasing;


        static void Main(string[] args)
        {
            var extensions = new List<string>(Settings.Default.extensions.Split('|').Select(x => x.Trim()));
            bucketname = Settings.Default.bucket;
            acesskey = Settings.Default.acesskey;
            secretkey = Settings.Default.secretkey;
            version = Settings.Default.version;
            keepCasing = Settings.Default.keepcasing;
            try
            {
                var srcDir = Settings.Default.dir;
                if (args.Length > 0)
                {
                    srcDir = args[0];
                }
                var srcFiles = Directory.GetFiles(Path.Combine(srcDir, Settings.Default.subdir), "*", SearchOption.AllDirectories)
                    .Where((x) => extensions.Contains(Path.GetExtension(x))).ToArray();
                Console.WriteLine("{0} files found.", srcFiles.Length);
                UploadToAmazon(srcFiles, srcDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static ManualResetEvent evt = new ManualResetEvent(false);

        private static List<S3Object> GetS3Objects(AmazonS3 client, string bucket, string prefix)
        {
            var request = new ListObjectsRequest().WithBucketName(bucket).WithPrefix(prefix);
            request.WithMaxKeys(1000);
            var objects = new List<S3Object>();
            ListObjectsResponse response = null;
            do
            {
                response = client.ListObjects(request);
                response.S3Objects.ForEach(entry => objects.Add(entry));
                if (objects.Count == 0) return objects;
                request.Marker = objects[objects.Count - 1].Key;

            } while (response.IsTruncated);
            return objects;
        }

        private static void UploadToAmazon(string[] srcfiles, string srcDir)
        {
            var cfg = new AmazonS3Config() { CommunicationProtocol = Amazon.S3.Model.Protocol.HTTP, MaxErrorRetry = 3 };
            var client = AWSClientFactory.CreateAmazonS3Client(acesskey, secretkey, cfg);
            //Create bucket if no availible
            Console.WriteLine("Quering bucket");
            var bucket = client.ListBuckets().Buckets.SingleOrDefault((x) => x.BucketName == bucketname);
            if (bucket == null)
            {
                Console.WriteLine("Bucket creating");
                client.PutBucket(new PutBucketRequest().WithBucketName(bucketname));
            }

            //List
            var existingkeys = new Dictionary<string, long>();
            Console.WriteLine("Quering bucket objects");
            var s3objs = GetS3Objects(client, bucketname, version);
            Console.WriteLine("total {0} objects", s3objs.Count);

            foreach (S3Object entry in s3objs)
            {
                existingkeys.Add(entry.Key, entry.Size);
            }

            var filesToUpload = new Queue<UploadFile>(srcfiles.Select(x => new UploadFile() { FileName = x, AmazonKey = GetAmazonKey(x, srcDir) }));
            var invalidates = new List<string>();

            while (filesToUpload.Count > 0)
            {
                UploadFile file = filesToUpload.Dequeue();
                var finfo = new FileInfo(file.FileName);
                if (existingkeys.ContainsKey(file.AmazonKey) && existingkeys[file.AmazonKey] == finfo.Length)
                {
                    Console.WriteLine("Skipping {0}", file.AmazonKey);
                    continue;//Don't put it
                }
                Console.WriteLine("Sending {0} size {1}", Path.GetFileName(file.AmazonKey),
                                                                   finfo.Length);
                var request = new PutObjectRequest();


                request.WithBucketName(bucketname)
                    .WithKey(file.AmazonKey)
                    .WithCannedACL(S3CannedACL.PublicRead)
                    .WithContentType(MimeMapping.GetMimeMapping(Path.GetFileName(file.FileName)))
                    .WithInputStream(File.OpenRead(file.FileName));

                var headers = new NameValueCollection();
                headers["Cache-Control"] = string.Format("public, maxage={0}", (int)TimeSpan.FromDays(365).TotalSeconds);
                headers["Etag"] = (finfo.Length + finfo.LastWriteTimeUtc.Ticks).ToString();
                headers["Last-Modified"] = DateTime.UtcNow.ToString("R");
                headers["Expires"] = DateTime.UtcNow.Add(TimeSpan.FromDays(365)).ToString("R");
                request.AddHeaders(headers);

                invalidates.Add(file.AmazonKey);

                try
                {
                    using (client.PutObject(request))
                    {
                        Console.WriteLine("Sent {0} OK", Path.GetFileName(file.AmazonKey));
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Fail: {0}", file.AmazonKey);
                    filesToUpload.Enqueue(file);
                }
            }

            //invalidate cloud front
            if (0 < invalidates.Count)
            {
                using (var cloudFront = AWSClientFactory.CreateAmazonCloudFrontClient(acesskey, secretkey))
                {
                    var distribution = cloudFront.ListDistributions().Distribution.Where(d => d.DistributionConfig.S3Origin.DNSName == bucketname + ".s3.amazonaws.com").FirstOrDefault();
                    if (distribution != null)
                    {
                        var request = new PostInvalidationRequest(new InvalidationBatch(Guid.NewGuid().ToString(), invalidates))
                            .WithDistribtionId(distribution.Id);
                        cloudFront.PostInvalidation(request);
                    }
                }
            }
        }

        private static string GetAmazonKey(string file, string srcDir)
        {
            var key = version + file.Substring(srcDir.Length).Replace('\\', '/');
            return keepCasing ? key : key.ToLowerInvariant();
        }
    }
}
