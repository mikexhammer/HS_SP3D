using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    // Fallguy Object
    public Transform target;
    // Offset from the target, 1 units up and 5 units back
    public Vector3 offset = new Vector3(0f, 1f, -5f);
    [Range(50, 500)] public int rotationSpeed = 100;
    
    // Zoom Variablen
    [Range(3, 8)] public int minDistance = 5;
    [Range(10, 20)] public int maxDistance = 10;
    [Range(1, 5)] public int zoomSpeed = 1;
    private float currentDistance;

    private void Start()
    {
        // set the camera position to the target position + offset
        transform.position = target.position + offset;
        // rotate the camera to look at the target
        transform.LookAt(target);
        
        currentDistance = offset.magnitude; //Magnitut = Betrag = Länges
    }

    private void LateUpdate()
    {
        transform.position = target.position + offset;
        
        // zoom in or out using mouse wheel input
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= zoomInput * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance); //Begrenzt Wert currentDistance auf minDistance und maxDistance

        // calculate new camera position based on current distance and offset
        Vector3 newPositionTwo = target.position + offset.normalized * currentDistance;

        // update camera position
        transform.position = newPositionTwo;

        // look at object
        transform.LookAt(target);
        
        
        if (Input.GetMouseButton(0))
        {
            // calculate the rotation amount based on mouse X input
            float rotationAmount = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            // calculate the new position of the camera after rotation
            Vector3 newPosition = Quaternion.Euler(0f, rotationAmount, 0f) * offset;
            // set the new offset
            offset = newPosition;
            // apply rotation around the target position
            transform.RotateAround(target.position, Vector3.up, rotationAmount);
        }
        
        

    }

}