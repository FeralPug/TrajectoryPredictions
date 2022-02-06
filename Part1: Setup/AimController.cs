using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimController : MonoBehaviour
{
    public Vector2 rotSpeed;

    // Update is called once per frame
    void Update()
    {
        Rotate();
    }

    void Rotate()
    {
        Vector3 currentRotation = transform.rotation.eulerAngles;

        Vector3 desRot = new Vector3();
        desRot.x = Input.GetAxis("Vertical") * rotSpeed.x;
        desRot.y = Input.GetAxis("Horizontal") * rotSpeed.y;

        desRot *= Time.deltaTime;

        currentRotation += desRot;

        transform.rotation = Quaternion.Euler(currentRotation);
    }
}
