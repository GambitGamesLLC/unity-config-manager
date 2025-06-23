/********************************************************
 * ConfigManager.cs
 * 
 * Reads and writes a json config file
 * 
 ********************************************************/


#region IMPORTS

using System;
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
        //public static ConfigSystem system;

        #endregion

    } //END ConfigManager Class

} //END gambit.config Namespace