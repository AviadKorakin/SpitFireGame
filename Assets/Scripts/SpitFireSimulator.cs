using System;
using System.Collections;
using Unity.Properties;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpitFireSimulator : MonoBehaviour
{
    [Header("SpitFire Settings")]
    [SerializeField] private float maxSpeed = 50f;
    [SerializeField] private float minSpeed = 0f;
    [SerializeField] private float accelerationRate = 0.1f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float flightSpeed = 30f;
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private float crashDelay = 0.5f;
    [SerializeField] private float throttleLevel =0f;
    [SerializeField] private int throttleClickCount = 0;
    [SerializeField] private float[] throttleSpeeds = { 0f, 10f, 20f, 30f, 40f, 50f };
    [SerializeField] private int clicksPerThrottleLevel = 10;
    [SerializeField] private ParticleSystem fireEffect;
    private float lastThrottleDirection = 0;
    private bool isFlying = false;
    private bool inScope = false;
    private readonly Vector3 resetPosition = new Vector3(0, 2.41f, -50);
    private float previousThrottleLevel;
    private float previousSpeed;
    private float maxAllowedSpeed;


    [Header("UI Settings")]
    [SerializeField] private int score = 0;
    [SerializeField] private Text speedText;
    [SerializeField] private Text throttleLevelText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text warningText;
    [SerializeField] private Text ammoText;          
    [SerializeField] private Text missileReloadingText; 
    [SerializeField] private GameObject crosshair;


    [Header("Missile Settings")]
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private GameObject missilePoint;
    [SerializeField] private float missileSpeed=600f;
    [SerializeField] private int ammoCount = 30;                   
    [SerializeField] private float missileCooldown = 3f;           
    [SerializeField] private float lastShotTime = -3f;

    [Header("Ballons Settings")]
    [SerializeField] private GameObject[] ballons;

    [Header("Map Settings")]
    [SerializeField] private Terrain innerTerrain;
    [SerializeField] private Terrain outerTerrain;
    private float outOfBoundsTimer = 0f;
    private bool isOutOfBounds = false;
    private float elapsedTime = 0f;

    [Header("SpitFire Parts")]
    private Transform propeller;
    private Transform aileronLeft;
    private Transform aileronRight;
    private Transform flaps1;
    private Transform flaps2;
    private Transform ruli;
    private Transform rudder;
    private Animator animator;

    [Header("Initial Part Rotations")]
    private Quaternion initialAileronLeftRotation;
    private Quaternion initialAileronRightRotation;
    private Quaternion initialFlap1Rotation;
    private Quaternion initialFlap2Rotation;
    private Quaternion initialRudderRotation;
    private Quaternion initialRuliRotation;

    [Header("Initial Part Rotations")]
    [SerializeField] private AudioSource engineLandSound;
    [SerializeField] private AudioSource engineFlySound;
    [SerializeField] private AudioSource crashSound;     
    [SerializeField] private AudioSource shootSound;

    [SerializeField] private AudioClip engineLandClip;
    [SerializeField] private AudioClip engineFlyClip;
    [SerializeField] private AudioClip crashClip;       
    [SerializeField] private AudioClip shootClip;


    //###############Override methods#########################
    void Start()
    {
        InitializeComponents();
        PlaceBallons();
        InitializeSounds();
        StoreInitialRotations();
        InitializeUIText();
        PlayEngineLandSound();
    }


    void FixedUpdate()
    {
        UpdateSpeed();
        MovePlane();
        ApplyGravityIfNeeded();
    }

    private void Update()
    {
        UpdateElapsedTime();
        HandleFlyingState();
        UpdateSpeedDisplay();
        UpdateReloadingDisplay();
        HandleUserInputs();
        UpdateAnimations();
        HandleOutOfBounds();
    }


    //###################################################################

    private void PlaceBallons()
    {
        Vector3 terrainPosition = innerTerrain.transform.position;
        Vector3 terrainSize = innerTerrain.terrainData.size;

        for (int i = 0; i < 10; i++) // Place exactly 10 balloons
        {
            bool isPlaced = false;
            int maxAttempts = 20; // Limit attempts to find a valid position

            while (!isPlaced && maxAttempts > 0)
            {
                // Random x and z within terrain bounds, leaving a 50-unit margin
                float randomX = UnityEngine.Random.Range(terrainPosition.x + 50, terrainPosition.x + terrainSize.x - 50);
                float randomZ = UnityEngine.Random.Range(terrainPosition.z + 50, terrainPosition.z + terrainSize.z - 50);
                

                // Get terrain height at the random position
                float terrainHeight = innerTerrain.SampleHeight(new Vector3(randomX, 0, randomZ));
                float randomY = terrainHeight + UnityEngine.Random.Range(50, 400);

                // Set balloon height to be at least 50 units above terrain height
                Vector3 potentialPosition = new Vector3(randomX, randomY, randomZ);

                // Define the box size around the potential position
                Vector3 boxSize = new Vector3(16f, 16f,16f); // Adjust this size based on balloon dimensions

                // Check for overlapping colliders within the box area
                // Perform an overlap check using an imaginary box at 'potentialPosition' to detect nearby colliders.
                // 'boxSize / 2' specifies the half-extents of the box (half the total size along each axis), centered on 'potentialPosition'.
                // 'Quaternion.identity' sets the box rotation to align with world axes (no rotation).
                // '~0' is a layer mask with all bits set to 1, meaning the check will consider colliders in all layers.
                // 'QueryTriggerInteraction.Collide' includes both regular and trigger colliders in the overlap check.
                // The result is stored in 'hasOverlap', which will be 'true' if any collider overlaps the box, and 'false' otherwise.
                bool hasOverlap = Physics.CheckBox(potentialPosition, boxSize / 2, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);

                if (!hasOverlap) // Only place if no overlap is detected
                {
                    // Randomly select a balloon model from the array
                    GameObject balloonPrefab = ballons[UnityEngine.Random.Range(0, ballons.Length)];

                    // Instantiate the balloon at the calculated position
                    Instantiate(balloonPrefab, potentialPosition, Quaternion.identity);

                    isPlaced = true; // Balloon successfully placed
                }

                maxAttempts--; // Reduce attempts if unsuccessful
            }

            // Log warning if unable to place a balloon
            if (!isPlaced)
            {
                Debug.LogWarning("Could not place a balloon within the given constraints.");
                EndGame("ERROR", "Cannot place the ballon in the playing field");
            }
        }
    }
    public int getAmmo()
    {
        return ammoCount;
    }
    public void OutOfAmmo()
    {
        EndGame("DEFEAT", "You've Exhausted Your Ammunition");
    }
    private void HandleUserInputs()
    {
        ShootMissile();
        HandleScopeView();
        HandleThrottle();
        HandleRotationInput();
    }

    private void UpdateAnimations()
    {
        AnimatePropeller();
        AnimateControlSurfaces();
    }
    private void HandleThrottle()
    {
        float throttleInput = Input.GetAxis("Throttle");
        if (throttleInput != 0 && Mathf.Sign(throttleInput) == Mathf.Sign(lastThrottleDirection))
        {
            throttleClickCount++;
            if (throttleClickCount >= 10)
            {
                throttleLevel = Mathf.Clamp(throttleLevel + Mathf.Sign(throttleInput), 0, throttleSpeeds.Length - 1);
                throttleClickCount = 0;
                maxAllowedSpeed = throttleSpeeds[(int)throttleLevel]; // Cache maxAllowedSpeed
            }
        }
        else if (throttleInput != 0)
        {
            throttleClickCount = 1;
            lastThrottleDirection = throttleInput;
        }
        UpdateThrottleDisplay();
    }
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        propeller = FindTransform("propeller");
        aileronLeft = FindTransform("aileron_left");
        aileronRight = FindTransform("aileron_right");
        flaps1 = FindTransform("flaps1");
        flaps2 = FindTransform("flaps2");
        ruli = FindTransform("ruli");
        rudder = FindTransform("rudder");
    }
    private void InitializeSounds()
    {
        engineLandSound.clip = engineLandClip;
        engineFlySound.clip = engineFlyClip;
        shootSound.clip=shootClip;
    }
    public void PlayEngineLandSound()
    {
        engineFlySound.Stop();
        engineLandSound.Play();  // Play crash sound once
    }
    public void PlayEngineFlySound()
    {
        engineLandSound.Stop();
        engineFlySound.Play();  // Play crash sound once
    }
    public void PlayCrashSound()
    {
        crashSound.PlayOneShot(crashClip);  
    }

    public void PlayShootSound()
    {
        shootSound.Play();  
    }
    public void StopShootSound()
    {
        shootSound.Stop();
    }

    private Transform FindTransform(string name)
    {
        var transformFound = transform.Find(name);
        if (!transformFound) Debug.LogError($"{name} not found!");
        return transformFound;
    }

    private void ShootMissile()
    {

        // Check if the fire button is pressed, ammo is available, and cooldown is over
        if (Input.GetAxis("Fire1") > 0 && ammoCount > 0 && Time.time >= lastShotTime + missileCooldown)
        {

            PlayShootSound();
            // Instantiate missile and set its force
            GameObject missile = Instantiate(missilePrefab, missilePoint.transform.position, transform.rotation);
            missile.GetComponent<Rigidbody>().AddForce(transform.forward * missileSpeed);
            missile.GetComponent<Missile>().flightSimulator = this;  // Assign reference to main script

            // Update ammo count and last shot time
            ammoCount--;
            lastShotTime = Time.time;

            // Update ammo display
            UpdateAmmoDisplay();

        }
    }
    private void UpdateAmmoDisplay()
    {
      
      
        ammoText.text = $"AMMO: {ammoCount}";
        if (ammoCount > 0)
        {
            missileReloadingText.text = "Reloading...";
        }
        else
        {
            missileReloadingText.text = "Out of Ammo";
        }
        
    }
    private void UpdateReloadingDisplay()
    {
        if (Time.time >= lastShotTime + missileCooldown)
        {
            missileReloadingText.text = "";
        }
    }
    private void StoreInitialRotations()
    {
        if (aileronLeft) initialAileronLeftRotation = aileronLeft.localRotation;
        if (aileronRight) initialAileronRightRotation = aileronRight.localRotation;
        if (flaps1) initialFlap1Rotation = flaps1.localRotation;
        if (flaps2) initialFlap2Rotation = flaps2.localRotation;
        if (rudder) initialRudderRotation = rudder.localRotation;
        if (ruli) initialRuliRotation = ruli.localRotation;
    }

    private void InitializeUIText()
    {
        SetText(speedText, "SPEED: 0");
        SetText(scoreText, "SCORE: 0");
        SetText(timeText, "TIME: 0");
        SetText(warningText, "");
        SetText(ammoText, "AMMO: 30"); // Initialize ammo text
        SetText(missileReloadingText, ""); // Clear reloading text at start
        SetText(throttleLevelText, "THROTTLE LEVEL: 0");
    }

    private void SetText(Text textComponent, string text)
    {
        if (textComponent) textComponent.text = text;
    }
    private void UpdateSpeed()
    {

        // If current speed is above the max speed for the throttle level, decrease speed gradually
        if (currentSpeed > maxAllowedSpeed)
        {
            currentSpeed = Mathf.Max(currentSpeed - accelerationRate , maxAllowedSpeed);
        }
        else
        {
            // Otherwise, adjust speed based on throttle level and acceleration rate
            currentSpeed = Mathf.Clamp(
                throttleLevel > 0 ? currentSpeed + throttleLevel * accelerationRate  : currentSpeed - accelerationRate ,
                minSpeed, maxSpeed
            );
        }
    }

    private void MovePlane()
    {
        Vector3 forwardDirection = (currentSpeed < flightSpeed && transform.position.y < 2.5f)
             ? new Vector3(transform.forward.x, 0, transform.forward.z) : transform.forward;
        transform.position += forwardDirection * currentSpeed * Time.deltaTime;
    }
    private void HandleFlyingState()
    {
        if (transform.position.y > 20 && !isFlying)
        {
            isFlying = true;
            PlayEngineFlySound();
            animator.SetTrigger("Flying");
        }
        else if (transform.position.y <= 20 && isFlying)
        {
            isFlying = false;
            PlayEngineLandSound();
            animator.SetTrigger("Landing");
        }
    }

    private void ApplyGravityIfNeeded()
    {
        if (transform.position.y > 2.5f)
        {
            transform.position += Vector3.down * gravity * Time.deltaTime * ((transform.position.y + 800f) / 800f);
            //simulating gravity per height
        }
    }

    private void UpdateElapsedTime()
    {
        elapsedTime += Time.deltaTime;
        SetText(timeText, $"TIME: {Mathf.FloorToInt(elapsedTime)} SECONDS");
    }

    private void HandleScopeView()
    {
        if (Input.GetMouseButtonDown(1) && !inScope)
        {
            animator.SetTrigger("InScope");
            inScope = true;
            crosshair?.SetActive(true);
        }
        if (Input.GetMouseButtonUp(1) && inScope)
        {
            animator.SetTrigger("OutScope");
            inScope = false;
            crosshair?.SetActive(false);
        }
    }

    private void HandleOutOfBounds()
    {
        if (IsOutOfInnerTerrain())
        {
            if (!isOutOfBounds) // Start countdown only if it's the first time out of bounds
            {
                isOutOfBounds = true;
                outOfBoundsTimer = 10f; // Set the countdown to 10 seconds
            }

            // Decrease timer and update warning text
            outOfBoundsTimer -= Time.deltaTime;
            SetText(warningText, $"Warning: Return to SAFE ZONE - {Mathf.Ceil(outOfBoundsTimer)} seconds left");
            warningText.color = Mathf.FloorToInt(outOfBoundsTimer) % 2 == 0 ? Color.red : Color.yellow;

            // Reset plane if the timer runs out
            if (outOfBoundsTimer <= 0)
            {
                ResetPlane();
                isOutOfBounds = false;
                SetText(warningText, ""); // Clear warning when back in bounds
            }
        }
        else
        {
            if (isOutOfBounds == true)
            {
                // Reset timer and flag if back within inner terrain
                isOutOfBounds = false;
                outOfBoundsTimer = 0;
                SetText(warningText, "");
            }
        }
    }

    private bool IsOutOfInnerTerrain()
    {
        Vector3 terrainPosition = innerTerrain.transform.position;
        Vector3 terrainSize = innerTerrain.terrainData.size;

        return transform.position.x < terrainPosition.x || transform.position.x > terrainPosition.x + terrainSize.x ||
               transform.position.z < terrainPosition.z || transform.position.z > terrainPosition.z + terrainSize.z ||
               transform.position.y > 4000;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Runway"))
        {

            EndGame("DEFEAT", "Your aircraft collided with " + collision.gameObject.tag);
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Runway"))
        {
            PlayCrashSound();
            StartCoroutine(HandleCrash());
        }
    }

        private IEnumerator HandleCrash()
    {
        fireEffect.Play();
        // Trigger the crash animation
        animator.SetTrigger("Crash");

        // Wait for the crash animation to play
        yield return new WaitForSeconds(crashDelay);

        EndGame("DEFEAT", "Your aircraft was destroyed upon impact with a balloon.");
    }
    private void ResetPlane()
    {
        transform.position = resetPosition;
        transform.rotation = Quaternion.Euler(-13.573f, 0f, 0f);
        currentSpeed = 0f;
        Debug.Log("Plane reset due to being out of bounds.");
    }

    private void UpdateSpeedDisplay()
    {
        if (Mathf.Abs(currentSpeed - previousSpeed) > 0.1f) 
        {
            SetText(speedText, $"SPEED: {Mathf.FloorToInt(currentSpeed * 14.2f)} KM/H");
            previousSpeed = currentSpeed;
        }
    }
    private void UpdateThrottleDisplay()
    {
        if (throttleLevel != previousThrottleLevel)
        {
            SetText(throttleLevelText, $"THROTTLE LEVEL: {throttleLevel}");
            previousThrottleLevel = throttleLevel;
        }
    }
    public void IncreaseScore(int amount)
    {
        score += amount;
        SetText(scoreText, $"SCORE: {score}");

        if (score >= 10)
        {
            EndGame("VICTORY", "Congratulations! You've cleared all balloons.");
        }
        else
        {
            if(ammoCount==0)
                    OutOfAmmo();
        }
    }
    private void EndGame(string method, string reason)
    {

        Debug.Log("Game Over!");
        GameData.Instance.SetGameOutcome(method, score, elapsedTime, reason);
        // Load the Game Over scene
        SceneManager.UnloadSceneAsync("SpitFireSimulator");
        SceneManager.LoadScene("GameOver");
    }

    private void HandleRotationInput()
    {
        float yaw = Input.GetAxis("Yaw") * currentSpeed * Time.deltaTime;
        float pitch = Input.GetAxis("Pitch") * currentSpeed * Time.deltaTime;
        float roll = Input.GetAxis("Roll") * currentSpeed * Time.deltaTime;

        if (currentSpeed < flightSpeed && transform.position.y < 2.5f)
            transform.Rotate(0, yaw, 0);
        else
            transform.Rotate(pitch, yaw, roll);
    }

    private void AnimatePropeller()
    {
            propeller.Rotate(Vector3.up, currentSpeed * 25f * Time.deltaTime, Space.Self);
    }

    private void AnimateControlSurfaces()
    {
        AnimateAilerons(Input.GetAxis("Roll"));
        AnimateFlaps(Input.GetAxis("Pitch"));
        AnimateRudder(Input.GetAxis("Yaw"));
        AnimateRuli(Input.GetAxis("Pitch"));
    }

    private void AnimateAilerons(float roll)
    {
        float aileronAngle = roll * 20f;
        if (aileronLeft) aileronLeft.localRotation = initialAileronLeftRotation * Quaternion.Euler(aileronAngle, 0, 0);
        if (aileronRight) aileronRight.localRotation = initialAileronRightRotation * Quaternion.Euler(-aileronAngle, 0, 0);
    }

    private void AnimateFlaps(float pitch)
    {
        float flapAngle = Mathf.Clamp(pitch * 20f, -35f, 35f);
        if (flaps1) flaps1.localRotation = initialFlap1Rotation * Quaternion.Euler(flapAngle, 0, 0);
        if (flaps2) flaps2.localRotation = initialFlap2Rotation * Quaternion.Euler(flapAngle, 0, 0);
    }

    private void AnimateRudder(float yaw)
    {
        float rudderAngle = Mathf.Clamp(yaw * 30f, -30f, 30f);
        if (rudder) rudder.localRotation = initialRudderRotation * Quaternion.Euler(0, 0, rudderAngle);
    }

    private void AnimateRuli(float pitch)
    {
        float ruliAngle = Mathf.Clamp(pitch * 15f, -25f, 25f);
        if (ruli) ruli.localRotation = initialRuliRotation * Quaternion.Euler(ruliAngle, 0, 0);
    }
}
