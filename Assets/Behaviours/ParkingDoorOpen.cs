using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingDoorOpen : MonoBehaviour
{
    float beginTime;

    // Start is called before the first frame update
    void Start()
    {
        beginTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - beginTime >= 2)
        {
            transform.position += Vector3.up * 0.4f * Time.deltaTime;
        }
    }
}
