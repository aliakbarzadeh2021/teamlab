//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASC.PhotoManager.Resources {
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
    internal class PhotoPatternResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal PhotoPatternResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ASC.PhotoManager.Resources.PhotoPatternResource", typeof(PhotoPatternResource).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to h1.New Photos in Event: &quot;$EventName&quot;:&quot;$EventURL&quot;
        ///
        ///&quot;$UserName&quot;:&quot;$UserURL&quot; $Date
        ///
        ///&quot;$PhotoCount&quot;:&quot;$URL&quot; new photos were added to &quot;&quot;$EventName&quot;:&quot;$EventURL&quot;&quot; event.
        ///
        ///&quot;View Photos&quot;:&quot;$URL&quot;
        ///
        ///Your portal address: &quot;$__VirtualRootPath&quot;:&quot;$__VirtualRootPath&quot;
        ///
        ///&quot;Edit subscription settings&quot;:&quot;$RecipientSubscriptionConfigURL&quot;.
        /// </summary>
        internal static string pattern_1 {
            get {
                return ResourceManager.GetString("pattern_1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to h1.New Comment in Event: &quot;$EventName&quot;:&quot;$EventURL&quot;
        ///
        ///&quot;$UserName&quot;:&quot;$UserURL&quot; $Date
        ///
        ///New comment for &quot;$PhotoName&quot; photo in the &quot;&quot;$AlbumName&quot;:&quot;$AlbumURL&quot;&quot; album:
        ///
        ///$CommentBody
        ///
        ///&quot;Read More&quot;:&quot;$URL&quot;
        ///
        ///Your portal address:&quot;$__VirtualRootPath&quot;:&quot;$__VirtualRootPath&quot;
        ///
        ///&quot;Edit subscription settings&quot;:&quot;$RecipientSubscriptionConfigURL&quot;.
        /// </summary>
        internal static string pattern_2 {
            get {
                return ResourceManager.GetString("pattern_2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Community. New Photos in Event: $EventName.
        /// </summary>
        internal static string subject_1 {
            get {
                return ResourceManager.GetString("subject_1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Community. New Comment in Event:  $EventName.
        /// </summary>
        internal static string subject_2 {
            get {
                return ResourceManager.GetString("subject_2", resourceCulture);
            }
        }
    }
}
