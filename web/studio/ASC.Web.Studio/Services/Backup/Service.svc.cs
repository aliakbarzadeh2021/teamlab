using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Threading;
using System.Web;
using ASC.Core;
using ASC.Data.Backup;
using ASC.Data.Storage;
using ASC.Web.Core;
using ASC.Web.Studio.Core.Notify;
using Microsoft.ServiceModel.Web;

// The following line sets the default namespace for DataContract serialized typed to be ""
[assembly: ContractNamespace("", ClrNamespace = "ASC.Web.Studio.Services.Backup")]
namespace ASC.Web.Studio.Services.Backup
{
    class ServiceHostFactory : WebServiceHost2Factory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var service = new WebServiceHost2(serviceType, true, baseAddresses);
            return service;
        }
    }


    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class Service : IBackupService, IDisposable
    {
        private List<BackupRequest> _backups = new List<BackupRequest>();
        private object synchRoot = new object();
        private Timer threadTimer;
        private TimeSpan timerPeriod = TimeSpan.FromDays(1);


        public Service()
        {
            //Create clean timer
            threadTimer = new Timer(CleanTimer);
            threadTimer.Change(timerPeriod, timerPeriod);
        }

        private void CleanTimer(object state)
        {
            CleanOldFiles();
        }

        protected void CheckPermission()
        {
            if (!SecurityContext.IsAuthenticated)
            {
                try
                {
                    if (!SecurityContext.AuthenticateMe(CookiesManager.GetCookies(CookiesType.AuthKey)))
                    {
                        throw GenerateException(HttpStatusCode.Unauthorized, "Unauthorized", null);
                    }
                    else
                    {
                        //Check admin
                        if (!CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID))
                        {
                            throw GenerateException(HttpStatusCode.Unauthorized, "Permission denied", null);
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw GenerateException(HttpStatusCode.Unauthorized, "Unauthorized", exception);
                }
            }
        }

        protected WebProtocolException GenerateException(HttpStatusCode code, string message, Exception inner)
        {
            string error="";
            while (inner != null)
            {
                error = string.Format("{0}\r\n", inner.Message) + error;
                inner = inner.InnerException;
            }
            return new WebProtocolException(code, message,error , inner);
        }

        protected WebProtocolException GenerateException(Exception inner)
        {
            return GenerateException(HttpStatusCode.BadRequest, inner);
        }

        protected WebProtocolException GenerateException(HttpStatusCode code, Exception inner)
        {
            return GenerateException(code, "Bad request", inner);
        }

        private int TenantId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        private Guid Username
        {
            get { return SecurityContext.CurrentAccount.ID; }
        }

        public ItemList<BackupRequest> GetBackupList()
        {
            try
            {
                CheckPermission();
                lock (synchRoot)
                {
                    return new ItemList<BackupRequest>(_backups.Where(x => x.TennantId == TenantId && x.UserId == Username));
                }
            }
            catch (WebProtocolException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }


        public BackupRequest RequestBackup()
        {
            try
            {
                CheckPermission();
                var request = GetRunningBackup();
                if (request == null)
                {
                    request = new BackupRequest();
                    request.Id = Guid.NewGuid().ToString("N");
                    request.Created = DateTime.UtcNow;
                    request.Availible = DateTime.UtcNow.AddDays(1);
                    request.TennantId = TenantId;
                    request.Percentdone = 0;
                    request.Status = BackupRequestStatus.Started;
                    request.UserId = Username;
                    request.BackupFile = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data/"), request.Id + ".tbm");
                    lock (synchRoot)
                    {
                        _backups.Add(request);
                    }
                    ProcessRequest(request);
                }
                return request;
            }
            catch (WebProtocolException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        private void ProcessRequest(BackupRequest request)
        {
            //Start a thread
            Thread thread = new Thread(RunBackup){Name = "backup"};
            thread.Start(request);
        }

        private void RunBackup(object obj)
        {
            var request = (BackupRequest)obj;
            DoBackup(request);
        }

        private void DoBackup(BackupRequest request)
        {
            try
            {
                if (request == null) throw new ArgumentNullException("request is null");
                var backuper = new BackupManager(request.BackupFile);
                backuper.ProgressChanged += new EventHandler<ProgressChangedEventArgs>(
                    (o, e) => { request.Percentdone = Math.Max(0,Math.Min((int)e.Progress/2,50)); }
                );
                backuper.ProgressError += new EventHandler<ErrorEventArgs>(
                    (o, e) =>
                    {
                        request.Status = BackupRequestStatus.Error;
                        GenerateError(request, e.GetException());
                    }
                );
                request.Status = BackupRequestStatus.Working;
                backuper.Save(request.TennantId);

                if (request.Status != BackupRequestStatus.Error)
                {
                    try
                    {
                        using (var fstream = new FileStream(request.BackupFile, FileMode.Open))
                        {
                            request.Status = BackupRequestStatus.Uploading;
                            request.Size = fstream.Length;
                            using (var progressStream = new ProgressStream(fstream))
                            {
                                progressStream.OnReadProgress += (o, e) => { request.Percentdone =Math.Max(0,Math.Min(100, 50+ e/2)); };
                                request.FileLink = Uploader
                                    .UploadBackupFile(progressStream, DateTime.UtcNow.AddDays(1), request.UserId,
                                                      request.TennantId);
                            }
                        }
                        request.Status = BackupRequestStatus.Done;
                        request.Availible = DateTime.UtcNow.AddDays(1);
                        request.Completed = true;
                        request.Percentdone = 100;
                        try
                        {
                            CoreContext.TenantManager.SetCurrentTenant(request.TennantId);
                            StudioNotifyService.Instance.SendMsgBackupCompleted(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID), request.FileLink, request.Availible);
                        }
                        catch (Exception e)
                        {
                            GenerateError(request, e);
                        }
                    }
                    catch (Exception e)
                    {
                        request.Status = BackupRequestStatus.Error;
                        GenerateError(request, e);
                    }
                }
            }
            catch (Exception e)
            {
                if (request != null)
                {
                    request.Status = BackupRequestStatus.Error;
                    GenerateError(request, e);
                }
            }
            finally
            {
                if (request != null)
                    if (!string.IsNullOrEmpty(request.BackupFile) && File.Exists(request.BackupFile))
                    {
                        File.Delete(request.BackupFile);
                    }
            }
        }

        private void GenerateError(BackupRequest request, Exception e)
        {
            if (request != null)
            {
                var inner = e;
                while (inner != null)
                {
                    request.Info = string.Format("{0}\r\n", inner.Message) + request.Info;
                    inner = inner.InnerException;
                }
            }
        }

        private void CleanOldFiles()
        {
            //Remove backups
            lock (synchRoot)
            {
                foreach (var backupRequest in new List<BackupRequest>(_backups))
                {
                    if (backupRequest.Availible < DateTime.UtcNow)
                    {
                        _backups.Remove(backupRequest);
                    }
                }
            }
            //This will clean all expired files even if service was restarted
            Uploader.CleanOldFiles(TimeSpan.FromDays(1));
        }

        public BackupRequest GetBackupStatus(string id)
        {
            try
            {
                CheckPermission();
                BackupRequest backup = GetBackupById(id);

                if (backup == null)
                {
                    throw GenerateException(HttpStatusCode.NotFound,"Not found", new Exception("item not found"));
                }
                return backup;


            }
            catch (WebProtocolException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw GenerateException(e);
            }
        }

        private BackupRequest GetBackupById(string id)
        {
            lock (synchRoot)
            {
                return _backups.Where(
                    x =>
                    x.TennantId == TenantId && x.UserId == Username &&
                    x.Id.Equals(id, StringComparison.InvariantCulture)).SingleOrDefault();
            }
        }

        private BackupRequest GetRunningBackup()
        {
            lock (synchRoot)
            {
                return _backups.Where(
                     x =>
                     x.TennantId == TenantId && x.UserId == Username && (x.Status == BackupRequestStatus.Started || x.Status == BackupRequestStatus.Working))
                     .SingleOrDefault();
            }
        }

        public void Dispose()
        {
            threadTimer.Dispose();
        }
    }
}