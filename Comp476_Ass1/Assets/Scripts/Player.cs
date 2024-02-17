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
    [SerializeField] private float bounceDivider = 2f;
    private bool isBouncing = false;
    private Vector3 bounce;



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
        if (isBouncing)
        {
            Vector3 timedBounce = bounce * Time.deltaTime;
            transform.Translate(timedBounce / bounceDivider);
            bounce -= timedBounce * bounceDivider;

            if (bounce.magnitude < 1f)
            {
                isBouncing = false;
            }
        }
        else if (doMovement && inputDir != Vector3.zero)
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
        // Catch heroes.
        if (other.gameObject.CompareTag("Hero"))
        {
            Hero potentialHero = other.gameObject.GetComponent<Hero>();

            if (potentialHero)
            {
                Prisoner potentialPrisoner = potentialHero.MyFriendPrisoner;
                bool wasRescuingPrisoner = potentialHero.MyState == Hero.HeroStates.ReachFortress;

                // If the hero is currently rescuing the prisoner, kill them forever.
                OnPlayerCaughtHero?.Invoke(other.gameObject, wasRescuingPrisoner);

                // Hijack their prisoner.
                if (potentialPrisoner && wasRescuingPrisoner)
                {
                    potentialPrisoner.InitFollowPlayer(gameObject);
                }
            }
        }

        // Bounce back against obstacles.
        if (other.gameObject.CompareTag("Obstacle"))
        {
            //Debug.Log(string.Format("The player hit an obstacle {0}.", other.gameObject));

            bounce = -currentVelocity;
            bounce.y = 0f;

            isBouncing = true;
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
