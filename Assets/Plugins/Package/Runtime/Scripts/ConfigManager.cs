/********************************************************
 * ConfigManager.cs
 * 
 * Reads and writes a json config file
 * 
 ********************************************************/


#region IMPORTS

using System;
using System.IO;
using UnityEngine;

#if GAMBIT_SINGLETON
using gambit.singleton;
#else
/// <summary>
/// Fallback Singleton base class if GAMBIT_SINGLETON is not defined.
/// </summary>
/// <typeparam name="T">Type of the MonoBehaviour singleton.</typeparam>
public class Singleton<T>: MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    /// <summary>
    /// Gets the singleton instance, creating it if necessary.
    /// </summary>
    //---------------------------------------------//
    public static T Instance
    //---------------------------------------------//
    {
        get
        {
            if(instance == null)
            {
                instance = new GameObject( typeof( T ).Name ).AddComponent<T>();
                GameObject.DontDestroyOnLoad( instance.gameObject );
            }
            return instance;
        }
    }

} //END Singleton<T> class
#endif

#if EXT_TOTALJSON
using Leguar.TotalJSON;
#endif

#endregion

namespace gambit.config
{

    /// <summary>
    /// Singleton Manager for reading and writing json config files
    /// </summary>
    public class ConfigManager : Singleton<ConfigManager>
    {
        #region PUBLIC - VARIABLES

        /// <summary>
        /// Current instance of the ConfigSystem instantiated during Create()
        /// </summary>
        public static ConfigManagerSystem system;

        #endregion

        #region PUBLIC - CREATION OPTIONS

        /// <summary>
        /// Options object passed in during Create()
        /// </summary>
        //---------------------------------------------//
        public class Options
        //---------------------------------------------//
        {

            /// <summary>
            /// Should debug logs be printed to the console log?
            /// </summary>
            public bool showDebugLogs = true;

            /// <summary>
            /// Path to the config file
            /// </summary>
            public string path = "";

            /// <summary>
            /// Generates a blank config file if missing
            /// </summary>
            public bool createIfMissing = false;

        } //END Options Class

        #endregion

        #region PUBLIC - RETURN CLASS : CONFIG MANAGER SYSTEM

        /// <summary>
        /// Configuration system returned upon successful Create()
        /// </summary>
        //----------------------------------------------------------//
        public class ConfigManagerSystem
        //----------------------------------------------------------//
        {

            /// <summary>
            /// The options object passed in during Create()
            /// </summary>
            public Options options;

        } //END ConfigManagerSystem Class

        #endregion

        #region PUBLIC - CREATE

        /// <summary>
        /// Create a config manager system, which can be used to read and write data to a config file
        /// </summary>
        /// <param name="options">The Config Options object filled with settings</param>
        /// <param name="OnSuccess">Returns a ConfigManagerSystem</param>
        /// <param name="OnFailed">Returns a string with an error message</param>
        //------------------------------------------------------------------------------//
        public static void Create
        //------------------------------------------------------------------------------//
        (
            Options options = null,
            Action<ConfigManagerSystem> OnSuccess = null,
            Action<string> OnFailed = null )
        {

            if(options == null)
                OnFailed?.Invoke( "ConfigManager.cs Create() Options object passed in is null, unable to continue" );

            if(system != null)
                OnFailed?.Invoke( "ConfigManager.cs Create() ConfigManagerSystem is not null, please call Destroy() before attempting to create a new ConfigManagerSystem via this Create() method" );

            system = new ConfigManagerSystem();
            system.options = options;
            
            if(system.options.path == "")
                OnFailed.Invoke( "ConfigManager.cs Create() path is empty" );

            OnSuccess?.Invoke( system );

        } //END Create Method

        #endregion

        #region PRIVATE - DOES FILE EXIST?

        /// <summary>
        /// Checks if the config file exists at the specified path
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        //------------------------------------------------------------------//
        public static bool DoesFileExist( ConfigManagerSystem system )
        //------------------------------------------------------------------//
        {

            if(system == null)
                return false;

            return DoesFileExist( system.options.path );

        } //END DoesFileExist Method

        /// <summary>
        /// Checks if the config file exists at the specified path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        //------------------------------------------------------------------//
        public static bool DoesFileExist( string path )
        //------------------------------------------------------------------//
        {

            if(path == null || (path != null && path == "") )
                return false;

            if(File.Exists( path ))
            {
                return true;
            }
            else
            {
                return false;
            }


        } //END DoesFileExist Method

        #endregion

