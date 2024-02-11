using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // VARIABLES
    [SerializeField] private float accelerationRate = 0.1f;
    [SerializeField] private float maxSpeed = 16f;
    private float currentSpeed = 0f;
    private Vector3 currentVelocity;



    // METHODS

    // Input handling & movement.
    private Vector3 GetInput()
    {
        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");

        return new Vector3(inputHorizontal, 0, inputVertical).normalized;
    }
    private void Move(Vector3 inputDir)
    {
        if (inputDir != Vector3.zero)
        {
            // Move the character
            transform.Translate(inputDir);


            // Just face where you are going.
            transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation * Quaternion.LookRotation(inputDir), Time.deltaTime);


            
        }
    }


    // Built in.
    void Start()
    {
        
    }
    void Update()
    {
        Move(GetInput());
    }
}
