using UnityEngine;
using UnityEngine.InputSystem;

public class FishermanController : MonoBehaviour
{
    [Header("Aiming")]
    [SerializeField] private Transform aimPivot;
    [SerializeField, Range(0f, 90f)] private float aimHalfArc = 70f;
    [Header("Bullets")]
    [SerializeField] private Transform gun;
    [SerializeField] private Rigidbody2D bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float fireCooldown = 0.4f;
    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSfx;
    private float nextFireTime = 0f;
    private Vector2 aimInput;

    private void OnAim(InputValue value)
    {
        aimInput = value.Get<Vector2>();
        Debug.Log(aimInput);
    }

    private void OnFire(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + fireCooldown;

        Rigidbody2D bullet = Instantiate(bulletPrefab, gun.position, gun.rotation);
        bullet.linearVelocity = (Vector2)gun.right * bulletSpeed;

        Destroy(bullet.gameObject, 3f);

        PlayFireSfx();
    }

    private void PlayFireSfx()
    {
        if (audioSource != null && fireSfx != null)
        {
            audioSource.PlayOneShot(fireSfx);
        }
    }

    private void Update()
    {
        if (aimInput.sqrMagnitude < 0.01f)
        {
            return;
        }

        float angleFromDown = Mathf.Atan2(aimInput.x, -aimInput.y) * Mathf.Rad2Deg;
        angleFromDown = Mathf.Clamp(angleFromDown, -aimHalfArc, aimHalfArc);

        float angle = -90f + angleFromDown;
        aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}