        #region PRIVATE - CREATE FILE

        /// <summary>
        /// Generates a config file at the path if one does not already exist
        /// </summary>
        //---------------------------------------//
        public static void CreateFile
        ( 
            ConfigManagerSystem system, 
            Action OnSuccess = null, 
            Action<string> OnFailed = null 
        )
        //---------------------------------------//
        {
            if(system == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs CreateFile() passed in system object is null" );
                return;
            }

            if(DoesFileExist( system ))
            {
                OnSuccess?.Invoke();
                return;
            }

            File.Create( system.options.path ).Close();

            OnSuccess?.Invoke();

        } //END CreateFile Method

        #endregion

        #region PUBLIC - WRITE FILE CONTENTS

#if EXT_TOTALJSON

        /// <summary>
        /// Writes the file with new json data
        /// </summary>
        /// <param name="system">The ConfigManagerSystem object passed back during Create. We will write to the path in the options object</param>
        /// <param name="json">The TotalJSON object you want to write to the file</param>
        //---------------------------------------------------------------------------------------------//
        public static void WriteFileContents
        (
            ConfigManagerSystem system,
            JSON json,
            Action OnSuccess = null,
            Action<string> OnFailed = null
        )
        //---------------------------------------------------------------------------------------------//
        {
            if(system == null)
                OnFailed?.Invoke( "ConfigManager.cs WriteFileContents() passed in system is null" );

            WriteFileContents( system.options.path, json, OnSuccess, OnFailed );

        } //END WriteFileContents Method

        /// <summary>
        /// Writes the file with new json data
        /// </summary>
        /// <param name="path">The path you want to write to</param>
        /// <param name="json">The TotalJSON object you want to write to the file</param>
        //---------------------------------------------------------------------------------------------//
        public static void WriteFileContents
        ( 
            string path, 
            JSON json,
            Action OnSuccess = null,
            Action<string> OnFailed = null
        )
        //---------------------------------------------------------------------------------------------//
        {
            if(path == null || path == "")
            {
                OnFailed?.Invoke( "ConfigManager.cs WriteFileContents() passed in path is null" );
                return;
            }

            if(json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs WriteFileContents() passed in json is null" );
                return;
            }

            if(!DoesFileExist( path ))
            {
                OnFailed?.Invoke( "ConfigManager.cs WriteFileContents() file does not exist to write json data to" );
                return;
            }

            try
            {
                string jsonAsString = json.CreatePrettyString();

                StreamWriter writer = new StreamWriter( path );

                writer.WriteLine( jsonAsString );

                writer.Close();

                OnSuccess?.Invoke();
            }
            catch( Exception e ) 
            {
                OnFailed?.Invoke( "ConfigManager.cs WriteFileContents() failed to write json to file at path . e = " + e.ToString() );
            }
            
        } //END WriteFileContents Method

#endif

        #endregion

        #region PUBLIC - READ FILE CONTENTS

#if EXT_TOTALJSON

        /// <summary>
        /// Reads a file and returns json data
        /// </summary>
        //---------------------------------------------------------------------------------------------//
        public static void ReadFileContents
        (
            ConfigManagerSystem system,
            Action<JSON> OnSuccess = null,
            Action<string> OnFailed = null
        )
        //---------------------------------------------------------------------------------------------//
        {
            if(system == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs ReadFileContents() passed in system is null" );
                return;
            }

            ReadFileContents( system.options.path, OnSuccess, OnFailed );

        } //END ReadFileContents Method

        /// <summary>
        /// Reads a file and returns json data
        /// </summary>
        //---------------------------------------------------------------------------------------------//
        public static void ReadFileContents
        (
            string path,
            Action<JSON> OnSuccess = null,
            Action<string> OnFailed = null
        )
        //---------------------------------------------------------------------------------------------//
        {
            if(path == null || (path != null && path == "") )
            {
                OnFailed?.Invoke( "ConfigManager.cs ReadFileContents() passed in path is null or empty" );
                return;
            }

            try
            {
                StreamReader reader = new StreamReader( path );

                string jsonAsString = reader.ReadToEnd();

                reader.Close();

                JSON json = JSON.ParseString( jsonAsString );

                OnSuccess?.Invoke( json );
            }
            catch( Exception e ) 
            {
                OnFailed?.Invoke( "ConfigManager.cs ReadFileContents() error = " + e.ToString() );
            }

        } //END ReadFileContents Method

#endif

        #endregion

    } //END ConfigManager Class

} //END gambit.config Namespace