using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    // VARIABLES
    [SerializeField] private float speed = 1f;
    [SerializeField] private float maxMoveDistance = 5f;
    private Vector3 originalPosition;
    private bool moveLeft = false;


    // METHODS

    // Movement.
    private void Move()
    {
        float distanceFromStarterPosition = Vector3.Magnitude(transform.position - originalPosition);
        if (distanceFromStarterPosition > maxMoveDistance)
        {

            moveLeft = !moveLeft;
        }

        Vector3 velocity = (moveLeft ? -transform.forward : transform.forward) * speed;
        transform.position += velocity * Time.deltaTime;
    }


    // Built in.
    void Start()
    {
        originalPosition = gameObject.transform.position;
    }
    void Update()
    {
        Move();
    }
}
