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
        /// Path to the backup config file and name (without the extension) in resources
        /// </summary>
        public string backupPathAndNameInResources = "config";

        /// <summary>
        /// The config manager system used to interact with our ConfigManager
        /// </summary>
        private ConfigManager.ConfigManagerSystem system;

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
            //Update local if needed and grab its config system
            ConfigManager.UpdateLocalDataAndReturn
            (
                backupPathAndNameInResources,
                debug,
                (ConfigManager.ConfigManagerSystem localSystem)=>
                {
                    system = localSystem;
                    PullValuesFromData();
                },
                LogError
            );
#endif

        } //END Start

        #endregion

        #region PRIVATE - PULL VALUES FROM JSON DATA

        /// <summary>
        /// After our data is loaded, pull variables to set them in our local demo component
        /// </summary>
        //----------------------------------------//
        private void PullValuesFromData()
        //----------------------------------------//
        {

            //Log the info variables
            Debug.Log( "Demo.cs PullValuesFromData() version = " + system.version );
            Debug.Log( "Demo.cs PullValuesFromData() timestamp = " + system.timestamp );
            Debug.Log( "Demo.cs PullValuesFromData() path = " + system.path );


            int count = 0;
            int total = 5;

            //Pull the 'app/longname' variable from the data to check its integrity
            ConfigManager.GetNestedString
            (
                system,
                new string[ ] { "app", "longname" },

                //OnSuccess
                ( string text ) =>
                {
                    if(debug)
                    {
                        Debug.Log( "Demo.cs PullValuesFromData() found longname = " + text );
                    }

                    count++;

                    if(count == total)
                    {
                        Debug.Log( "Finished pulling data from local system" );
                    }
                },
                LogError
            );

            //Pull the 'app/shortname' variable from the data to check its integrity
            ConfigManager.GetNestedString
            (
                system,
                new string[ ] { "app", "shortname" },

                //OnSuccess
                ( string text ) =>
                {
                    if(debug)
                    {
                        Debug.Log( "Demo.cs PullValuesFromData() found shortname = " + text );
                    }

                    count++;

                    if(count == total)
                    {
                        Debug.Log( "Finished pulling data from local system" );
                    }
                },
                LogError
            );

            //Pull the 'app/path' variable from the data to check its integrity
            ConfigManager.GetNestedPath
            (
                system,
                new string[ ] { "app", "path" },

                //OnSuccess
                ( string text ) =>
                {
                    if(debug)
                    {
                        Debug.Log( "Demo.cs PullValuesFromData() found app path = " + text );
                    }

                    count++;

                    if(count == total)
                    {
                        Debug.Log( "Finished pulling data from local system" );
                    }
                },
                LogError
            );

            //Pull the 'communication/address' variable from the data to check its integrity
            ConfigManager.GetNestedString
            (
                system,
                new string[ ] { "communication", "address" },

                //OnSuccess
                ( string text ) =>
                {
                    if(debug)
                    {
                        Debug.Log( "Demo.cs PullValuesFromData() found address = " + text );
                    }

                    count++;

                    if(count == total)
                    {
                        Debug.Log( "Finished pulling data from local system" );
                    }
                },
                LogError
            );

            //Pull the 'communication/port' variable from the data to check its integrity
            ConfigManager.GetNestedInteger
            (
                system,
                new string[ ] { "communication", "port" },

                //OnSuccess
                ( int value ) =>
                {
                    if(debug)
                    {
                        Debug.Log( "Demo.cs PullValuesFromData() found port = " + value );
                    }

                    count++;

                    if(count == total)
                    {
                        Debug.Log( "Finished pulling data from local system" );
                    }
                },
                LogError
            );

        } //END PullValuesFromData Method

        #endregion

        #region PRIVATE - LOG ERROR

        /// <summary>
        /// Logs the error to the console log if the debug variable is enabled
        /// </summary>
        /// <param name="error"></param>
        //----------------------------------------//
        private void LogError( string error )
        //----------------------------------------//
        {
            if( debug )
                Debug.LogError( error );
        
        } //END LogError

        #endregion

    } //END Demo Class

} //END gambit.config Namespace