//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASC.Web.Host {
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
    internal class ServerMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ServerMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ASC.Web.Host.ServerMessages", typeof(ServerMessages).Assembly);
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
        ///   Looks up a localized string similar to TeamLab Web Server.
        /// </summary>
        internal static string ASCWebServer {
            get {
                return ResourceManager.GetString("ASCWebServer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error processing.
        /// </summary>
        internal static string ErrorProcessing {
            get {
                return ResourceManager.GetString("ErrorProcessing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while closing socket.
        /// </summary>
        internal static string ErrorWhileClosingSocket {
            get {
                return ResourceManager.GetString("ErrorWhileClosingSocket", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while writing responce body.
        /// </summary>
        internal static string ErrorWhileWritingResponceBody {
            get {
                return ResourceManager.GetString("ErrorWhileWritingResponceBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while processing {0}.
        /// </summary>
        internal static string ExceptionWhileProcessing {
            get {
                return ResourceManager.GetString("ExceptionWhileProcessing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Windows XP SP2 or Server 2003 is required to use the HttpListener class..
        /// </summary>
        internal static string HttpListenerNotSupported {
            get {
                return ResourceManager.GetString("HttpListenerNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Scheme not supported.
        /// </summary>
        internal static string SchemeNotSupported {
            get {
                return ResourceManager.GetString("SchemeNotSupported", resourceCulture);
            }
        }
    }
}
