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
            //Create a ConfigManager
            ConfigManager.Create
            (
                //Options
                new ConfigManager.Options()
                {
                    showDebugLogs = debug
                },

                //OnSuccess
                (ConfigManager.ConfigManagerSystem newSystem ) =>  
                {
                    system = newSystem;

                    //Pull the config file path from our config file in resources
                    ConfigManager.ReadFileContentsFromResources
                    (
                        system,
                        backupPathAndNameInResources,
                        PullValuesFromData,
                        LogError
                    );
                },

                //OnFailed
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
        private void PullValuesFromData( JSON json )
        //----------------------------------------//
        {

            //Log the info variables
            Debug.Log( "Demo.cs PullValuesFromData() version = " + system.version );
            Debug.Log( "Demo.cs PullValuesFromData() timestamp = " + system.timestamp );
            Debug.Log( "Demo.cs PullValuesFromData() path = " + system.path );


            int areDone = 0;
            int waitForTotal = 5;

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

                    areDone++;

                    if(areDone == waitForTotal)
                    {
                        ReplaceFileUsingResources();
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

                    areDone++;

                    if(areDone == waitForTotal)
                    {
                        ReplaceFileUsingResources();
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

                    areDone++;

                    if(areDone == waitForTotal)
                    {
                        ReplaceFileUsingResources();
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

                    areDone++;

                    if(areDone == waitForTotal)
                    {
                        ReplaceFileUsingResources();
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

                    areDone++;

                    if(areDone == waitForTotal)
                    {
                        ReplaceFileUsingResources();
                    }
                },
                LogError
            );

        } //END PullValuesFromData Method

        #endregion

        #region PRIVATE - REPLACE FILE USING RESOURCES BACKUP

        /// <summary>
        /// Sets the ConfigManagerSystem's path variable using the data from the backup file we found in resources
        /// </summary>
        /// <param name="json"></param>
        //--------------------------------------------------//
        private void ReplaceFileUsingResources()
        //--------------------------------------------------//
        {

            //Delete + Replace the file using our backup in Resources
            ConfigManager.ReplaceFileUsingResources
            (
                system,
                backupPathAndNameInResources,

                //OnSuccess
                ( JSON json ) =>
                {
                    //Log our success
                    if(debug)
                    {
                        Debug.Log( "Demo.cs ReplaceFileUsingResources() Successfully replaced config file using backup in resources" );
                        ConfigManager.Log( system );
                    }
                },

                //OnFailed
                LogError
            );

        } //END GetPathFromData

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