#region IMPORTS

using UnityEngine;
using System;

#if GAMBIT_CONFIG
using gambit.config;
#endif

#if EXT_TOTALJSON
using Leguar.TotalJSON;
#endif

#endregion

namespace gambit.config
{

    /// <summary>
    /// Demo for the ConfigManager
    /// </summary>
    //-------------------------------------------//
    public class Demo: MonoBehaviour
    //-------------------------------------------//
    {

        #region PUBLIC - VARIABLES

        /// <summary>
        /// Should we show debug logs?
        /// </summary>
        public bool debug = true;

        /// <summary>
        /// The path to use
        /// </summary>
        public string path;

        /// <summary>
        /// Path to the backup config file and name (without the extension) in resources
        /// </summary>
        public string backupPathAndNameInResources = "config";

        /// <summary>
        /// The keys we want to look through our JSON for. The last value in this array is the final key that should contain the string we're looking for
        /// </summary>
        public string[ ] keysToFindLongNameValueInJSON = { "app", "longname" };

        /// <summary>
        /// The keys we want to look through our JSON for. The last value in this array is the final key that should contain the string we're looking for
        /// </summary>
        public string[ ] keysToFindShortNameValueInJSON = { "app", "shortname" };

        /// <summary>
        /// The keys we want to look through our JSON for. The last value in this array is the final key that should contain the string we're looking for
        /// </summary>
        public string[ ] keysToFindPathValueInJSON = { "app", "path" };

        /// <summary>
        /// The keys we want to look through our JSON for. The last value in this array is the final key that should contain the value we're looking for
        /// </summary>
        public string[ ] keysToFindAddressValueInJSON = { "communication", "address" };

        /// <summary>
        /// The keys we want to look through our JSON for. The last value in this array is the final key that should contain the value we're looking for
        /// </summary>
        public string[ ] keysToFindPortValueInJSON = { "communication", "port" };


        #endregion

        #region PUBLIC - START

        /// <summary>
        /// Unity Lifecycle Function
        /// </summary>
        //----------------------------------//
        public void Start()
        //----------------------------------//
        {
#if !EXT_TOTALJSON
            Debug.LogError( "Demo.cs Start() Missing 'EXT_TOTALJSON' scripting define symbol in project settings." );
            return;
#else
            //Create a ConfigManager
            ConfigManager.Create
            (
                //Options
                new ConfigManager.Options()
                {
                    showDebugLogs = debug,
                    path = path
                },

                //OnSuccess
                (ConfigManager.ConfigManagerSystem system ) =>  
                {
                    //Delete + Replace the file using our backup in Resources
                    ConfigManager.ReplaceFileUsingResources
                    (
                        system,
                        backupPathAndNameInResources,

                        //OnSuccess
                        (JSON json)=>
                        {
                            //Log our success
                            if(debug)
                            {
                                Debug.Log( "Demo.cs Start() Replaced file using backup in resources" );
                                ConfigManager.Log( system );
                            }

                            //Pull the 'app/longname' variable from the data to check its integrity
                            ConfigManager.GetNestedString
                            (
                                system,
                                keysToFindLongNameValueInJSON,

                                //OnSuccess
                                ( string text ) =>
                                {
                                    if(debug)
                                    {
                                        Debug.Log( "Demo.cs Start() found longname = " + text );
                                    }
                                },
                                ( string error ) =>
                                {
                                    if(debug)
                                    {
                                        Debug.LogError( error );
                                    }
                                }
                            );

                            //Pull the 'app/shortname' variable from the data to check its integrity
                            ConfigManager.GetNestedString
                            (
                                system,
                                keysToFindShortNameValueInJSON,

                                //OnSuccess
                                ( string text ) =>
                                {
                                    if(debug)
                                    {
                                        Debug.Log( "Demo.cs Start() found shortname = " + text );
                                    }
                                },
                                ( string error ) =>
                                {
                                    if(debug)
                                    {
                                        Debug.LogError( error );
                                    }
                                }
                            );

                            //Pull the 'app/path' variable from the data to check its integrity
                            ConfigManager.GetNestedPath
                            (
                                system,
                                keysToFindPathValueInJSON,
                                
                                //OnSuccess
                                (string text ) => 
                                {
                                    if(debug)
                                    {
                                        Debug.Log( "Demo.cs Start() found path = " + text );
                                    }
                                },
                                (string error)=>
                                {
                                    if(debug)
                                    {
                                        Debug.LogError( error );
                                    }
                                }
                            );

                            //Pull the 'communication/address' variable from the data to check its integrity
                            ConfigManager.GetNestedString
                            (
                                system,
                                keysToFindAddressValueInJSON,

                                //OnSuccess
                                ( string text ) =>
                                {
                                    if(debug)
                                    {
                                        Debug.Log( "Demo.cs Start() found address = " + text );
                                    }
                                },
                                ( string error ) =>
                                {
                                    if(debug)
                                    {
                                        Debug.LogError( error );
                                    }
                                }
                            );

                            //Pull the 'communication/port' variable from the data to check its integrity
                            ConfigManager.GetNestedInteger
                            (
                                system,
                                keysToFindPortValueInJSON,

                                //OnSuccess
                                ( int value ) =>
                                {
                                    if(debug)
                                    {
                                        Debug.Log( "Demo.cs Start() found port = " + value );
                                    }
                                },
                                ( string error ) =>
                                {
                                    if(debug)
                                    {
                                        Debug.LogError( error );
                                    }
                                }
                            );

                        },

                        //OnFailed
                        (string error)=>
                        {
                            if( debug ) Debug.LogError( error );
                        }
                    );

                },

                //OnFailed
                (string error)=> { if(debug) Debug.LogError( error ); }
            );
#endif

        } //END Start

        #endregion



    } //END Demo Class

} //END gambit.config Namespace