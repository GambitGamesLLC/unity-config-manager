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
using System.Text.RegularExpressions;

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

        #endregion

        #region PRIVATE - VARIABLES

        /// <summary>
        /// Temporary object reference
        /// </summary>
        private static ConfigManagerSystem localSystem;

        /// <summary>
        /// Temporary object reference
        /// </summary>
        public static ConfigManagerSystem resourcesSystem;

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

            /// <summary>
            /// Path to the config file
            /// </summary>
            public string path = "";

#if EXT_TOTALJSON
            /// <summary>
            /// The JSON contained within the config file
            /// </summary>
            public JSON json;
#endif

            /// <summary>
            /// The version of the configuration this system and data are representing
            /// </summary>
            public int version = -99;

            /// <summary>
            /// The datetime of when this version of the configuration file was created
            /// </summary>
            public DateTime timestamp = DateTime.MinValue;

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
            
            OnSuccess?.Invoke( system );

        } //END Create Method

        #endregion

        #region PUBLIC - GET NESTED STRING

        /// <summary>
        /// Public method to start the recursive search for a nested string. Returns a string that is deserialized
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
        /// Public method to start the recursive search for a nested string. Returns a string that is deserialized
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
            FindStringRecursive( json, keys, 0, OnSuccess, OnFailed );

        } //END GetNestedString Method

        /// <summary>
        /// The private recursive function that traverses the JSON object in search of a string.
        /// </summary>
        /// <param name="currentObject"></param>
        /// <param name="keys"></param>
        /// <param name="currentIndex"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //-------------------------------------------------------------------//
        private static void FindStringRecursive
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
                OnFailed?.Invoke( "ConfigManager.cs FindStringRecursive() Error: Key " + currentKey + " not found at the current level" );
                return;
            }

            // --- Check if this is the last key in the path ---
            bool isLastKey = (currentIndex == keys.Length - 1);

            if(isLastKey)
            {
                // This should be the final value. Try to get it as a string.
                try
                {
                    string text = currentObject.GetString( currentKey );
                    OnSuccess?.Invoke( text );
                }
                catch(Exception) // Catches if the value is not a string (e.g., an object or number)
                {
                    OnFailed?.Invoke( "ConfigManager.cs FindStringRecursive() Error: Final key " + currentKey + " does not point to a string value." );
                }
            }
            else // This is an intermediate key, it must point to another JSON object
            {
                try
                {
                    JSON nextObject = currentObject.GetJSON( currentKey );

                    // Continue recursion to the next level
                    FindStringRecursive( nextObject, keys, currentIndex + 1, OnSuccess, OnFailed );
                }
                catch(Exception) // Catches if the value is not a JSON object
                {
                    OnFailed?.Invoke( "ConfigManager.cs FindStringRecursive() Error: Intermediate key " + currentKey + " does not point to a nested object." );
                }
            }

        } //END FindStringRecursive Method

        #endregion

        #region PUBLIC - GET NESTED PATH

        /// <summary>
        /// Public method to start the recursive search for a nested path string. Returns it with any environment variables expanded, and like all strings parsed from TotalJSON, comes to us fully deserialized
        /// </summary>
        /// <param name="system">The ConfigManagerSystem object with a json data contained within</param>
        /// <param name="keys">An array of keys representing the path to the desired value.</param>
        /// <param name="OnSuccess">Callback invoked with the processed string if found.</param>
        /// <param name="OnFailed">Callback invoked with an error message if not found.</param>
        //------------------------------------------------------------------------------------------------//
        public static void GetNestedPath
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
                OnFailed?.Invoke( "ConfigManager.cs GetNestedPath() passed in ConfigManagerSystem object is null" );
                return;
            }

            if(system.json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedPath() passed in ConfigManagerSystem.json object is null" );
                return;
            }

            GetNestedPath( system.json, keys, OnSuccess, OnFailed );

        } //END GetNestedPath Method

        /// <summary>
        /// Public method to start the recursive search for a nested path string. Returns it fully deserialized with any environment variables expanded
        /// </summary>
        /// <param name="json">The root TotalJSON object to search within.</param>
        /// <param name="keys">An array of keys representing the path to the desired value.</param>
        /// <param name="OnSuccess">Callback invoked with the processed string if found.</param>
        /// <param name="OnFailed">Callback invoked with an error message if not found.</param>
        //------------------------------------------------------------------------------------------------//
        public static void GetNestedPath
        (
            JSON json,
            string[ ] keys,
            Action<string> OnSuccess,
            Action<string> OnFailed
        )
        //------------------------------------------------------------------------------------------------//
        {

            if(json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedPath() Error: Root JSON object is null." );
                return;
            }
            if(keys == null || keys.Length == 0)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedPath() Error: Keys array is null or empty." );
                return;
            }

            // --- Start Recursive Search ---
            FindStringRecursive
            ( 
                json, 
                keys, 
                0, 
                (string text)=>
                {
                    string expandedPath = Environment.ExpandEnvironmentVariables( text );
                    OnSuccess?.Invoke( expandedPath );
                    return;
                }, 
                OnFailed 
            );

        } //END GetNestedPath Method

        #endregion

        #region PUBLIC - GET NESTED DATETIME

        /// <summary>
        /// Public method to start the recursive search for a nested datetime string and returns it as a DateTime object
        /// </summary>
        /// <param name="system">The ConfigManagerSystem object with a json data contained within</param>
        /// <param name="keys">An array of keys representing the path to the desired value.</param>
        /// <param name="OnSuccess">Callback invoked with the processed string if found.</param>
        /// <param name="OnFailed">Callback invoked with an error message if not found.</param>
        //------------------------------------------------------------------------------------------------//
        public static void GetNestedDateTime
        (
            ConfigManagerSystem system,
            string[ ] keys,
            Action<DateTime> OnSuccess,
            Action<string> OnFailed
        )
        //------------------------------------------------------------------------------------------------//
        {
            if(system == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedDateTime() passed in ConfigManagerSystem object is null" );
                return;
            }

            if(system.json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedDateTime() passed in ConfigManagerSystem.json object is null" );
                return;
            }

            GetNestedDateTime( system.json, keys, OnSuccess, OnFailed );

        } //END GetNestedDateTime Method

        /// <summary>
        /// Public method to start the recursive search for a nested datetime.
        /// </summary>
        /// <param name="json">The root TotalJSON object to search within.</param>
        /// <param name="keys">An array of keys representing the path to the desired value.</param>
        /// <param name="OnSuccess">Callback invoked with the processed string if found.</param>
        /// <param name="OnFailed">Callback invoked with an error message if not found.</param>
        //------------------------------------------------------------------------------------------------//
        public static void GetNestedDateTime
        (
            JSON json,
            string[ ] keys,
            Action<DateTime> OnSuccess,
            Action<string> OnFailed
        )
        //------------------------------------------------------------------------------------------------//
        {

            if(json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedDateTime() Error: Root JSON object is null." );
                return;
            }
            if(keys == null || keys.Length == 0)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedDateTime() Error: Keys array is null or empty." );
                return;
            }

            // --- Start Recursive Search ---
            FindDateTimeRecursive( json, keys, 0, OnSuccess, OnFailed );

        } //END GetNestedDateTime Method

        /// <summary>
        /// The private recursive function that traverses the JSON object in search of a Datetime.
        /// </summary>
        /// <param name="currentObject"></param>
        /// <param name="keys"></param>
        /// <param name="currentIndex"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //-------------------------------------------------------------------//
        private static void FindDateTimeRecursive
        (
            JSON currentObject,
            string[ ] keys,
            int currentIndex,
            Action<DateTime> OnSuccess,
            Action<string> OnFailed
        )
        //-------------------------------------------------------------------//
        {
            string currentKey = keys[ currentIndex ];

            // --- Check if the current key exists ---
            if(!currentObject.ContainsKey( currentKey ))
            {
                OnFailed?.Invoke( "ConfigManager.cs FindDateTimeRecursive() Error: Key " + currentKey + " not found at the current level" );
                return;
            }

            // --- Check if this is the last key in the path ---
            bool isLastKey = (currentIndex == keys.Length - 1);

            if(isLastKey)
            {
                // This should be the final value. Try to get it as a string.
                try
                {
                    string text = currentObject.GetString( currentKey );
                    DateTime dateTime = DateTime.Parse( text );
                    OnSuccess?.Invoke( dateTime );
                }
                catch(Exception) // Catches if the value is not a string (e.g., an object or number)
                {
                    OnFailed?.Invoke( "ConfigManager.cs FindDateTimeRecursive() Error: Final key " + currentKey + " does not point to a string/datetime value." );
                }
            }
            else // This is an intermediate key, it must point to another JSON object
            {
                try
                {
                    JSON nextObject = currentObject.GetJSON( currentKey );

                    // Continue recursion to the next level
                    FindDateTimeRecursive( nextObject, keys, currentIndex + 1, OnSuccess, OnFailed );
                }
                catch(Exception) // Catches if the value is not a JSON object
                {
                    OnFailed?.Invoke( "ConfigManager.cs FindDateTimeRecursive() Error: Intermediate key " + currentKey + " does not point to a nested object." );
                }
            }

        } //END FindDateTimeRecursive Method

        #endregion

        #region PUBLIC - GET NESTED INTEGER

        /// <summary>
        /// Public method to start the recursive search for a nested integer.
        /// </summary>
        /// <param name="system">The ConfigManagerSystem object with a json data contained within</param>
        /// <param name="keys">An array of keys representing the path to the desired value.</param>
        /// <param name="OnSuccess">Callback invoked with the processed string if found.</param>
        /// <param name="OnFailed">Callback invoked with an error message if not found.</param>
        //------------------------------------------------------------------------------------------------//
        public static void GetNestedInteger
        (
            ConfigManagerSystem system,
            string[ ] keys,
            Action<int> OnSuccess,
            Action<string> OnFailed
        )
        //------------------------------------------------------------------------------------------------//
        {
            if(system == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedInteger() passed in ConfigManagerSystem object is null" );
                return;
            }

            if(system.json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedInteger() passed in ConfigManagerSystem.json object is null" );
                return;
            }

            GetNestedInteger( system.json, keys, OnSuccess, OnFailed );

        } //END GetNestedInteger Method

        /// <summary>
        /// Public method to start the recursive search for a nested integer.
        /// </summary>
        /// <param name="system">The ConfigManagerSystem object with a json data contained within</param>
        /// <param name="keys">An array of keys representing the path to the desired value.</param>
        /// <param name="OnSuccess">Callback invoked with the processed string if found.</param>
        /// <param name="OnFailed">Callback invoked with an error message if not found.</param>
        //------------------------------------------------------------------------------------------------//
        public static void GetNestedInteger
        (
            JSON json,
            string[ ] keys,
            Action<int> OnSuccess,
            Action<string> OnFailed
        )
        //------------------------------------------------------------------------------------------------//
        {
            if(json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedInteger() Error: Root JSON object is null." );
                return;
            }
            if(keys == null || keys.Length == 0)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedInteger() Error: Keys array is null or empty." );
                return;
            }

            // --- Start Recursive Search ---
            FindIntegerRecursive( json, keys, 0, OnSuccess, OnFailed );

        } //END GetNestedInteger Method

        /// <summary>
        /// The private recursive function that traverses the JSON object in search of a integer.
        /// </summary>
        /// <param name="currentObject"></param>
        /// <param name="keys"></param>
        /// <param name="currentIndex"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //-------------------------------------------------------------------//
        private static void FindIntegerRecursive
        (
            JSON currentObject,
            string[ ] keys,
            int currentIndex,
            Action<int> OnSuccess,
            Action<string> OnFailed
        )
        //-------------------------------------------------------------------//
        {
            string currentKey = keys[ currentIndex ];

            // --- Check if the current key exists ---
            if(!currentObject.ContainsKey( currentKey ))
            {
                OnFailed?.Invoke( "ConfigManager.cs FindIntegerRecursive() Error: Key " + currentKey + " not found at the current level" );
                return;
            }

            // --- Check if this is the last key in the path ---
            bool isLastKey = (currentIndex == keys.Length - 1);

            if(isLastKey)
            {
                // This should be the final value. Try to get it as a string.
                try
                {
                    int value = currentObject.GetInt( currentKey );
                    OnSuccess?.Invoke( value );
                }
                catch(Exception) // Catches if the value is not a integer
                {
                    OnFailed?.Invoke( "ConfigManager.cs FindIntegerRecursive() Error: Final key " + currentKey + " does not point to a integer value." );
                }
            }
            else // This is an intermediate key, it must point to another JSON object
            {
                try
                {
                    JSON nextObject = currentObject.GetJSON( currentKey );

                    // Continue recursion to the next level
                    FindIntegerRecursive( nextObject, keys, currentIndex + 1, OnSuccess, OnFailed );
                }
                catch(Exception) // Catches if the value is not a JSON object
                {
                    OnFailed?.Invoke( "ConfigManager.cs FindIntegerRecursive() Error: Intermediate key " + currentKey + " does not point to a nested object." );
                }
            }

        } //END FindIntegerRecursive Method

        #endregion

        #region PUBLIC - GET NESTED FLOAT

        /// <summary>
        /// Public method to start the recursive search for a nested float.
        /// </summary>
        /// <param name="system">The ConfigManagerSystem object with a json data contained within</param>
        /// <param name="keys">An array of keys representing the path to the desired value.</param>
        /// <param name="OnSuccess">Callback invoked with the processed string if found.</param>
        /// <param name="OnFailed">Callback invoked with an error message if not found.</param>
        //------------------------------------------------------------------------------------------------//
        public static void GetNestedFloat
        (
            ConfigManagerSystem system,
            string[ ] keys,
            Action<float> OnSuccess,
            Action<string> OnFailed
        )
        //------------------------------------------------------------------------------------------------//
        {
            if(system == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedFloat() passed in ConfigManagerSystem object is null" );
                return;
            }

            if(system.json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedFloat() passed in ConfigManagerSystem.json object is null" );
                return;
            }

            GetNestedFloat( system.json, keys, OnSuccess, OnFailed );

        } //END GetNestedFloat Method

        /// <summary>
        /// Public method to start the recursive search for a nested float.
        /// </summary>
        /// <param name="system">The ConfigManagerSystem object with a json data contained within</param>
        /// <param name="keys">An array of keys representing the path to the desired value.</param>
        /// <param name="OnSuccess">Callback invoked with the processed string if found.</param>
        /// <param name="OnFailed">Callback invoked with an error message if not found.</param>
        //------------------------------------------------------------------------------------------------//
        public static void GetNestedFloat
        (
            JSON json,
            string[ ] keys,
            Action<float> OnSuccess,
            Action<string> OnFailed
        )
        //------------------------------------------------------------------------------------------------//
        {
            if(json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedFloat() Error: Root JSON object is null." );
                return;
            }
            if(keys == null || keys.Length == 0)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedFloat() Error: Keys array is null or empty." );
                return;
            }

            // --- Start Recursive Search ---
            FindFloatRecursive( json, keys, 0, OnSuccess, OnFailed );

        } //END GetNestedFloat Method

        /// <summary>
        /// The private recursive function that traverses the JSON object in search of a float.
        /// </summary>
        /// <param name="currentObject"></param>
        /// <param name="keys"></param>
        /// <param name="currentIndex"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //-------------------------------------------------------------------//
        private static void FindFloatRecursive
        (
            JSON currentObject,
            string[ ] keys,
            int currentIndex,
            Action<float> OnSuccess,
            Action<string> OnFailed
        )
        //-------------------------------------------------------------------//
        {
            string currentKey = keys[ currentIndex ];

            // --- Check if the current key exists ---
            if(!currentObject.ContainsKey( currentKey ))
            {
                OnFailed?.Invoke( "ConfigManager.cs FindFloatRecursive() Error: Key " + currentKey + " not found at the current level" );
                return;
            }

            // --- Check if this is the last key in the path ---
            bool isLastKey = (currentIndex == keys.Length - 1);

            if(isLastKey)
            {
                // This should be the final value. Try to get it as a string.
                try
                {
                    float value = currentObject.GetFloat( currentKey );
                    OnSuccess?.Invoke( value );
                }
                catch(Exception) // Catches if the value is not a float
                {
                    OnFailed?.Invoke( "ConfigManager.cs FindFloatRecursive() Error: Final key " + currentKey + " does not point to a float value." );
                }
            }
            else // This is an intermediate key, it must point to another JSON object
            {
                try
                {
                    JSON nextObject = currentObject.GetJSON( currentKey );

                    // Continue recursion to the next level
                    FindFloatRecursive( nextObject, keys, currentIndex + 1, OnSuccess, OnFailed );
                }
                catch(Exception) // Catches if the value is not a JSON object
                {
                    OnFailed?.Invoke( "ConfigManager.cs FindFloatRecursive() Error: Intermediate key " + currentKey + " does not point to a nested object." );
                }
            }

        } //END FindFloatRecursive Method

        #endregion

        #region PUBLIC - GET NESTED BOOLEAN

        /// <summary>
        /// Public method to start the recursive search for a nested boolean.
        /// </summary>
        /// <param name="system">The ConfigManagerSystem object with a json data contained within</param>
        /// <param name="keys">An array of keys representing the path to the desired value.</param>
        /// <param name="OnSuccess">Callback invoked with the processed string if found.</param>
        /// <param name="OnFailed">Callback invoked with an error message if not found.</param>
        //------------------------------------------------------------------------------------------------//
        public static void GetNestedBool
        (
            ConfigManagerSystem system,
            string[ ] keys,
            Action<bool> OnSuccess,
            Action<string> OnFailed
        )
        //------------------------------------------------------------------------------------------------//
        {
            if(system == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedBool() passed in ConfigManagerSystem object is null" );
                return;
            }

            if(system.json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedBool() passed in ConfigManagerSystem.json object is null" );
                return;
            }

            GetNestedBool( system.json, keys, OnSuccess, OnFailed );

        } //END GetNestedBool Method

        /// <summary>
        /// Public method to start the recursive search for a nested bool.
        /// </summary>
        /// <param name="system">The ConfigManagerSystem object with a json data contained within</param>
        /// <param name="keys">An array of keys representing the path to the desired value.</param>
        /// <param name="OnSuccess">Callback invoked with the processed string if found.</param>
        /// <param name="OnFailed">Callback invoked with an error message if not found.</param>
        //------------------------------------------------------------------------------------------------//
        public static void GetNestedBool
        (
            JSON json,
            string[ ] keys,
            Action<bool> OnSuccess,
            Action<string> OnFailed
        )
        //------------------------------------------------------------------------------------------------//
        {
            if(json == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedBool() Error: Root JSON object is null." );
                return;
            }
            if(keys == null || keys.Length == 0)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNestedBool() Error: Keys array is null or empty." );
                return;
            }

            // --- Start Recursive Search ---
            FindBoolRecursive( json, keys, 0, OnSuccess, OnFailed );

        } //END GetNestedBool Method

        /// <summary>
        /// The private recursive function that traverses the JSON object in search of a boolean.
        /// </summary>
        /// <param name="currentObject"></param>
        /// <param name="keys"></param>
        /// <param name="currentIndex"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //-------------------------------------------------------------------//
        private static void FindBoolRecursive
        (
            JSON currentObject,
            string[ ] keys,
            int currentIndex,
            Action<bool> OnSuccess,
            Action<string> OnFailed
        )
        //-------------------------------------------------------------------//
        {
            string currentKey = keys[ currentIndex ];

            // --- Check if the current key exists ---
            if(!currentObject.ContainsKey( currentKey ))
            {
                OnFailed?.Invoke( "ConfigManager.cs FindBoolRecursive() Error: Key " + currentKey + " not found at the current level" );
                return;
            }

            // --- Check if this is the last key in the path ---
            bool isLastKey = (currentIndex == keys.Length - 1);

            if(isLastKey)
            {
                // This should be the final value. Try to get it as a string.
                try
                {
                    bool value = currentObject.GetBool( currentKey );
                    OnSuccess?.Invoke( value );
                }
                catch(Exception) // Catches if the value is not a float
                {
                    OnFailed?.Invoke( "ConfigManager.cs FindBoolRecursive() Error: Final key " + currentKey + " does not point to a bool value." );
                }
            }
            else // This is an intermediate key, it must point to another JSON object
            {
                try
                {
                    JSON nextObject = currentObject.GetJSON( currentKey );

                    // Continue recursion to the next level
                    FindBoolRecursive( nextObject, keys, currentIndex + 1, OnSuccess, OnFailed );
                }
                catch(Exception) // Catches if the value is not a JSON object
                {
                    OnFailed?.Invoke( "ConfigManager.cs FindFloatRecursive() Error: Intermediate key " + currentKey + " does not point to a nested object." );
                }
            }

        } //END FindBoolRecursive Method

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

            return DoesFileExist( system.path );

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

            CreateFile( system.path, OnSuccess, OnFailed );

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
        /// Writes the file with new json data referenced by the path value of the ConfigManagerSystem. When successfull we will store the new json data in the system object for future reference.
        /// </summary>
        /// <param name="system">The ConfigManagerSystem object passed back during Create. If successfull, we will store the new json data in this object for future reference</param>
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
                system.path, 
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
        /// Reads a file and returns json data. Overwrites the ConfigManagerSystem.json object when successfull
        /// </summary>
        /// <param name="system"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
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
                system.path, 

                (JSON json)=>
                {
                    system.json = json;

                    ReadConfigInfoFromData( system, OnSuccess, OnFailed );
                }, 
                
                OnFailed 
            );

        } //END ReadFileContents Method

        /// <summary>
        /// Reads a file and returns json data
        /// </summary>
        /// <param name="path"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
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

        /// <summary>
        /// Reads the data and sets the version number and datetime on the ConfigManagerSystem if the values are found
        /// </summary>
        /// <param name="system"></param>
        /// <param name="json"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //-----------------------------------------------//
        private static void ReadConfigInfoFromData
        //-----------------------------------------------//
        (
            ConfigManagerSystem system,
            Action<JSON> OnSuccess,
            Action<string> OnFailed
        )
        {
            //Store the version number, then look for the timestamp
            StoreVersionNumberFromData
            (
                system,
                ()=>
                {
                    OnSuccess?.Invoke( system.json );
                },
                OnFailed
            );

        } //END ReadConfigInfoFromData

        /// <summary>
        /// If we find the 'version' value in the 'config' object, store it in the ConfigManagerSystem object
        /// </summary>
        /// <param name="system"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //------------------------------------------------------//
        private static void StoreVersionNumberFromData
        //------------------------------------------------------//
        (
            ConfigManager.ConfigManagerSystem system,
            Action OnSuccess,
            Action<string> OnFailed
        )
        {
            GetNestedInteger
            (
                system,
                new string[ ] { "config", "version" },
                ( int value ) =>
                {
                    system.version = value;

                    StoreTimestampFromData
                    (
                        system,
                        OnSuccess,
                        OnFailed
                    );
                },
                OnFailed
            );

        } //END StoreVersionNumberFromData Method

        /// <summary>
        /// If we find the 'timestamp' value in the 'config' object, store it in the ConfigManagerSystem object
        /// </summary>
        /// <param name="system"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //----------------------------------------------//
        private static void StoreTimestampFromData
        //----------------------------------------------//
        (
            ConfigManager.ConfigManagerSystem system,
            Action OnSuccess,
            Action<string> OnFailed
        )
        {
            GetNestedDateTime
            (
                system,
                new string[ ] { "config", "timestamp" },
                ( DateTime value ) =>
                {
                    system.timestamp = value;

                    StorePathFromData
                    (
                        system,
                        OnSuccess,
                        OnFailed
                    );
                },
                OnFailed
            );

        } //END StoreTimestampFromData Method

        /// <summary>
        /// If we find the 'path' value in the 'config' object, store it in the ConfigManagerSystem object
        /// </summary>
        /// <param name="system"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //----------------------------------------------//
        private static void StorePathFromData
        //----------------------------------------------//
        (
            ConfigManager.ConfigManagerSystem system,
            Action OnSuccess,
            Action<string> OnFailed
        )
        {
            GetNestedPath
            (
                system,
                new string[ ] { "config", "path" },
                ( string value ) =>
                {
                    system.path = value;

                    OnSuccess?.Invoke();
                },
                OnFailed
            );

        } //END StorePathFromData Method

