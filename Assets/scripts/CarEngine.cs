﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour {

    public Transform path;
    public List<Transform> nodes;
    private int currentNode = 0;
    public float maxSteerAngle = 45f;
    public float turnSpeed = 5;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public float maxMotorTorque = 500f;
    public float maxBrakeTorque = 150f;
    public float currentSpeed;
    public float maximumSpeed = 30;
    public Vector3 centerOfMass;
    private float targetSteerAngle = 0;
    public GameObject trafficLight;
    private bool canDrive = false;

	// Use this for initialization
	void Start () {

        GetComponent<Rigidbody>().centerOfMass = centerOfMass;
        nodes = new List<Transform>();

        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();

        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != path.transform)
            {
                nodes.Add(pathTransforms[i]);
            }
        }
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        CheckTrafficLight();
        ApplySteer();
        Drive();
        CheckWaypointDistance();
        LerpToSteerAngle();
        Brake();
        
	}

    private void CheckTrafficLight() {
        TrafficLight tl = trafficLight.GetComponent<TrafficLight>();

        // We measure how close we are to the traffic light
        Vector3 distanceVector = 
            transform.InverseTransformPoint(trafficLight.transform.position);

        // If we are close and the traffic light is red, then we cannot drive
        if (distanceVector.magnitude < 6f && tl.allow == false) {
            canDrive = false;      
        } else {
            canDrive = true;
        }
    }

    private void ApplySteer() {
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode].position);
        float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;

        targetSteerAngle = newSteer;
    }

    private void Drive() {
        currentSpeed = 2 * Mathf.PI * wheelFL.radius * wheelFL.rpm * 60 / 1000; // current speed

        if (currentSpeed < maximumSpeed) {
            print(maxMotorTorque);
            wheelFL.motorTorque = maxMotorTorque;
            wheelFR.motorTorque = maxMotorTorque;    
        } else {
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
        }

    }

    private void Brake() {
        if (!canDrive) {
            print("Breaaaaak!!!");
            wheelFL.brakeTorque = maxBrakeTorque;
            wheelFR.brakeTorque = maxBrakeTorque;
        } else {
            wheelFL.brakeTorque = 0;
            wheelFR.brakeTorque = 0;
        }
    }

    private void CheckWaypointDistance() {
        if(Vector3.Distance(transform.position, nodes[currentNode].position) < 3) {
            if (currentNode != nodes.Count - 1) {
                currentNode++;
            }
        }
    }

    private void LerpToSteerAngle() {
        wheelFL.steerAngle = Mathf.Lerp(wheelFL.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
        wheelFR.steerAngle = Mathf.Lerp(wheelFR.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
    }
}
