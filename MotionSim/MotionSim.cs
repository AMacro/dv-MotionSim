using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using MotionSim.Components;
using UnityModManagerNet;

namespace MotionSim;

public static class MotionSim
{
    private const string LOG_FILE = "MotionSim.log";

    public static UnityModManager.ModEntry Mod;
    public static Settings Settings;
    public static Dictionary<TrainCar,Telemetry> Telemetries = new Dictionary<TrainCar, Telemetry>();

    public static string Ver {
        get {
            AssemblyInformationalVersionAttribute info = (AssemblyInformationalVersionAttribute)typeof(MotionSim).Assembly.
                                                            GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                                                            .FirstOrDefault();

            if (info == null || Settings.ForceJSON)
                return Mod .Info.Version;

            return info.InformationalVersion.Split('+')[0];
        }
    }

    [UsedImplicitly]
    private static bool Load(UnityModManager.ModEntry modEntry)
    {
        Mod = modEntry;
       
        Settings = Settings.Load(modEntry);
        Mod.OnGUI = Settings.Draw;
        Mod.OnSaveGUI = Settings.Save;

        Harmony harmony = null;
        

        try
        {
            
            File.Delete(LOG_FILE);

            Log($"{typeof(MotionSim)} JSON Version: {Mod.Info.Version}, Internal Version: {Ver} ");

            Log("Patching...");
            harmony = new Harmony(Mod.Info.Id);
            harmony.PatchAll();
            

        }
        catch (Exception ex)
        {
            LogException("Failed to load:", ex);
            harmony?.UnpatchAll();
            return false;
        }
        
        return true;
    }

    #region Logging

    public static void LogDebug(Func<object> resolver)
    {
        if (!Settings.DebugLogging)
            return;
        WriteLog($"[Debug] {resolver.Invoke()}");
    }

    public static void Log(object msg)
    {
        WriteLog($"[Info] {msg}");
    }

    public static void LogWarning(object msg)
    {
        WriteLog($"[Warning] {msg}");
    }

    public static void LogError(object msg)
    {
        WriteLog($"[Error] {msg}");
    }

    public static void LogException(object msg, Exception e)
    {
        Mod.Logger.LogException($"{msg}", e);
    }

    private static void WriteLog(string msg)
    {
        string str = $"[{DateTime.Now.ToUniversalTime():HH:mm:ss.fff}] {msg}";
        /*
         if (Settings.EnableLogFile)
            File.AppendAllLines(LOG_FILE, new[] { str });
        */
        Mod.Logger.Log(str);
    }

    #endregion
}
