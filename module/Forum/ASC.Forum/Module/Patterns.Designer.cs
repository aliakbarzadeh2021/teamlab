//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASC.Forum.Module {
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
    internal class Patterns {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Patterns() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ASC.Forum.Module.Patterns", typeof(Patterns).Assembly);
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
        ///   Looks up a localized string similar to &lt;pt:catalog xmlns:pt=&quot;urn:asc.notify.pattern.xsd&quot;&gt;
        ///  &lt;block&gt;
        ///    &lt;formatter type=&quot;ASC.Notify.Patterns.NVelocityPatternFormatter, ASC.Common&quot; /&gt;
        ///    &lt;patterns&gt;
        ///      &lt;pattern id=&quot;PostInTopicEmailPattern&quot; name=&quot;new post in topic&quot; contentType=&quot;html&quot;&gt;
        ///        &lt;subject resource=&quot;|subject_PostInTopicEmailPattern|ASC.Forum.Module.ForumPatternResource,ASC.Forum&quot;&gt;&lt;/subject&gt;
        ///        &lt;body styler=&quot;ASC.Notify.Textile.TextileStyler,ASC.Notify.Textile&quot; resource=&quot;|pattern_PostInTopicEmailPattern|ASC.Forum.Module.For [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string forum_patterns {
            get {
                return ResourceManager.GetString("forum_patterns", resourceCulture);
            }
        }
    }
}
