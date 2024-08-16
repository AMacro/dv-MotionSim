using HarmonyLib;
using UnityEngine;
using MotionSim.Components;

namespace MotionSim.Patches.Player;

[HarmonyPatch(typeof(CustomFirstPersonController))]
public static class CustomFirstPersonControllerPatch
{
    public static readonly char[] ON_FOOT = "Foot Falcon".PadRight(50).ToCharArray();
    public static readonly char[] DEFAULT_LOCATION = "Derail Valley".PadRight(50).ToCharArray();

    public static CustomFirstPersonController fps;
    public static TrainCar currentCar;
    public static char[] location;
    private static Vector3 lastPos;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CustomFirstPersonController.Awake))]
    private static void Awake(CustomFirstPersonController __instance)
    {
        MotionSim.Log("CustomFirstPersonController.Awake()");

        fps = __instance;
        currentCar = PlayerManager.Car;
        location = DEFAULT_LOCATION;

        PlayerManager.CarChanged += OnCarChanged;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CustomFirstPersonController.OnDestroy))]
    private static void OnDestroy()
    {
        MotionSim.Log("CustomFirstPersonController.OnDestroy()");

        if (UnloadWatcher.isQuitting)
            return;

        PlayerManager.CarChanged -= OnCarChanged;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CustomFirstPersonController.Update))]
    private static void Update()
    {
        if (UnloadWatcher.isQuitting)
            return;

        if ((fps.transform.position - lastPos).sqrMagnitude > 100) //player moved more than 10 metres
        {
            lastPos = fps.transform.position;
            UpdateLocation();
        }

        if (PlayerManager.Car == null) //check we haven't jumped to another car
        {
            SimRacingStudio.Instance.SimRacingStudio_UpdateTelemetry(ON_FOOT
                                                                    , CustomFirstPersonControllerPatch.location
                                                                    , 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                                                                    );
        }
    }

    private static void OnCarChanged(TrainCar trainCar)
    {
        Telemetry telem;

        MotionSim.Log($"OnCarChanged({trainCar?.ID}) currentCar: {currentCar?.ID}");

        //Find the current car's telemetry and disable it
        if (currentCar != null && MotionSim.Telemetries.TryGetValue(currentCar, out telem))
        {
            telem.enabled = false;
        }

        currentCar = trainCar;

        //Find the new car's telemetry and enable it
        if(MotionSim.Telemetries.TryGetValue(trainCar, out telem))
        {
            telem.enabled = true;
        }
        else
        {
            MotionSim.LogWarning($"Telemetry not found for loco {trainCar.ID}");
        }

    }

    private static void UpdateLocation()
    {
        float closestDist = float.MaxValue;
        StationController closestStation = null;

        foreach (StationController station in StationController.allStations)
        {
            float sqrDistToStation = (fps.transform.position - station.stationRange.stationCenterAnchor.position).sqrMagnitude;

            if (sqrDistToStation < closestDist)
            {
                closestDist = sqrDistToStation;
                closestStation = station;
            }
        }

        //Are we actually close to this station?
        if (closestStation != null && closestDist <= closestStation.stationRange.generateJobsSqrDistance)
        {
            location = closestStation.stationInfo.Name.PadRight(50).ToCharArray();
        }
        else
        {
            location = DEFAULT_LOCATION;
        }
    }

}
