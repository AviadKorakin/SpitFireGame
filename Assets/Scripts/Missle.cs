using System.Collections;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public float speed = 600f;  // Speed of the missile
    public SpitFireSimulator flightSimulator;  // Reference to the main script to update score
    [SerializeField] private AudioSource hitSound;
    [SerializeField] private AudioClip hitClip;
    private Transform smoke;
    private Transform fire;
    private Transform backfire;
    private Transform missile;
    private bool isHit = false;
    private bool isLastMissle = false;
    private void Start()
    {
        if (flightSimulator.getAmmo() == 0) isLastMissle = true;
        // Ensure the missile destroys itself after some time to prevent memory issues
        Invoke(nameof(DestroyMissile), 7f);  
        // Disable child effects like smoke, fire, and backfire
        smoke = transform.Find("smoke");
        fire = transform.Find("fire");
        backfire = transform.Find("backfire");
        missile = transform.Find("missile");
    }

    public void PlayHitSound()
    {
        hitSound.PlayOneShot(hitClip);  // Play the sound without interrupting it
    }
    private void DestroyMissile()
    {
        Debug.Log("Missle TimeOut!");
        Destroy(gameObject);
        if (isLastMissle)
        {
            flightSimulator.OutOfAmmo();
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Ballon!");
        // Check if the missile hit a balloon
        if (other.gameObject.CompareTag("Balloon"))
        {
            flightSimulator.StopShootSound();
            isHit = true;
            PlayHitSound();

            // Disable the missile's visuals and collider
            DisableMissile();

            // Disable the balloon
            other.gameObject.SetActive(false);


            // Update the score in the main script
            if (flightSimulator != null)
            {
                flightSimulator.IncreaseScore(1);
            }

            // Start coroutine to wait for sound to finish, then destroy the missile
            StartCoroutine(DestroyAfterSound());
        }
    }

    private void DisableMissile()
    {
  

        if (smoke != null) smoke.gameObject.SetActive(false);
        if (fire != null) fire.gameObject.SetActive(false);
        if (backfire != null) backfire.gameObject.SetActive(false);
        if (missile != null) missile.gameObject.SetActive(false);
    }

    private IEnumerator DestroyAfterSound()
    {
        // Wait for the length of the sound clip to finish playing
        yield return new WaitForSeconds(4f);

        // Destroy the missile after the sound finishes
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Balloon") && isHit==false)
        {
            Debug.Log("Collision!");
            flightSimulator.StopShootSound();
            if (isLastMissle)
            {
                flightSimulator.OutOfAmmo();
            }
            // Destroy the missile on any collision with the terrain or other objects
            Destroy(gameObject);
        }
    }
}
