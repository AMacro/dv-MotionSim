using HarmonyLib;
using UnityEngine;
using MotionSim.Components;
using MotionSim.Utils;
using MotionSim.Patches.Player;

namespace MotionSim.Patches.Trains;

[HarmonyPatch(typeof(TrainCar))]
public static class TrainCarPatch
{
    [HarmonyPatch(nameof(TrainCar.Awake))]
    [HarmonyPostfix]
    private static void Awake_Postfix(TrainCar __instance)
    {
        InitTelem(__instance);
    }

    [HarmonyPatch(nameof(TrainCar.AwakeForPooledCar))]
    [HarmonyPostfix]
    private static void AwakeForPooledCar_Postfix(TrainCar __instance)
    {
        InitTelem(__instance);
    }

    private static void InitTelem(TrainCar __instance)
    {
        if (CarSpawner.Instance.PoolSetupInProgress)
            return;

        Telemetry telem = __instance.GetOrAddComponent<Telemetry>();
        MotionSim.Telemetries.Add(__instance, telem);

        if (CustomFirstPersonControllerPatch.currentCar != __instance || PlayerManager.Car != __instance)
            telem.enabled = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TrainCar.PrepareForDestroy))]
    private static void PrepareForDestroy_Prefix(TrainCar __instance)
    {
        MotionSim.Log($"TrainCar.PrepareForDestroy({__instance?.ID})");

        Telemetry telem = __instance.GetComponent<Telemetry>();

        if( telem != null )
            MotionSim.Telemetries.Remove(__instance);
    }

}
