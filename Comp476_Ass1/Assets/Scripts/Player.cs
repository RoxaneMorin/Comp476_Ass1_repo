using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // VARIABLES
    [Header("References")]
    [SerializeField] private GameObject myHat;
    [SerializeField] private GameObject myRespawnPoint;
    [SerializeField] private GameObject myDeathParticleSystem;

    [Header("Movement")]
    private bool doMovement = true;
    public bool DoMovement { get; set; }
    [SerializeField] private float accelerationRate = 0.1f;
    [SerializeField] private float maxSpeed = 16f;
    private float currentSpeed = 0f;
    private Vector3 currentVelocity;



    // EVENTS
    public delegate void EventPlayerCaughtHero(GameObject heroGO, bool killForever);
    public static event EventPlayerCaughtHero OnPlayerCaughtHero;



    // METHODS

    // Game stuff.
    private void PrepareRespawn(GameObject hitObject)
    {
        if (hitObject = this.gameObject)
        {
            doMovement = false;
            Instantiate(myDeathParticleSystem, transform.position, Quaternion.identity);

            Invoke("Respawn", 0.5f);
        }
    }
    private void Respawn()
    {
        gameObject.transform.position = myRespawnPoint.transform.position;
        doMovement = true;
    }


    // Input handling & movement.
    private Vector3 GetInput()
    {
        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");

        return new Vector3(inputHorizontal, 0, inputVertical).normalized;
    }
    private void Move(Vector3 inputDir)
    {
        if (doMovement && inputDir != Vector3.zero)
        {
            // Move the character
            currentSpeed += accelerationRate * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

            currentVelocity = new Vector3(inputDir.x * currentSpeed, 0f, inputDir.z * currentSpeed);

            transform.Translate(currentVelocity * Time.deltaTime);

            // Rotate the character's hat.
            myHat.transform.LookAt(transform.position + currentVelocity);
        }
    }


    // Built in.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Hero"))
        {
            Hero potentialHero = other.gameObject.GetComponent<Hero>();

            if (potentialHero)
            {
                // If the hero is currently rescuing the prisoner, kill them forever.
                OnPlayerCaughtHero?.Invoke(other.gameObject, potentialHero.MyState == Hero.HeroStates.ReachFortress);

                // Hijack their prisoner.
                potentialHero.MyFriendPrisoner.InitFollowPlayer(gameObject);
            }
        }
    }
    void Start()
    {
        HeroKillZone.OnPlayerKillZoneEnter += PrepareRespawn;
        GuardianKillZone.OnPlayerKillZoneEnter += PrepareRespawn;
    }
    void Update()
    {
        Move(GetInput());
    }
}