#endif

        #endregion

        #region PUBLIC - READ FILE CONTENTS FROM RESOURCES

#if EXT_TOTALJSON

        /// <summary>
        /// Returns the JSON object from a config file located in the resources folder.
        /// In addition, using this version of the function will fill in the 'json', 'version' and 'timestamp' variables for your ConfigManagerSystem object
        /// </summary>
        /// <param name="configFilePathAndNameInResources"></param>
        /// <returns></returns>
        //------------------------------------------------------------------------------------------------//
        public static void ReadFileContentsFromResources
        (
            ConfigManagerSystem system,
            string configFilePathAndNameInResources,
            Action<JSON> OnSuccess,
            Action<string> OnFailed
        )
        //------------------------------------------------------------------------------------------------//
        {
            if(system == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs ReadFileContentsFromResources() passed in ConfigManagerSystem object is null" );
            }

            ReadFileContentsFromResources
            (
                configFilePathAndNameInResources,
                (JSON json)=>
                {
                    system.json = json;

                    ReadConfigInfoFromData( system, OnSuccess, OnFailed );
                },
                OnFailed
            );

        } //END ReadFileContentsFromResources

        /// <summary>
        /// Returns the JSON object from a config file located in the resources folder
        /// </summary>
        /// <param name="configFilePathAndNameInResources"></param>
        /// <returns></returns>
        //------------------------------------------------------------------------------------------------//
        public static void ReadFileContentsFromResources
        (
            string configFilePathAndNameInResources,
            Action<JSON> OnSuccess,
            Action<string> OnFailed
        )
        //------------------------------------------------------------------------------------------------//
        {
            if(string.IsNullOrEmpty( configFilePathAndNameInResources ))
            {
                OnFailed?.Invoke( "ConfigManager.cs ReadFileContentsFromResources" );
            }

            try
            {
                UnityEngine.Object obj = Resources.Load( configFilePathAndNameInResources );

                if(obj == null)
                {
                    OnFailed?.Invoke( "ConfigManager.cs ReadFileContentsFromResources() unable to load resources to object, check if the file and the value of configFilePathAndNameInResources match up = " + configFilePathAndNameInResources );
                    return;
                }

                string jsonAsString = obj.ToString();

                JSON json = JSON.ParseString( jsonAsString );

                OnSuccess?.Invoke( json );
                return;
            }
            catch( Exception e ) 
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFileUsingResources() error = " + e.ToString() );
            }

        } //END ReadFileContentsFromResources

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

            if(system.path == null || (system.path != null && system.path == ""))
            {
                OnFailed?.Invoke( "ConfigManager.cs DestroyFile() passed in ConfigManagerSystem.options.path object is null or empty, unable to continue" );
                return;
            }

            DestroyFile( system.path, OnSuccess, OnFailed );

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

            if(system.path == null || (system.path != null && system.path == ""))
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in ConfigManagerSystem.options.path object is null or empty, unable to continue" );
                return;
            }

            ReplaceFileUsingResources
            (
                system.path,
                replacementFilePathAndName,
                (JSON json)=>
                {
                    system.json = json;
                    ReadConfigInfoFromData( system, OnSuccess, OnFailed );
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

            if(system.path == null || (system.path != null && system.path == ""))
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in ConfigManagerSystem.options.path object is null or empty, unable to continue" );
                return;
            }

            ReplaceFile
            ( 
                system.path,
                replacementFilePath,
                
                (JSON json)=>
                {
                    system.json = json;
                    ReadConfigInfoFromData( system, OnSuccess, OnFailed );
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

            if(system.path == null || (system.path != null && system.path == ""))
            {
                OnFailed?.Invoke( "ConfigManager.cs ReplaceFile() passed in ConfigManagerSystem.options.path object is null or empty, unable to continue" );
                return;
            }

            ReplaceFile
            ( 
                system.path, 
                json,
                (JSON json) => 
                {
                    system.json = json;
                    ReadConfigInfoFromData( system, OnSuccess, OnFailed );
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

        #region PUBLIC - UN-ESCAPE & EXPAND PATH

        /// <summary>
        /// Converts the path to expand environment variables, also unescapes character sequences like \\ or \n.
        /// This version of the function also stores the result in the path variable in the options object
        /// </summary>
        //---------------------------------------------//
        public static string UnescapeAndExpandPath( ConfigManagerSystem system )
        //---------------------------------------------//
        {
            system.path = UnescapeAndExpandPath( system.path );

            return system.path;

        } //END UnescapeAndExpandPath Method

        /// <summary>
        /// Converts the path to expand environment variables, also unescapes character sequences like \\ or \n
        /// </summary>
        //---------------------------------------------//
        public static string UnescapeAndExpandPath( string path )
        //---------------------------------------------//
        {
            if(string.IsNullOrEmpty( path ))
            {
                Debug.LogError( "ConfigManager.cs UnescapeAndExpandPath() path is null or empty." );
                return path;
            }

            //Unescape the string
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS0168
            try
            {
                path = Regex.Unescape( path );
            }
            catch(Exception e)
            {
                //Debug.LogWarning( e.ToString() );
            }
#pragma warning restore CS0168
#pragma warning restore IDE0059 // Unnecessary assignment of a value

            //Expand any environment variables for the configPath
            path = System.Environment.ExpandEnvironmentVariables( path );

            return path;

        } //END UnescapeAndExpandPath Method

        #endregion

        #region PUBLIC - GET NEWER SYSTEM

        /// <summary>
        /// Given two ConfigManagerSystems, returns the one that has a newer version number and timestamp
        /// </summary>
        /// <param name="system1"></param>
        /// <param name="system2"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //------------------------------------------//
        public static void GetNewerSystem
        //------------------------------------------//
        (
            ConfigManagerSystem system1,
            ConfigManagerSystem system2,
            Action<ConfigManagerSystem> OnSuccess,
            Action<string> OnFailed
        )
        {
            if(system1 == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNewerSystem() passed in system1 object is null" );
            }

            if(system2 == null)
            {
                OnFailed?.Invoke( "ConfigManager.cs GetNewerSystem() passed in system2 object is null" );
            }


            if(system1.version < system2.version)
            {
                //System 1 has an older version number
                OnSuccess?.Invoke( system2 );
            }
            else if(system1.version > system2.version)
            {
                //System 2 has a older version number
                OnSuccess?.Invoke( system1 );
            }
            else
            {
                //Version numbers are the same, compare timestamps
                if(system1.timestamp < system2.timestamp)
                {
                    //System 1 has an older timestamp
                    OnSuccess?.Invoke( system2 );
                }
                else if(system1.timestamp > system2.timestamp)
                {
                    //System2 has an older timestamp
                    OnSuccess?.Invoke( system1 );
                }
                else
                {
                    //Both are equal, return system1
                    OnSuccess?.Invoke( system1 );
                }
            }

        } //END IsTimestampNewer Method

        #endregion

        #region PUBLIC - IS NEWER?

        /// <summary>
        /// Returns true if the first system passed in is newer than the second system
        /// </summary>
        /// <param name="system1"></param>
        /// <param name="system2"></param>
        /// <returns></returns>
        //-----------------------------------------//
        public static bool IsNewer
        //-----------------------------------------//
        (
            ConfigManagerSystem system1,
            ConfigManagerSystem system2
        )
        {

            if(system1.version < system2.version)
            {
                //System 1 has an older version number
                return false;
            }
            else if(system1.version > system2.version)
            {
                //System 2 has a older version number
                return true;
            }
            else
            {
                //Version numbers are the same, compare timestamps
                if(system1.timestamp < system2.timestamp)
                {
                    //System 1 has an older timestamp
                    return false;
                }
                else if(system1.timestamp > system2.timestamp)
                {
                    //System2 has an older timestamp
                    return true;
                }
                else
                {
                    //Both are equal, return system1
                    return false;
                }
            }

        } //END IsNewer Method

        #endregion

        #region PUBLIC - UPDATE LOCAL DATA WITH INFO FROM RESOURCES AND RETURN LOCAL SYSTEM

#if EXT_TOTALJSON

        /// <summary>
        /// Replaces the local config with the one in resources, but only if the local config is missing or older. Returns the local config
        /// </summary>
        /// <param name="pathAndFilenameToConfigInResources"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //----------------------------------------------------------------//
        public static void UpdateLocalDataAndReturn
        //----------------------------------------------------------------//
        (
            string pathAndFilenameToConfigInResources,
            bool showDebugLogs,
            Action<ConfigManagerSystem> OnSuccess,
            Action<string> OnFailed
        )
        {
            if(string.IsNullOrEmpty( pathAndFilenameToConfigInResources ))
            {
                OnFailed?.Invoke( "ConfigManager.cs UpdateLocalDataAndReturn() passed in pathAndFilenameToConfigInResources is null or empty" );
                return;
            }

            //Reset our temp variables
            resourcesSystem = null;
            localSystem = null;

            int count = 0;
            int total = 2;

            //Create the resources and local configs
            Create
            (
                new Options() { showDebugLogs = showDebugLogs },
                ( ConfigManagerSystem system )=>
                {
                    resourcesSystem = system;
                    count++;
                    if(count == total)
                    {
                        LoadResourcesConfig( pathAndFilenameToConfigInResources, OnSuccess, OnFailed );
                    }
                },
                OnFailed
            );

            Create
            (
                new Options() { showDebugLogs = showDebugLogs },
                ( ConfigManagerSystem system ) =>
                {
                    localSystem = system;
                    count++;
                    if(count == total)
                    {
                        LoadResourcesConfig( pathAndFilenameToConfigInResources, OnSuccess, OnFailed );
                    }
                },
                OnFailed
            );

        } //END UpdateLocalDataAndReturn Method

        /// <summary>
        /// Next step is to load the resources config
        /// </summary>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //------------------------------------------------------------//
        private static void LoadResourcesConfig
        //------------------------------------------------------------//
        ( 
            string pathAndFilenameToConfigInResources,
            Action<ConfigManagerSystem> OnSuccess, 
            Action<string> OnFailed 
        )
        {
            ReadFileContentsFromResources
            (
                resourcesSystem,
                pathAndFilenameToConfigInResources,
                (JSON json) => 
                {
                    //Resources is loaded, load the local config next
                    LoadLocalConfig
                    (
                        resourcesSystem.path,
                        pathAndFilenameToConfigInResources,
                        OnSuccess,
                        OnFailed
                    );
                },
                OnFailed

            );

        } //END LoadResourcesConfig

        /// <summary>
        /// Next step is to load the local config, use the local path from resources data using the 'config' object and 'path' key
        /// </summary>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFailed"></param>
        //------------------------------------------------------------//
        private static void LoadLocalConfig
        //------------------------------------------------------------//
        (
            string pathWithFilenameAndExtensionToConfigFileOnLocal,
            string pathAndFilenameToConfigInResources,
            Action<ConfigManagerSystem> OnSuccess,
            Action<string> OnFailed
        )
        {
            localSystem.path = pathWithFilenameAndExtensionToConfigFileOnLocal;

            Debug.Log( "LoadLocalConfig() start ... localSystem.path = " + localSystem.path );

            ReadFileContents
            (
                localSystem,
                (JSON json ) =>
                {
                    //Local file found, is it newer?
                    if(IsNewer( localSystem, resourcesSystem ))
                    {
                        //Local file is newer, return it
                        OnSuccess?.Invoke( localSystem );
                        return;
                    }
                    else
                    {
                        //Local file is older, replace it with the resources data,
                        //replace it using what we have in resources then return the new local config system
                        ReplaceFileUsingResources
                        (
                            localSystem,
                            pathAndFilenameToConfigInResources,
                            ( JSON json ) =>
                            {
                                OnSuccess?.Invoke( localSystem );
                            },
                            OnFailed
                        );
                    }
                },

                (string error)=>
                {
                    //Local file not found, replace it using what we have in resources then return the new local config system
                    ReplaceFileUsingResources
                    (
                        localSystem,
                        pathAndFilenameToConfigInResources,
                        (JSON json) =>
                        {
                            OnSuccess?.Invoke( localSystem );
                        },
                        OnFailed
                    );
                }

            );

        } //END LoadLocalConfig

#endif

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