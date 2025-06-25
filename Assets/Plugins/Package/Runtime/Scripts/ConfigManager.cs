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
using UnityEngine.Rendering.VirtualTexturing;

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

#if EXT_TOTALJSON
            /// <summary>
            /// The JSON contained within the config file
            /// </summary>
            public JSON json;
#endif

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

            ConfigManagerSystem system = new ConfigManagerSystem();
            system.options = options;
            
            if(system.options.path == "")
                OnFailed.Invoke( "ConfigManager.cs Create() path is empty" );

            OnSuccess?.Invoke( system );

        } //END Create Method

        #endregion

        #region PUBLIC - GET NESTED STRING

        /// <summary>
        /// Public method to start the recursive search for a nested string. Returns it fully deserialized with any environment variables expanded
        /// </summary>
        /// <param name="system">The ConfigManagerSystem object with a json data contained within</param>
        /// <param name="keys">An array of keys representing the path to the desired value.</param>
        /// <param name="OnSuccess">Callback invoked with the processed string if found.</param>
        /// <param name="OnFailed">Callback invoked with an error message if not found.</param>
        //------------------------------------------------------------------------------------------------//
        public static void GetNestedString
        (
            ConfigManagerSystem system,
            string[ ] keys,
            Action<string> OnSuccess,
            Action<string> OnFailed
        )
        //------------------------------------------------------------------------------------------------//
        {
            if(system == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedString() passed in ConfigManagerSystem object is null" );
                return;
            }

            if(system.json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedString() passed in ConfigManagerSystem.json object is null" );
                return;
            }

            GetNestedString( system.json, keys, OnSuccess, OnFailed );

        } //END GetNestedString Method

        /// <summary>
        /// Public method to start the recursive search for a nested string. Returns it fully deserialized with any environment variables expanded
        /// </summary>
        /// <param name="json">The root TotalJSON object to search within.</param>
        /// <param name="keys">An array of keys representing the path to the desired value.</param>
        /// <param name="OnSuccess">Callback invoked with the processed string if found.</param>
        /// <param name="OnFailed">Callback invoked with an error message if not found.</param>
        //------------------------------------------------------------------------------------------------//
        public static void GetNestedString
        ( 
            JSON json, 
            string[ ] keys, 
            Action<string> OnSuccess, 
            Action<string> OnFailed 
        )
        //------------------------------------------------------------------------------------------------//
        {
            // --- Input Validation ---
            if(json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedString() Error: Root JSON object is null." );
                return;
            }
            if(keys == null || keys.Length == 0)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedString() Error: Keys array is null or empty." );
                return;
            }

            // --- Start Recursive Search ---
            FindPathRecursive( json, keys, 0, OnSuccess, OnFailed );

        } //END GetNestedString Method

        /// <summary>
        /// The private recursive function that traverses the JSON object.
        /// </summary>
        /// <param name="currentObject"></param>
        /// <param name="keys"></param>
        /// <param name="currentIndex"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //-------------------------------------------------------------------//
        private static void FindPathRecursive
        ( 
            JSON currentObject, 
            string[ ] keys, 
            int currentIndex, 
            Action<string> OnSuccess, 
            Action<string> OnFailed 
        )
        //-------------------------------------------------------------------//
        {
            string currentKey = keys[ currentIndex ];

            // --- Check if the current key exists ---
            if(!currentObject.ContainsKey( currentKey ))
            {
                OnFailed?.Invoke( "ConfigManager.cs FindPathRecursive() Error: Key " + currentKey + " not found at the current level" );
                return;
            }

            // --- Check if this is the last key in the path ---
            bool isLastKey = (currentIndex == keys.Length - 1);

            if(isLastKey)
            {
                // This should be the final value. Try to get it as a string.
                try
                {
                    string rawPath = currentObject.GetString( currentKey );
                    string expandedPath = Environment.ExpandEnvironmentVariables( rawPath );
                    OnSuccess?.Invoke( expandedPath );
                }
                catch(Exception) // Catches if the value is not a string (e.g., an object or number)
                {
                    OnFailed?.Invoke( "ConfigManager.cs FindPathRecursive() Error: Final key " + currentKey + " does not point to a string value." );
                }
            }
            else // This is an intermediate key, it must point to another JSON object
            {
                try
                {
                    JSON nextObject = currentObject.GetJSON( currentKey );

                    // Continue recursion to the next level
                    FindPathRecursive( nextObject, keys, currentIndex + 1, OnSuccess, OnFailed );
                }
                catch(Exception) // Catches if the value is not a JSON object
                {
                    OnFailed?.Invoke( "ConfigManager.cs FindPathRecursive() Error: Intermediate key " + currentKey + " does not point to a nested object." );
                }
            }

        } //END FindPathRecursive Method

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

            CreateFile( system.options.path, OnSuccess, OnFailed );

        } //END CreateFile Method

        /// <summary>
        /// Generates a config file at the path if one does not already exist
        /// </summary>
        //---------------------------------------//
        public static void CreateFile
        (
            string path,
            Action OnSuccess = null,
            Action<string> OnFailed = null
        )
        //---------------------------------------//
        {
            if(path == null || (path != null && path == "") )
            {
                OnFailed?.Invoke( "ConfigManager.cs CreateFile() passed in path is null or empty" );
                return;
            }

            if(DoesFileExist( path ))
            {
                OnSuccess?.Invoke();
                return;
            }

            //Generate the directory to the file if it doesn't exist
            Directory.CreateDirectory( Path.GetDirectoryName( path ) );

            File.Create( path ).Close();

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

            WriteFileContents
            ( 
                system.options.path, 
                json,
                () => 
                {
                    system.json = json;
                    OnSuccess?.Invoke();
                }, 
                OnFailed 
            );

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

            ReadFileContents
            ( 
                system.options.path, 

                (JSON json)=>
                {
                    system.json = json;
                    OnSuccess?.Invoke( json );
                }, 
                
                OnFailed 
            );

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

        #region PUBLIC - DESTROY FILE

        /// <summary>
        /// If a file exists at the path, destroys it
        /// </summary>
        /// <param name="system"></param>
        //-------------------------------------------------------------------//
        public static void DestroyFile
        ( 
            ConfigManagerSystem system,
            Action OnSuccess = null,
            Action<string> OnFailed = null
        )
        //-------------------------------------------------------------------//
        {

            if(system == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs DestroyFile() passed in ConfigManagerSystem object is null, unable to continue" );
                return;
            }

            if(system.options == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs DestroyFile() passed in ConfigManagerSystem.Options object is null, unable to continue" );
                return;
            }

            if(system.options.path == null || (system.options.path != null && system.options.path == ""))
            {
                OnFailed?.Invoke( "ConfigManager.cs DestroyFile() passed in ConfigManagerSystem.options.path object is null or empty, unable to continue" );
                return;
            }

            DestroyFile( system.options.path, OnSuccess, OnFailed );

        } //END DestroyFile Method

        /// <summary>
        /// If a file exists at the path, destroys it. OnSuccess is called if a file is destroyed or never existed.
        /// </summary>
        /// <param name="system"></param>
        //-------------------------------------------------------------------//
        public static void DestroyFile
        (
            string path,
            Action OnSuccess = null,
            Action<string> OnFailed = null
        )
        //-------------------------------------------------------------------//
        {

            if(path == null || (path != null && path == ""))
            {
                OnFailed?.Invoke( "ConfigManager.cs DestroyFile() passed in path is null or empty, unable to continue" );
                return;
            }

            if(DoesFileExist( path ))
            {
                try
                {
                    File.Delete( path );
                    OnSuccess?.Invoke();
                }
                catch(Exception e)
                {
                    OnFailed?.Invoke( e.ToString() );
                    return;
                }
            }
            else
            {
                OnSuccess?.Invoke();
                return;
            }

        } //END DestroyFile Method

        #endregion

        #region PUBLIC - REPLACE FILE

#if EXT_TOTALJSON

        /// <summary>
        /// Destroys the original file if it exists, and replaces it with the file located at the resources file path and name. Do not include the file type extension
        /// </summary>
        /// <param name="system">The ConfigManagerSystem returned by the Create() function</param>
        /// <param name="replacementFilePathAndName">Path and file name (no extension) to the file within the resources folder you want to use as the replacement</param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //-----------------------------------------//
        public static void ReplaceFileUsingResources
        //-----------------------------------------//
        (
            ConfigManagerSystem system,
            string replacementFilePathAndName,
            Action<JSON> OnSuccess = null,
            Action<string> OnFailed = null
        )
        {

            if(system == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in ConfigManagerSystem object is null, unable to continue" );
                return;
            }

            if(system.options == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in ConfigManagerSystem.Options object is null, unable to continue" );
                return;
            }

            if(system.options.path == null || (system.options.path != null && system.options.path == ""))
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in ConfigManagerSystem.options.path object is null or empty, unable to continue" );
                return;
            }

            ReplaceFileUsingResources
            (
                system.options.path,
                replacementFilePathAndName,
                (JSON json)=>
                {
                    system.json = json;
                    OnSuccess?.Invoke( json );
                    return;
                },
                OnFailed
            );

        } //END ReplaceFileUsingResources Method

        /// <summary>
        /// Destroys the original file if it exists, and replaces it with the file located at the resources file path and name. Do not include the file type extension
        /// </summary>
        /// <param name="path">The direct path of the file you want to replace</param>
        /// <param name="replacementFilePathAndName">Path and file name (no extension) to the file within the resources folder you want to use as the replacement</param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //-----------------------------------------//
        public static void ReplaceFileUsingResources
        //-----------------------------------------//
        (
            string path,
            string replacementFilePathAndName,
            Action<JSON> OnSuccess = null,
            Action<string> OnFailed = null
        )
        {

            if(path == null || (path != null && path == ""))
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFileUsingResources() passed in ConfigManagerSystem.options.path object is null or empty, unable to continue" );
                return;
            }

            UnityEngine.Object obj = Resources.Load( replacementFilePathAndName );

            if(obj == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFileUsingResources() unable to load resources to object, check if the file and the value of replacementFilePathAndName match up = " + replacementFilePathAndName );
                return;
            }

            string jsonAsString = obj.ToString();

            try
            {
                JSON json = JSON.ParseString( jsonAsString );

                ReplaceFile
                (
                    path,
                    json,
                    OnSuccess,
                    OnFailed
                );
            }
            catch(Exception e)
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFileUsingResources() error = " + e.ToString() );
            }

        } //END ReplaceFileUsingResources Method

        /// <summary>
        /// Destroys the original file if it exists, and replaces it with the file located at the replacement file path
        /// </summary>
        /// <param name="system">The ConfigManagerSystem returned by the Create() function</param>
        /// <param name="replacementFilePath">Direct path to the file you want to use as the replacement</param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //-----------------------------------------//
        public static void ReplaceFile
        //-----------------------------------------//
        (
            ConfigManagerSystem system,
            string replacementFilePath,
            Action<JSON> OnSuccess = null,
            Action<string> OnFailed = null
        )
        {

            if(system == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in ConfigManagerSystem object is null, unable to continue" );
                return;
            }

            if(system.options == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in ConfigManagerSystem.Options object is null, unable to continue" );
                return;
            }

            if(system.options.path == null || (system.options.path != null && system.options.path == ""))
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in ConfigManagerSystem.options.path object is null or empty, unable to continue" );
                return;
            }

            ReplaceFile
            ( 
                system.options.path,
                replacementFilePath,
                
                (JSON json)=>
                {
                    system.json = json;
                    OnSuccess?.Invoke( json );
                }, 
                
                OnFailed 
            );

        } //END ReplaceFile Method

        /// <summary>
        /// Destroys the original file if it exists, and replaces it with the file located at the replacement file path
        /// </summary>
        /// <param name="path">The direct path to the original file that you want to replace</param>
        /// <param name="replacementFilePath">Direct path to the file you want to use as the replacement</param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //-----------------------------------------//
        public static void ReplaceFile
        //-----------------------------------------//
        (
            string path,
            string replacementFilePath,
            Action<JSON> OnSuccess = null,
            Action<string> OnFailed = null
        )
        {

            if(path == null || (path != null && path == ""))
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in path object is null or empty, unable to continue" );
                return;
            }

            if(replacementFilePath == null || (replacementFilePath != null && replacementFilePath == ""))
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in replacementFilePath object is null or empty, unable to continue" );
                return;
            }

            if(!DoesFileExist( replacementFilePath ))
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in replacementFilePath does not exist, unable to continue" );
                return;
            }

            //Check if the replacement file path can be read as JSON
            try
            {
                JSON json = JSON.ParseString( path );

                ReplaceFile
                (
                    path,
                    json,
                    OnSuccess,
                    OnFailed
                );
            }
            catch(Exception e) 
            {
                OnFailed?.Invoke( e.ToString() );
                return;
            }

        } //END ReplaceFile Method

        /// <summary>
        /// Destroys the original file if it exists, and replaces it with a new file containing the json data
        /// <param name="system">The ConfigManagerSystem returned by the Create() function</param>
        /// <param name="json">JSON data you want to add to a new file and replace the original with</param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //-----------------------------------------//
        public static void ReplaceFile
        //-----------------------------------------//
        (
            ConfigManagerSystem system,
            JSON json,
            Action OnSuccess = null,
            Action<string> OnFailed = null
        )
        {

            if(system == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in ConfigManagerSystem object is null, unable to continue" );
                return;
            }

            if(system.options == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in ConfigManagerSystem.Options object is null, unable to continue" );
                return;
            }

            if(system.options.path == null || (system.options.path != null && system.options.path == ""))
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in ConfigManagerSystem.options.path object is null or empty, unable to continue" );
                return;
            }

            ReplaceFile
            ( 
                system.options.path, 
                json,
                (JSON json) => 
                {
                    system.json = json;
                    OnSuccess?.Invoke();
                }, 
                OnFailed 
            );

        } //END ReplaceFile Method

        /// <summary>
        /// Destroys the original file if it exists, and replaces it with a new file containing the json data
        /// <param name="path">The path to the original file you want to replace</param>
        /// <param name="json">JSON data you want to add to a new file and replace the original with</param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //-----------------------------------------//
        public static void ReplaceFile
        //-----------------------------------------//
        (
            string path,
            JSON json,
            Action<JSON> OnSuccess = null,
            Action<string> OnFailed = null
        )
        {

            if(path == null || (path != null && path == ""))
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in path object is null or empty, unable to continue" );
                return;
            }

            if(json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in json object is null, unable to continue" );
                return;
            }

            //Destroy the existing file
            DestroyFile
            (
                path,

                //OnSuccess
                ()=>
                {
                    //Original file was destroyed, create a new one
                    CreateFile
                    ( 
                        path,

                        //OnSuccess
                        ()=>
                        {
                            //Write the json to the new file
                            WriteFileContents
                            (
                                path,
                                json,
                                ()=>
                                {
                                    OnSuccess?.Invoke(json);
                                    return;
                                },
                                OnFailed
                            );
                        },

                        OnFailed
                    );
                },

                OnFailed
            );

        } //END ReplaceFile Method

#endif

        #endregion

        #region PUBLIC - LOG DATA TO CONSOLE

        /// <summary>
        /// If the json data object in the ConfigManagerSystem exists, logs it to the console
        /// </summary>
        /// <param name="system"></param>
        //----------------------------------------------------------//
        public static void Log( ConfigManagerSystem system )
        //----------------------------------------------------------//
        {
            if(system == null)
            {
                return;
            }

            if(system.json == null)
            {
                return;
            }

            Debug.Log( system.json.CreatePrettyString() );
        
        } //END Log Method

        #endregion

        #region PUBLIC - DESTROY

        /// <summary>
        /// Destroys a ConfigManagerSystem object, preparing the data for cleanup
        /// </summary>
        /// <param name="system"></param>
        //--------------------------------------------------------------//
        public static void Destroy( ConfigManagerSystem system )
        //--------------------------------------------------------------//
        {
            if(system == null)
            {
                return;
            }

            system = null;

        } //END Destroy Method

        #endregion

    } //END ConfigManager Class

} //END gambit.config Namespace