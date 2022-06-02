using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading;
using ASC.Common.Security.Authorizing;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using log4net;

namespace ASC.Web.Files.Services.WCFService
{
    abstract class FileOperation : IProgressItem
    {
        private IPrincipal principal;
        private double step;
        private int processed;

        public object Id
        {
            get;
            set;
        }

        public bool IsCompleted
        {
            get;
            set;
        }

        public double Percentage
        {
            get;
            set;
        }

        public object Status
        {
            get;
            set;
        }

        public object Error
        {
            get;
            set;
        }

        public object Source
        {
            get;
            set;
        }

        public Guid Owner
        {
            get;
            private set;
        }


        protected List<int> Folders
        {
            get;
            private set;
        }

        protected List<int> Files
        {
            get;
            private set;
        }

        protected Tenant CurrentTenant
        {
            get;
            private set;
        }

        protected FileSecurity FilesSecurity
        {
            get;
            private set;
        }

        protected IFolderDao FolderDao
        {
            get;
            private set;
        }

        protected IFileDao FileDao
        {
            get;
            private set;
        }

        protected ITagDao TagDao
        {
            get;
            private set;
        }

        protected IDataStore Store
        {
            get;
            private set;
        }

        protected IDataStore StoreTmp
        {
            get;
            private set;
        }

        protected bool Canceled
        {
            get;
            private set;
        }


        protected FileOperation(Tenant tenant, List<int> folders, List<int> files)
        {
            if (tenant == null) throw new ArgumentNullException("tenant");

            Id = Guid.NewGuid().ToString();
            this.CurrentTenant = tenant;
            Owner = ASC.Core.SecurityContext.CurrentAccount.ID;
            principal = Thread.CurrentPrincipal;

            Folders = folders ?? new List<int>();
            Files = files ?? new List<int>();
            Source = string.Join(" ", Folders.Select(f => "folder_" + f).Concat(Files.Select(f => "file_" + f)).ToArray());
        }

        public void RunJob()
        {
            IPrincipal oldPrincipal = null;
            try
            {
                oldPrincipal = Thread.CurrentPrincipal;
            }
            catch { }
            try
            {
                if (principal != null)
                {
                    Thread.CurrentPrincipal = principal;
                }
                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);
                FolderDao = Global.DaoFactory.GetFolderDao();
                FileDao = Global.DaoFactory.GetFileDao();
                TagDao = Global.DaoFactory.GetTagDao();
                FilesSecurity = new FileSecurity(Global.DaoFactory);
                Store = Global.GetStore();
                StoreTmp = Global.GetStoreTmp();

                try
                {
                    step = InitProgressStep();
                }
                catch { }

                Do();
            }
            catch (AuthorizingException authError)
            {
                Error = new SecurityException(FilesCommonResource.ErrorMassage_SecurityException, authError);
            }
            catch (Exception error)
            {
                Error = error;
                LogManager.GetLogger("ASC.Files").Error(error);
            }
            finally
            {
                IsCompleted = true;
                Percentage = 100;
                try
                {
                    if (oldPrincipal != null) Thread.CurrentPrincipal = oldPrincipal;
                    FolderDao.Dispose();
                    FileDao.Dispose();
                    TagDao.Dispose();
                }
                catch { }
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var op = obj as FileOperation;
            return op != null && op.Id == Id;
        }


        public FileOperationResult GetResult()
        {
            var r = new FileOperationResult()
            {
                Id = this.Id.ToString(),
                OperationType = this.OperationType,
                Progress = (int)this.Percentage,
                Source = this.Source != null ? this.Source.ToString().Trim() : null,
                Result = this.Status != null ? this.Status.ToString().Trim() : null,
                Error = this.Error != null ? Error.ToString() : null,
                Processed = processed.ToString(),
            };
#if !DEBUG
            var error = Error as Exception;
            if (error != null)
            {
                if (error is System.IO.IOException)
                {
                    r.Error = FilesCommonResource.ErrorMassage_FileNotFound;
                }
                else
                {
                    r.Error = error.Message;
                }
            }
#endif
            return r;
        }

        public void Terminate()
        {
            Canceled = true;
        }


        protected virtual double InitProgressStep()
        {
            var count = Files.Count;
            Folders.ForEach(f => count += FolderDao.GetItemsCount(f, FilterType.None, null, true));
            return count == 0 ? 100d : 100d / count;
        }

        protected void ProgressStep()
        {
            Percentage = Percentage + step < 100d ? Percentage + step : 99d;
        }

        protected void ProcessedFolder(int id)
        {
            processed++;
            if (Folders.Contains(id))
            {
                Status += string.Format("folder_{0} ", id);
            }
        }

        protected void ProcessedFile(int id)
        {
            processed++;
            if (Files.Contains(id))
            {
                Status += string.Format("file_{0} ", id);
            }
        }


        protected abstract FileOperationType OperationType
        {
            get;
        }

        protected abstract void Do();
    }
}