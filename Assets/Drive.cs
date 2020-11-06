using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour
{
    public WheelCollider[] WCs;
    public float torque = 200;
    public GameObject[] Wheels;
    public float maxSteerAngle = 30;
    public float maxBrakeTorque = 500;

    public AudioSource skidSound;

    public Transform SkidTrailPrefab;
    Transform[] skidTrails = new Transform[4];

    public ParticleSystem smokePrefab;
    ParticleSystem[] skidSmoke = new ParticleSystem[4];

    public GameObject BrakeLight;

    public void StartSkidTrail(int i)
    {
        if (skidTrails[i] == null)
            skidTrails[i] = Instantiate(SkidTrailPrefab);

        skidTrails[i].parent = WCs[i].transform;
        skidTrails[i].localRotation = Quaternion.Euler(90, 0, 0);
        skidTrails[i].localPosition = -Vector3.up * WCs[i].radius;
    }

    public void EndSkidTrail(int i)
    {
        if (skidTrails[i] == null) return;
        Transform holder = skidTrails[i];
        skidTrails[i] = null;
        holder.parent = null;
        holder.rotation = Quaternion.Euler(90, 0, 0); 
        Destroy(holder.gameObject, 30);
    }

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 4; i++)
        {
            skidSmoke[i] = Instantiate(smokePrefab);
            skidSmoke[i].Stop();
        }
        BrakeLight.SetActive(false);
    }

    void Go(float accel, float steer, float brake)
    {
        accel = Mathf.Clamp(accel, -1, 1); // min = -1 , max = 1
        steer = Mathf.Clamp(steer, -1, 1) * maxSteerAngle;
        brake = Mathf.Clamp(brake, 0, 1) * maxBrakeTorque; 
        float thrustTorque = accel * torque;

        if(brake != 0)
            BrakeLight.SetActive(true);
        else
            BrakeLight.SetActive(false);

        for (int i = 0; i < 4; i++)
        {
            WCs[i].motorTorque = thrustTorque;

            if (i < 2)
                WCs[i].steerAngle = steer;
            else
                WCs[i].brakeTorque = brake; 

            Quaternion quat;
            Vector3 position;
            WCs[i].GetWorldPose(out position, out quat);
            Wheels[i].transform.position = position;
            Wheels[i].transform.rotation = quat;
        }
    }

    void checkForSkid()
    {
        int numSkidding = 0;
        for(int i = 0; i < 4; i++)
        {
            WheelHit wheelHit;
            WCs[i].GetGroundHit(out wheelHit);

            if(Mathf.Abs(wheelHit.forwardSlip) >= 0.4f || Mathf.Abs(wheelHit.sidewaysSlip) >= 0.4f)
            {
                numSkidding++;
                if (!skidSound.isPlaying)
                {
                    skidSound.Play();
                }
                StartSkidTrail(i);
                skidSmoke[i].transform.position = WCs[i].transform.position - WCs[i].transform.up * WCs[i].radius;
                skidSmoke[i].Emit(1);
            }
            else
            {
                EndSkidTrail(i);
            }
        }

        if(numSkidding == 0 && skidSound.isPlaying)
        {
            skidSound.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        float a = Input.GetAxis("Vertical");
        float s = Input.GetAxis("Horizontal");
        float b = Input.GetAxis("Jump");

        Go(a, s, b);

        checkForSkid();
    }
}
