//-----------------------------------------------------------------------
// <copyright file="TfsSource.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.VisualStudio
{
    using System;
    using System.Globalization;
    using System.IO;
    using Microsoft.Build.Framework;

    /// <summary>
    /// <b>Valid TaskActions are:</b>
    /// <para><i>Add</i> (<b>Required: </b>ItemPath or ItemCol <b>Optional: </b>Server, Version, WorkingDirectory, Recursive <b>Output:</b> ExitCode)</para>
    /// <para><i>Checkin</i> (<b>Required: </b>ItemPath or ItemCol <b>Optional: </b>Server, Comments, Notes, Version, WorkingDirectory, Recursive <b>Output:</b> ExitCode)</para>
    /// <para><i>Checkout</i> (<b>Required: </b>ItemPath or ItemCol <b>Optional: </b>Server, Version, WorkingDirectory, Recursive <b>Output:</b> ExitCode)</para>
    /// <para><i>Delete</i> (<b>Required: </b>ItemPath or ItemCol <b>Optional: </b>Server, Version, WorkingDirectory, Recursive <b>Output:</b> ExitCode)</para>
    /// <para><i>Get</i> (<b>Required: </b>ItemPath or ItemCol <b>Optional: </b>Server, Version, WorkingDirectory, Recursive, Force, Overwrite, All <b>Output:</b> ExitCode)</para>
    /// <para><i>Merge</i> (<b>Required: </b>ItemPath, Destination <b>Optional: </b>Server, Recursive, VersionSpec, Version, Baseless, Force <b>Output:</b> ExitCode)</para>
    /// <para><i>GetPendingChanges</i> (<b>Required: </b>ItemPath <b>Optional: </b>Server, Recursive, Version <b>Output: </b>PendingChanges, PendingChangesExist <b>Output:</b> ExitCode)</para>
    /// <para><i>UndoCheckout</i> (<b>Required: </b>ItemPath or ItemCol <b>Optional: </b>Server, Version, WorkingDirectory, Recursive <b>Output:</b> ExitCode)</para>
    /// <para><i>Undelete</i> (<b>Required: </b>ItemPath or ItemCol <b>Optional: </b>Server, Version, WorkingDirectory, Recursive <b>Output:</b> ExitCode)</para>
    /// <para><b>Remote Execution Support:</b> NA</para>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Project ToolsVersion="3.5" DefaultTargets="Default" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    ///     <PropertyGroup>
    ///         <TPath>$(MSBuildProjectDirectory)\..\MSBuild.ExtensionPack.tasks</TPath>
    ///         <TPath Condition="Exists('$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks')">$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks</TPath>
    ///     </PropertyGroup>
    ///     <Import Project="$(TPath)"/>
    ///     <ItemGroup>
    ///         <FilesToAdd Include="C:\Projects\SpeedCMMI\Demo1\*"/>
    ///     </ItemGroup>
    ///     <Target Name="Default">
    ///         <!-- Check for pending changes -->
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="GetPendingChanges" ItemPath="$/AProject/APath" WorkingDirectory="C:\Projects\SpeedCMMI">
    ///             <Output TaskParameter="PendingChanges" PropertyName="PendingChangesText" />
    ///             <Output TaskParameter="PendingChangesExist" PropertyName="DoChangesExist" />
    ///         </MSBuild.ExtensionPack.VisualStudio.TfsSource>
    ///         <Message Text="Pending Changes Report: $(PendingChangesText)"/>
    ///         <Message Text="Pending Changes Exist: $(DoChangesExist)"/>
    ///         <!-- Perfrom various other source control operations -->
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="Checkout" ItemPath="C:\projects\SpeedCMMI\Demo1" Version="2008" WorkingDirectory="C:\projects\SpeedCMMI"/>
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="Checkin" ItemPath="C:\projects\SpeedCMMI\Demo1" WorkingDirectory="C:\projects\SpeedCMMI" Comments="Testing" Notes="Code reviewer=buildrobot" OverrideText="Justdoit" />
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="Add" ItemPath="C:\projects\SpeedCMMI\Demo1" Version="2008" WorkingDirectory="C:\projects\SpeedCMMI"/>
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="Checkin" ItemCol="@(FilesToAdd)" WorkingDirectory="C:\projects\SpeedCMMI" ContinueOnError="true"/>
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="Checkout" ItemCol="@(FilesToAdd)" WorkingDirectory="C:\projects\SpeedCMMI"/>
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="UndoCheckout" ItemCol="@(FilesToAdd)" WorkingDirectory="C:\projects\SpeedCMMI"/>
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="Delete" ItemCol="@(FilesToAdd)" WorkingDirectory="C:\projects\SpeedCMMI"/>
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="Checkin" ItemPath="C:\projects\SpeedCMMI\Demo1" WorkingDirectory="C:\projects\SpeedCMMI" ContinueOnError="true"/>
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="Undelete" ItemCol="@(FilesToAdd)" WorkingDirectory="C:\projects\SpeedCMMI"/>
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="Checkin" ItemCol="@(FilesToAdd)" WorkingDirectory="C:\projects\SpeedCMMI" ContinueOnError="true"/>
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="Get" ItemPath="C:\Projects\SpeedCMMI\Demo1" WorkingDirectory="C:\projects\SpeedCMMI"/>
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="Merge" ItemPath="C:\Projects\SpeedCMMI\Client2" Destination="C:\Projects\SpeedCMMI\Client" WorkingDirectory="C:\projects\SpeedCMMI"/>
    ///         <MSBuild.ExtensionPack.VisualStudio.TfsSource TaskAction="Checkin" ItemPath="C:\projects\SpeedCMMI\Client" WorkingDirectory="C:\projects\SpeedCMMI" Comments="Testing" Notes="Code reviewer=buildrobot" OverrideText="Justdoit" />
    ///     </Target>
    /// </Project>
    /// ]]></code>    
    /// </example>
    [HelpUrl("http://www.msbuildextensionpack.com/help/3.5.4.0/html/773f774e-5791-9318-76e8-ba31ee077b2d.htm")]
    public class TfsSource : BaseTask
    {
        private const string AddTaskAction = "Add";
        private const string CheckinTaskAction = "Checkin";
        private const string CheckoutTaskAction = "Checkout";
        private const string DeleteTaskAction = "Delete";
        private const string GetTaskAction = "Get";
        private const string MergeTaskAction = "Merge";
        private const string GetPendingChangesTaskAction = "GetPendingChanges";
        private const string UndoCheckoutTaskAction = "UndoCheckout";
        private const string UndeleteTaskAction = "Undelete";
        
        private string teamFoundationExe;
        private string version = "2008";
        private bool recursive = true;
        private ShellWrapper shellWrapper;
        private string itemSpec = string.Empty;
        private string returnOutput;

        [DropdownValue(AddTaskAction)]
        [DropdownValue(CheckinTaskAction)]
        [DropdownValue(CheckoutTaskAction)]
        [DropdownValue(DeleteTaskAction)]
        [DropdownValue(GetTaskAction)]
        [DropdownValue(MergeTaskAction)]
        [DropdownValue(GetPendingChangesTaskAction)]
        [DropdownValue(UndoCheckoutTaskAction)]
        [DropdownValue(UndeleteTaskAction)]
        public override string TaskAction
        {
            get { return base.TaskAction; }
            set { base.TaskAction = value; }
        }

        /// <summary>
        /// Sets the version spec for Get
        /// </summary>
        [TaskAction(MergeTaskAction, false)]
        public string VersionSpec { get; set; }

        /// <summary>
        /// Forces all files to be retrieved, not just those that are out-of-date.
        /// </summary>
        [TaskAction(GetTaskAction, false)]
        public bool All { get; set; }

        /// <summary>
        /// Overwrites writable files that are not checked out.
        /// </summary>
        [TaskAction(GetTaskAction, false)]
        public bool Overwrite { get; set; }

        /// <summary>
        /// Implies All and Overwrite.
        /// </summary>
        [TaskAction(GetTaskAction, false)]
        [TaskAction(MergeTaskAction, false)]
        public bool Force { get; set; }

        /// <summary>
        /// Set to true to perform a merge without a basis version
        /// </summary>
        [TaskAction(MergeTaskAction, false)]
        public bool Baseless { get; set; }

        /// <summary>
        /// Sets the TFS Server
        /// </summary>
        [TaskAction(AddTaskAction, false)]
        [TaskAction(CheckinTaskAction, false)]
        [TaskAction(CheckoutTaskAction, false)]
        [TaskAction(DeleteTaskAction, false)]
        [TaskAction(GetTaskAction, false)]
        [TaskAction(MergeTaskAction, false)]
        [TaskAction(GetPendingChangesTaskAction, true)]
        [TaskAction(UndoCheckoutTaskAction, false)]
        [TaskAction(UndeleteTaskAction, false)]
        public string Server { get; set; }

        /// <summary>
        /// Sets the files or folders to use.
        /// </summary>
        [TaskAction(AddTaskAction, false)]
        [TaskAction(CheckinTaskAction, false)]
        [TaskAction(CheckoutTaskAction, false)]
        [TaskAction(DeleteTaskAction, false)]
        [TaskAction(GetTaskAction, false)]
        [TaskAction(MergeTaskAction, false)]
        [TaskAction(GetPendingChangesTaskAction, true)]
        [TaskAction(UndoCheckoutTaskAction, false)]
        [TaskAction(UndeleteTaskAction, false)]
        public string ItemPath { get; set; }

        /// <summary>
        /// Sets the Item Collection of files to use.
        /// </summary>
        [TaskAction(AddTaskAction, false)]
        [TaskAction(CheckinTaskAction, false)]
        [TaskAction(CheckoutTaskAction, false)]
        [TaskAction(DeleteTaskAction, false)]
        [TaskAction(GetTaskAction, false)]
        [TaskAction(UndoCheckoutTaskAction, false)]
        [TaskAction(UndeleteTaskAction, false)]
        public ITaskItem[] ItemCol { get; set; }

        /// <summary>
        /// Sets the working directory. If the directory is mapped in a workspace, then there is no need to specify the Server.
        /// </summary>
        [TaskAction(AddTaskAction, false)]
        [TaskAction(CheckinTaskAction, false)]
        [TaskAction(CheckoutTaskAction, false)]
        [TaskAction(DeleteTaskAction, false)]
        [TaskAction(GetTaskAction, false)]
        [TaskAction(UndoCheckoutTaskAction, false)]
        [TaskAction(UndeleteTaskAction, false)]
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Sets the version of Tfs. Default is 2008
        /// </summary>
        [TaskAction(AddTaskAction, false)]
        [TaskAction(CheckinTaskAction, false)]
        [TaskAction(CheckoutTaskAction, false)]
        [TaskAction(DeleteTaskAction, false)]
        [TaskAction(GetTaskAction, false)]
        [TaskAction(MergeTaskAction, false)]
        [TaskAction(GetPendingChangesTaskAction, true)]
        [TaskAction(UndoCheckoutTaskAction, false)]
        [TaskAction(UndeleteTaskAction, false)]
        public string Version
        {
            get { return this.version; }
            set { this.version = value; }
        }

        /// <summary>
        /// Sets the comments.
        /// </summary>
        [TaskAction(CheckinTaskAction, false)]
        public string Comments { get; set; }

        /// <summary>
        /// Sets the Destination for a Merge
        /// </summary>
        [TaskAction(MergeTaskAction, false)]
        public string Destination { get; set; }

        /// <summary>
        /// Sets the notes.
        /// </summary>
        [TaskAction(CheckinTaskAction, false)]
        public string Notes { get; set; }

        /// <summary>
        /// Sets whether the Tfs operation should be recursive. Default is true.
        /// </summary>
        [TaskAction(AddTaskAction, false)]
        [TaskAction(CheckinTaskAction, false)]
        [TaskAction(CheckoutTaskAction, false)]
        [TaskAction(DeleteTaskAction, false)]
        [TaskAction(GetTaskAction, false)]
        [TaskAction(MergeTaskAction, false)]
        [TaskAction(GetPendingChangesTaskAction, true)]
        [TaskAction(UndoCheckoutTaskAction, false)]
        [TaskAction(UndeleteTaskAction, false)]
        public bool Recursive
        {
            get { return this.recursive; }
            set { this.recursive = value; }
        }

        /// <summary>
        /// Gets the pending changes in the format '/Format:detailed'
        /// </summary>
        [Output]
        [TaskAction(GetPendingChangesTaskAction, true)]
        public string PendingChanges { get; set; }

        /// <summary>
        /// Gets whether pending changes exist for a given ItemPath
        /// </summary>
        [Output]
        [TaskAction(GetPendingChangesTaskAction, true)]
        public bool PendingChangesExist { get; set; }

        /// <summary>
        /// Lets you set text to override check-in policies
        /// </summary>
        public string OverrideText { get; set; }

        /// <summary>
        /// Gets the ExitCode
        /// </summary>
        [Output]
        public int ExitCode { get; set; }

        protected override void InternalExecute()
        {
            if (!this.TargetingLocalMachine())
            {
                return;
            }
            
            this.ResolveExePath();
            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "TF Operation: {0}", this.TaskAction));
            switch (this.TaskAction)
            {
                case "Add":
                    this.Add();
                    break;
                case "Checkin":
                    this.Checkin();
                    break;
                case "Checkout":
                    this.Checkout();
                    break;
                case "Get":
                    this.GetFiles();
                    break;
                case "GetPendingChanges":
                    this.GetPendingChanges();
                    break;
                case "Delete":
                    this.Delete();
                    break;
                case "Merge":
                    this.Merge();
                    break;
                case "UndoCheckout":
                    this.UndoCheckout();
                    break;
                case "Undelete":
                    this.Undelete();
                    break;
                default:
                    this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "Invalid TaskAction passed: {0}", this.TaskAction));
                    return;
            }
        }

        private void GetPendingChanges()
        {
            this.ExecuteCommand("status", string.Empty, "/Format:detailed /user:* /recursive");
            this.PendingChanges = this.returnOutput;
            if (this.returnOutput.IndexOf("There are no pending changes", StringComparison.OrdinalIgnoreCase) < 0)
            {
                this.PendingChangesExist = true;
            }
        }

        private void GetFiles()
        {
            string args = string.Empty;

            if (!string.IsNullOrEmpty(this.VersionSpec))
            {
                args += " /version:" + "\"" + this.VersionSpec + "\"";
            }

            if (this.Force)
            {
                args += " /force";
            }
            else if (this.Overwrite)
            {
                args += " /overwrite";
            }
            else if (this.All)
            {
                args += " /all";
            }

            this.ExecuteCommand("get", args, "/noprompt /recursive");
        }

        private void Undelete()
        {
            this.ExecuteCommand("undelete", string.Empty, "/noprompt /recursive");
        }

        private void UndoCheckout()
        {
            this.ExecuteCommand("undo", string.Empty, "/noprompt /recursive");
        }

        private void Checkout()
        {
            this.ExecuteCommand("checkout", string.Empty, "/noprompt /recursive");
        }

        private void Merge()
        {
            if (string.IsNullOrEmpty(this.ItemPath))
            {
                Log.LogError("ItemPath is required for Merge");
                return;
            }

            string args = string.Empty;

            if (!string.IsNullOrEmpty(this.VersionSpec))
            {
                args += " /version:" + "\"" + this.VersionSpec + "\"";
            }

            if (this.Force)
            {
                args += " /force";
            }

            if (this.Baseless)
            {
                args += " /baseless";
            }

            if (!string.IsNullOrEmpty(this.Destination))
            {
                args += " \"" + this.Destination + "\" ";
            }
            else
            {
                Log.LogError("Destination is required for Merge");
                return;
            }

            this.ExecuteCommand("merge", args, "/noprompt /recursive");
        }

        private void Checkin()
        {
            string comment = string.Empty;
            if (!string.IsNullOrEmpty(this.Comments))
            {
                comment = String.Format(CultureInfo.CurrentCulture, "/comment:\"{0}\"", this.Comments);
            }

            string note = string.Empty;
            if (!string.IsNullOrEmpty(this.Notes))
            {
                note = String.Format(CultureInfo.CurrentCulture, "/notes:\"{0}\"", this.Notes);
            }

            this.ExecuteCommand("checkin", String.Format(CultureInfo.CurrentCulture, "{0} {1}", comment, note), "/noprompt /recursive");
        }

        private void Delete()
        {
            this.ExecuteCommand("delete", string.Empty, "/recursive");
        }

        private void Add()
        {
            this.ExecuteCommand("add", string.Empty, "/noprompt /recursive");
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="options">The options.</param>
        /// <param name="lastOptions">The last options.</param>
        private void ExecuteCommand(string action, string options, string lastOptions)
        {
            if (!this.DetermineItemSpec())
            {
                return;
            }

            string arguments = String.Format(CultureInfo.CurrentCulture, "{0} {1} {2}", action, this.itemSpec, options);
            if (string.IsNullOrEmpty(this.OverrideText) == false)
            {
                arguments += " /override:\"" + this.OverrideText + "\"";
            }

            if (string.IsNullOrEmpty(this.Server) == false)
            {
                arguments += " /s:" + this.Server;
            }

            if (!this.Recursive)
            {
                lastOptions = lastOptions.Replace("/recursive", string.Empty);
            }

            arguments += " " + lastOptions;

            this.shellWrapper = new ShellWrapper(this.teamFoundationExe, arguments);
            if (string.IsNullOrEmpty(this.WorkingDirectory) == false)
            {
                this.shellWrapper.WorkingDirectory = this.WorkingDirectory;
            }

            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Executing {0} {1}", this.shellWrapper.Executable, this.shellWrapper.Arguments));
            this.ExitCode = this.shellWrapper.Execute();
            this.returnOutput = this.shellWrapper.StandardOutput;
            this.LogTaskMessage(MessageImportance.Low, this.returnOutput);
            this.SwitchReturnValue(this.shellWrapper.StandardError.Trim());
        }

        private bool DetermineItemSpec()
        {
            if (string.IsNullOrEmpty(this.ItemPath))
            {
                if (this.ItemCol == null)
                {
                    this.Log.LogError("ItemCol or ItemPath must be defined");
                    return false;
                }

                foreach (ITaskItem i in this.ItemCol)
                {
                    this.itemSpec += "\"" + i.ItemSpec + "\" ";
                }
            }
            else
            {
                this.itemSpec = "\"" + this.ItemPath + "\"";
            }

            return true;
        }

        private void SwitchReturnValue(string error)
        {
            switch (this.ExitCode)
            {
                case 1:
                    this.LogTaskWarning("Exit Code 1. Partial success: " + error);
                    break;
                case 2:
                    this.Log.LogError("Exit Code 2. Unrecognized command: " + error);
                    break;
                case 100:
                    this.Log.LogError("Exit Code 100. Nothing Succeeded: " + error);
                    break;
            }
        }

        private void ResolveExePath()
        {
            this.LogTaskMessage(MessageImportance.Low, "Resolve TF.exe path");

            string vstools = string.Empty;
            switch (this.Version)
            {
                case "2008":
                    vstools = Environment.GetEnvironmentVariable("VS90COMNTOOLS");
                    break;
                case "2005":
                    vstools = Environment.GetEnvironmentVariable("VS80COMNTOOLS");
                    break;
            }

            if (!string.IsNullOrEmpty(vstools))
            {
                this.teamFoundationExe = Path.Combine(vstools, @"..\IDE\tf.exe");
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.CurrentCulture, "TF.exe path resolved to: {0}", this.teamFoundationExe));
            }

            if (!File.Exists(this.teamFoundationExe))
            {
                this.teamFoundationExe = "tf.exe";
                this.LogTaskMessage(MessageImportance.Low, "Unable to resolve TF.exe path. Assuming it is in the PATH environment variable.");
            }
        }
    }
}