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
        /// Generate a config file if missing from a backup .json file
        /// </summary>
        public bool createIfMissing = false;

        /// <summary>
        /// Path to the backup config file in resources, used if 'createIfMissing' is enabled
        /// </summary>
        public string backupPathInResources = "";

        #endregion

        #region PUBLIC - START

        /// <summary>
        /// Unity Lifecycle Function
        /// </summary>
        //----------------------------------//
        public void Start()
        //----------------------------------//
        {
            ConfigManager.Create
            (
                //Options
                new ConfigManager.Options()
                {
                    showDebugLogs = debug,
                    path = path
                },

                //OnSuccess
                (ConfigManager.ConfigManagerSystem system ) => { CopyFileFromBackupIfMissing( system ); },

                //OnFailed
                (string error)=> { Debug.LogError( error ); }
            );

        } //END Start

        #endregion

        #region PRIVATE - COPY IF MISSING

        /// <summary>
        /// Copies the backup to the file at the path if its missing
        /// </summary>
        //--------------------------------------------------------------//
        private void CopyFileFromBackupIfMissing( ConfigManager.ConfigManagerSystem system )
        //--------------------------------------------------------------//
        {

            bool exists = ConfigManager.DoesFileExist( system );

            if(debug)
            {
                if(exists)
                {
                    Debug.Log( "Demo.cs Start() ConfigManager.Create() was successful and file exists" );
                }
                else
                {
                    Debug.Log( "Demo.cs Start() ConfigManager.Create() was successful but file does not exist at the path" );

                    if(createIfMissing)
                    {
                        ConfigManager.CreateFile
                        (
                            system,

                            //OnSuccess
                            () => 
                            {
                                Debug.Log( "Demo.cs Start() ConfigManager.CreateFile() generated new file since one did not exist" );
                                CopyBackupToFile(system); 
                            },

                            //OnFailed
                            ( string error ) => { Debug.LogError( error ); }
                        );

                    }

                }
            }

        } //END CopyFileFromBackupIfMissing

        #endregion

        #region PRIVATE - COPY BACKUP TO FILE

        /// <summary>
        /// Copies the backup json to the file that was generated
        /// </summary>
        /// <param name="system"></param>
        //------------------------------------------------------------------------------//
        private void CopyBackupToFile( ConfigManager.ConfigManagerSystem system )
        //------------------------------------------------------------------------------//
        {

#if EXT_TOTALJSON
            //Load the backup json that we'll write to the path
            string backupString = Resources.Load( backupPathInResources ).ToString();

            try
            {
                //Turn the string data into a json object
                JSON json = JSON.ParseString( backupString );

                //Write the backup json to the new file
                ConfigManager.WriteFileContents
                (
                    system,
                    json,

                    //OnSuccess
                    () =>
                    {
                        Debug.Log( "Demo.cs ConfigManager.WriteFileContents() successfully copied backup to file" );
                    },

                     //OnFailed
                     ( string error ) => { Debug.Log( error ); }
                );
            }
            catch(Exception e)
            {
                Debug.Log( e.ToString() );
            }
#endif

        } //END CopyBackupToFile Method

        #endregion

    } //END Demo Class

} //END gambit.config Namespace