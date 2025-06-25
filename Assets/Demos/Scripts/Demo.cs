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

        #endregion

        #region PUBLIC - START

        /// <summary>
        /// Unity Lifecycle Function
        /// </summary>
        //----------------------------------//
        public void Start()
        //----------------------------------//
        {
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

        } //END Start

        #endregion



    } //END Demo Class

} //END gambit.config Namespace