using UnityEngine;
using System;
using LocoSim.Implementations;
using LocoSim.Definitions;
using System.Collections.Generic;
using System.Collections;
using DV.Simulation.Cars;
using MotionSim.Patches.Player;

namespace MotionSim.Components;

[ExecuteInEditMode]
public class Telemetry : MonoBehaviour
{
    private char[] carID;

    private bool initialised;

    private TrainCar car;
    
    private Port maxRPM;
    private Port curRPM;
    private Vector3 lastPos = Vector3.zero;

    private Rigidbody vehicleBody;

    void Awake()
    {
        MotionSim.Log($"Telemetry.Awake()");

        carID = CustomFirstPersonControllerPatch.ON_FOOT;

        car = GetComponent<TrainCar>();
        car.LogicCarInitialized += OnInitialise;

        //if the logic car is already initialised, run the initialise routine
        if (car.logicCar != null)
        {
            MotionSim.Log($"Telemetry.Awake() LogicCar Initialised");
            OnInitialise();
        }
    }

    private void OnInitialise()
    {
        StartCoroutine(WaitForInit());
    }

    private IEnumerator WaitForInit()
    {
        SimulationFlow simFlow = null;

        carID = car.ID.PadRight(50).ToCharArray();

        MotionSim.Log($"Telemetry.Initialise({car?.ID}) Waiting for logicCar");
        while (car.logicCar == null)
            yield return null;

        MotionSim.Log($"Telemetry.Initialise({car?.ID}) Waiting for RigidBody");
        while(vehicleBody == null)
        {
            vehicleBody = GetComponent<Rigidbody>();
            yield return null;
        }

        if (car.IsLoco)
        {
            MotionSim.Log($"Telemetry.Initialise({car?.ID}) Waiting for SimulationFlow");
            while (simFlow == null)
            {
                simFlow = GetComponent<SimController>()?.simFlow;
                yield return null;
            }

            foreach (KeyValuePair<string, Port> kvp in simFlow.fullPortIdToPort)
            {
                MotionSim.LogError($"Telemetry.Initialise() {car.ID} found: {kvp.Key}, {kvp.Value.type}, {kvp.Value.valueType}");

                switch (kvp.Value.valueType)
                {
                    case (PortValueType.RPM):
                        if (kvp.Key.EndsWith("MAX_RPM"))
                            maxRPM = kvp.Value;
                        else if(kvp.Key.EndsWith(".RPM"))
                            curRPM = kvp.Value;

                        break;
                }
            }
        }

        initialised = true;
    }


    // Update is called once per frame
    void Update()
    {
        float rpm = 0f;
        float rpmMax = 0f;
        float speed = 0f;

        if (!initialised)
            return;

        MotionSim.Log($"Update({carID}) isLoco?");

        if (car.IsLoco)
        {
            MotionSim.Log($"Update({carID}) get RMP");
            rpm = curRPM.value;
            MotionSim.Log($"Update({carID}) get Max RMP");
            rpmMax = maxRPM.value;

            //other parameters for locos?
        }

        MotionSim.Log($"RigidBody {carID}: {vehicleBody.velocity.magnitude} m/s, {vehicleBody.velocity.magnitude * 3.6}");

        speed = Convert.ToSingle(vehicleBody.velocity.magnitude * 3.6);   //convert to km/hr (velocity * 3.6)

        SimRacingStudio.Instance.SimRacingStudio_UpdateTelemetry(
                                                                   carID
                                                                 , CustomFirstPersonControllerPatch.location
                                                                 , speed
                                                                 , rpm
                                                                 , rpmMax
                                                                 , -1           //Gear
                                                                 , vehicleBody.rotation.eulerAngles.x, vehicleBody.rotation.eulerAngles.y, vehicleBody.rotation.eulerAngles.z
                                                                 , 0
                                                                 , 0, 0, 0      //Acceleration
                                                                 , 0, 0, 0, 0   //Suspension travel
                                                                 , 0, 0, 0, 0   //Wheel terrain
                                                                );
    }

    void OnDestroy()
    {
        car.LogicCarInitialized -= OnInitialise;
    }
}
