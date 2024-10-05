using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    private float angularSpeed = 10.0f;
    private float radius = 0.3f;
    private Vector3 centre = new Vector3(0.0f,0.0f,0.0f);
    private float angle = 0.0f;
    
    void FixedUpdate()
    {
        angle += Time.deltaTime * angularSpeed;
        Vector3 direction = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up;
        transform.position = centre + direction * radius;
    }
}
