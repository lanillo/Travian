﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TravianBot.Ressources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Constants {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Constants() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TravianBot.Ressources.Constants", typeof(Constants).Assembly);
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
        ///   Looks up a localized string similar to E:\Projects\SeleniumBots\TravianBot\TravianBot\Ressources\data.json.
        /// </summary>
        internal static string dataJson {
            get {
                return ResourceManager.GetString("dataJson", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is no village at these coordinates..
        /// </summary>
        internal static string noVillageMsg {
            get {
                return ResourceManager.GetString("noVillageMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to E:\Projects\SeleniumBots\TravianBot\TravianBot\Ressources\Oasis.txt.
        /// </summary>
        internal static string oasisPath {
            get {
                return ResourceManager.GetString("oasisPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to E:\Projects\SeleniumBots\TravianBot\TravianBot\Ressources\Raid.txt.
        /// </summary>
        internal static string raidPath {
            get {
                return ResourceManager.GetString("raidPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://ts4.travian.com/.
        /// </summary>
        internal static string travianUrl {
            get {
                return ResourceManager.GetString("travianUrl", resourceCulture);
            }
        }
    }
}
