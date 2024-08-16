using System;
using UnityEngine;
using UnityModManagerNet;
using Console = DV.Console;

namespace MotionSim;

[Serializable]
[DrawFields(DrawFieldMask.OnlyDrawAttr)]
public class Settings : UnityModManager.ModSettings, IDrawable
{
    public static Action<Settings> OnSettingsUpdated;

    public int SettingsVer = 0;

    [Space(10)]
    [Header("SimRacingStudio")]
    [Draw("IP Address", Tooltip = "IP Address for SRS Host (default 127.0.0.1)")]
    public string IP_SRS = "127.0.0.1";
    [Draw("Port", Tooltip = "The port for SimRacingStudio (default 33001")]
    public int Port_SRS = 33001;

    [Space(10)]
    [Header("Advanced Settings")]
    [Draw("Show Advanced Settings", Tooltip = "You probably don't need to change these.")]
    public bool ShowAdvancedSettings;
    [Draw("Debug Logging", Tooltip = "Whether to log extra information. This is useful for debugging, but should otherwise be kept off.", VisibleOn = "ShowAdvancedSettings|true")]
    public bool DebugLogging;

    public bool ForceJSON = false;
    
    public void Draw(UnityModManager.ModEntry modEntry)
    {
        Settings self = this;
        UnityModManager.UI.DrawFields(ref self, modEntry, DrawFieldMask.OnlyDrawAttr, OnChange);
        if (ShowAdvancedSettings && GUILayout.Button("Enable Developer Commands"))
            Console.RegisterDevCommands();
    }

    public override void Save(UnityModManager.ModEntry modEntry)
    {
        Port_SRS = Mathf.Clamp(Port_SRS, 1024, 49151);

        if (!UnloadWatcher.isQuitting)
            OnSettingsUpdated?.Invoke(this);
        Save(this, modEntry);
    }

    public void OnChange()
    {
        // yup
    }

    public static Settings Load(UnityModManager.ModEntry modEntry)
    {
        Settings data = Settings.Load<Settings>(modEntry);

            MigrateSettings(ref data);
            
            data.SettingsVer = GetCurrentVersion();

            data.Save(modEntry);
 
        return data;
    }

    private static int GetCurrentVersion()
    {
        return 1;
    }

    // Function to handle migrations based on the current version
    private static void MigrateSettings(ref Settings data)
    { 
        switch (data.SettingsVer)
        {

            case 0:
                break;

            default:
                break;
        }

    }
}
