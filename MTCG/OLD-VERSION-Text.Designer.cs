﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MTCG {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class OLD_VERSION_Text {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal OLD_VERSION_Text() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MTCG.OLD-VERSION-Text", typeof(OLD_VERSION_Text).Assembly);
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
        ///   Looks up a localized string similar to HTTP/1.1 200 OK
        ///Content-Type: application/json
        ///
        ///{0}.
        /// </summary>
        internal static string Res_200_WithContent {
            get {
                return ResourceManager.GetString("Res_200_WithContent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 204 No Content
        ///
        ///.
        /// </summary>
        internal static string Res_204_NoContent {
            get {
                return ResourceManager.GetString("Res_204_NoContent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 400 Bad Request
        ///Content-Type: text/plain
        ///
        ///Request could not be processed.
        /// </summary>
        internal static string Res_400_BadRequest {
            get {
                return ResourceManager.GetString("Res_400_BadRequest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 401 Unauthorized
        ///Content-Type: text/plain
        ///
        ///Access token is missing or invalid.
        /// </summary>
        internal static string Res_401_Unauthorized {
            get {
                return ResourceManager.GetString("Res_401_Unauthorized", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 404 Not Found
        ///Content-Type: text/plain
        ///
        ///404 Not Found.
        /// </summary>
        internal static string Res_404_NotFound {
            get {
                return ResourceManager.GetString("Res_404_NotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 404 Not Found
        ///Content-Type: text/plain
        ///
        ///User not found..
        /// </summary>
        internal static string Res_404_User {
            get {
                return ResourceManager.GetString("Res_404_User", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 500 Internal Server Error
        ///Content-Type: text/plain
        ///
        ///The server has encountered a situation it does not know how to handle.
        /// </summary>
        internal static string Res_500_ServerError {
            get {
                return ResourceManager.GetString("Res_500_ServerError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 200 OK
        ///Content-Type: application/json
        ///
        ///{0}
        ///
        ///{1}.
        /// </summary>
        internal static string Res_GetDeck_200 {
            get {
                return ResourceManager.GetString("Res_GetDeck_200", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 200 OK
        ///Content-Type: application/json
        ///
        ///{0}.
        /// </summary>
        internal static string Res_GetStack_200 {
            get {
                return ResourceManager.GetString("Res_GetStack_200", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 200 OK
        ///Content-Type: application/json
        ///
        ///Data successfully retrieved
        ///
        ///{0}.
        /// </summary>
        internal static string Res_GetUser_200 {
            get {
                return ResourceManager.GetString("Res_GetUser_200", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 201 Created
        ///Content-Type: text/plain
        ///
        ///Package and cards successfully created.
        /// </summary>
        internal static string Res_PostPackage_201 {
            get {
                return ResourceManager.GetString("Res_PostPackage_201", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 403 Forbidden
        ///Content-Type: text/plain
        ///
        ///Provided user is not &quot;admin&quot;.
        /// </summary>
        internal static string Res_PostPackage_403 {
            get {
                return ResourceManager.GetString("Res_PostPackage_403", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 409 Conflict
        ///Content-Type: text/plain
        ///
        ///At least one card in the packages already exists.
        /// </summary>
        internal static string Res_PostPackage_409 {
            get {
                return ResourceManager.GetString("Res_PostPackage_409", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 200 OK
        ///Content-Type: text/plain
        ///Authorization: Bearer {0}-mtcgToken
        ///
        ///User login successful.
        /// </summary>
        internal static string Res_PostSession_200 {
            get {
                return ResourceManager.GetString("Res_PostSession_200", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 401 Unauthorized
        ///Content-Type: text/plain
        ///
        ///Invalid username/password provided.
        /// </summary>
        internal static string Res_PostSession_401 {
            get {
                return ResourceManager.GetString("Res_PostSession_401", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 201 Created
        ///Content-Type: text/plain
        ///
        ///Trading deal successfully created.
        /// </summary>
        internal static string Res_PostTrading_201 {
            get {
                return ResourceManager.GetString("Res_PostTrading_201", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 403 Forbidden
        ///Content-Type: text/plain
        ///
        ///The deal contains a card that is not owned by the user or locked in the deck..
        /// </summary>
        internal static string Res_PostTrading_403 {
            get {
                return ResourceManager.GetString("Res_PostTrading_403", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 409 Conflict
        ///Content-Type: text/plain
        ///
        ///A deal with this deal ID already exists..
        /// </summary>
        internal static string Res_PostTrading_409 {
            get {
                return ResourceManager.GetString("Res_PostTrading_409", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 200 OK
        ///Content-Type: application/json
        ///
        ///A package has been successfully bought
        ///
        ///{0}.
        /// </summary>
        internal static string Res_PostTransaction_200 {
            get {
                return ResourceManager.GetString("Res_PostTransaction_200", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 403 Forbidden
        ///Content-Type: text/plain
        ///
        ///Not enough money for buying a card package.
        /// </summary>
        internal static string Res_PostTransaction_403 {
            get {
                return ResourceManager.GetString("Res_PostTransaction_403", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 404 Not Found
        ///Content-Type: text/plain
        ///
        ///No card package available for buying.
        /// </summary>
        internal static string Res_PostTransaction_404 {
            get {
                return ResourceManager.GetString("Res_PostTransaction_404", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 201 Created
        ///Content-Type: text/plain
        ///
        ///User successfully created.
        /// </summary>
        internal static string Res_PostUser_201 {
            get {
                return ResourceManager.GetString("Res_PostUser_201", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 409 Conflict
        ///Content-Type: text/plain
        ///
        ///User with same username already registered.
        /// </summary>
        internal static string Res_PostUser_409 {
            get {
                return ResourceManager.GetString("Res_PostUser_409", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 200 OK
        ///Content-Type: text/plain
        ///
        ///The deck has been successfully configured.
        /// </summary>
        internal static string Res_PutDeck_200 {
            get {
                return ResourceManager.GetString("Res_PutDeck_200", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 400 Bad Request
        ///Content-Type: text/plain
        ///
        ///The provided deck did not include the required amount of cards.
        /// </summary>
        internal static string Res_PutDeck_400 {
            get {
                return ResourceManager.GetString("Res_PutDeck_400", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 403 Forbidden
        ///Content-Type: text/plain
        ///
        ///At least one of the provided cards does not belong to the user or is not available..
        /// </summary>
        internal static string Res_PutDeck_403 {
            get {
                return ResourceManager.GetString("Res_PutDeck_403", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP/1.1 200 OK
        ///Content-Type: text/plain
        ///
        ///User sucessfully updated..
        /// </summary>
        internal static string Res_PutUser_200 {
            get {
                return ResourceManager.GetString("Res_PutUser_200", resourceCulture);
            }
        }
    }
}