﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASC.Web.Files.Services.NotifyService {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class FilesPatternResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal FilesPatternResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ASC.Web.Files.Services.NotifyService.FilesPatternResource", typeof(FilesPatternResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to h1.The Document $DocumentTitle is Deleted
        ///
        ///$__DateTime The user &quot;$__AuthorName&quot;:&quot;$__AuthorUrl&quot; has deleted the document.
        ///
        ///Your portal address is: &quot;$__VirtualRootPath&quot;:&quot;$__VirtualRootPath&quot;.
        /// </summary>
        public static string pattern_DeleteDocument {
            get {
                return ResourceManager.GetString("pattern_DeleteDocument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to h1.$__DateTime User &quot;$__AuthorName&quot;:&quot;$__AuthorUrl&quot; 
        ///
        ///granted you the access to the &quot;$DocumentTitle&quot;:&quot;$DocumentURL&quot;
        ///
        ///with access rights: &quot;$AccessRights&quot;.
        ///
        ///Your portal address: &quot;$__VirtualRootPath&quot;:&quot;$__VirtualRootPath&quot;.
        /// </summary>
        public static string pattern_ShareDocument {
            get {
                return ResourceManager.GetString("pattern_ShareDocument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to h1.$__DateTime User &quot;$__AuthorName&quot;:&quot;$__AuthorUrl&quot; 
        ///
        ///granted you the access to the folder &quot;$FolderTitle&quot;:&quot;$__VirtualRootPath/products/files/#$FolderID&quot;
        ///
        ///with access rights: &quot;$AccessRights&quot;.
        ///
        ///Your portal address: &quot;$__VirtualRootPath&quot;:&quot;$__VirtualRootPath&quot;.
        /// </summary>
        public static string pattern_ShareFolder {
            get {
                return ResourceManager.GetString("pattern_ShareFolder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to h1.The Document Version &quot;$DocumentTitle&quot;:&quot;$DocumentURL&quot; is updated
        ///
        ///$__DateTime The user &quot;$AuthorName&quot;:&quot;$AuthorUrl&quot; has updated the document to version $VersionNumber.
        ///
        ///Your portal address is: &quot;$__VirtualRootPath&quot;:&quot;$__VirtualRootPath&quot;.
        /// </summary>
        public static string pattern_UpdateDocument {
            get {
                return ResourceManager.GetString("pattern_UpdateDocument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to h1.The Document &quot;$DocumentTitle&quot;:&quot;$DocumentURL&quot; is uploaded
        ///
        ///$__DateTimeThe user &quot;$AuthorName&quot;:&quot;$AuthorUrl&quot; has uploaded the document to the folder &quot;$FolderTitle&quot;:&quot;${__VirtualRootPath}/products/files/#$FolderID&quot; .
        ///
        ///#if($Comment != &quot;&quot; )
        ///
        ///With a comment:
        ///
        ///$__Helper.Unhtml($Comment)
        ///
        ///#end
        ///
        ///Your portal address is: &quot;$__VirtualRootPath&quot;:&quot;$__VirtualRootPath&quot;.
        /// </summary>
        public static string pattern_UploadDocument {
            get {
                return ResourceManager.GetString("pattern_UploadDocument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;pt:catalog xmlns:pt=&quot;urn:asc.notify.pattern.xsd&quot;&gt;
        ///  &lt;block&gt;
        ///    &lt;formatter type=&quot;ASC.Notify.Patterns.NVelocityPatternFormatter, ASC.Common&quot; /&gt;
        ///    &lt;patterns&gt;
        ///      &lt;pattern id=&quot;ShareDocument&quot; name=&quot;share document&quot; contentType=&quot;html&quot;&gt;
        ///        &lt;subject resource=&quot;|subject_ShareDocument|ASC.Web.Files.Services.NotifyService.FilesPatternResource,ASC.Web.Files&quot;&gt;&lt;/subject&gt;
        ///        &lt;body styler=&quot;ASC.Notify.Textile.TextileStyler,ASC.Notify.Textile&quot; resource=&quot;|pattern_ShareDocument|ASC.Web.Files.Services.Notify [rest of string was truncated]&quot;;.
        /// </summary>
        public static string patterns {
            get {
                return ResourceManager.GetString("patterns", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Docs. The document $DocumentTitle is deleted.
        /// </summary>
        public static string subject_DeleteDocument {
            get {
                return ResourceManager.GetString("subject_DeleteDocument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Documents. You were granted access to $DocumentTitle.
        /// </summary>
        public static string subject_ShareDocument {
            get {
                return ResourceManager.GetString("subject_ShareDocument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Documents. You were granted access to $FolderTitle.
        /// </summary>
        public static string subject_ShareFolder {
            get {
                return ResourceManager.GetString("subject_ShareFolder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Docs. The document version $DocumentTitle is updated.
        /// </summary>
        public static string subject_UpdateDocument {
            get {
                return ResourceManager.GetString("subject_UpdateDocument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Docs. The document $DocumentTitle is uploaded.
        /// </summary>
        public static string subject_UploadDocument {
            get {
                return ResourceManager.GetString("subject_UploadDocument", resourceCulture);
            }
        }
    }
}
