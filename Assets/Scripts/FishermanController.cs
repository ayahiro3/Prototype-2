using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class FishermanController : MonoBehaviour
{
    [Header("Aiming")]
    [SerializeField] private Transform aimPivot;
    [SerializeField, Range(0f, 90f)] private float aimHalfArc = 70f;

    [Header("Bullets")]
    [SerializeField] private Transform gun;
    [SerializeField] private Rigidbody2D bulletPrefab;
    [SerializeField, Min(0f)] private float bulletSpeed = 10f;
    [SerializeField, Min(0f)] private float chargedBulletSpeed = 20f;
    [SerializeField, Min(0f)] private float fireCooldown = 0.4f;

    [Header("Steadying and Charging")]
    [SerializeField, Min(0.01f)] private float steadyTime = 0.6f;
    [SerializeField, Min(0.01f)] private float chargeTime = 1.2f;
    [SerializeField, Range(0f, 90f)] private float startingSpread = 20f;

    [Header("Aim Lines")]
    [SerializeField] private LineRenderer aimLineLeft;
    [SerializeField] private LineRenderer aimLineRight;
    [SerializeField, Min(0.1f)] private float lineLength = 8f;
    [SerializeField, Min(0.001f)] private float lineWidth = 0.04f;
    [SerializeField] private Color unsteadyColor = Color.white;
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color chargedColor = Color.yellow;
    [SerializeField] private LayerMask wallLayers;

    [Header("Audio")]
    [SerializeField] private AudioSource aimAudio;
    [SerializeField] private AudioClip steadySound;
    [SerializeField] private AudioClip fullChargeSound;
    [SerializeField] private AudioClip fireSound;
    private bool playedSteadySound;
    private bool playedFullChargeSound;
    private Vector2 aimInput;
    private bool isHolding;
    private float holdStartTime;
    private float nextFireTime;

    private void Awake()
    {
        ConfigureLine(aimLineLeft);
        ConfigureLine(aimLineRight);
        HideAimLines();
    }

    private void ConfigureLine(LineRenderer line)
    {
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.loop = false;
        line.alignment = LineAlignment.View;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.sortingOrder = 10;
    }

    private void OnAim(InputValue value)
    {
        aimInput = value.Get<Vector2>();
    }

    private void OnFire(InputValue value)
    {
        Debug.Log($"Hunter Fire | Value: {value.Get<float>():F3} | Pressed: {value.isPressed}");
        if (!isActiveAndEnabled) return;

        if (value.isPressed)
        {
            if (isHolding || Time.time < nextFireTime) return;

            isHolding = true;
            holdStartTime = Time.time;

            playedSteadySound = false;
            playedFullChargeSound = false;
        }
        else if (isHolding)
        {
            UpdateAiming();
            FireShot();

            isHolding = false;
            HideAimLines();
        }
    }

    private void Update()
    {
        UpdateAiming();

        if (isHolding)
        {
            UpdateAimLines();
        }
    }

    private void UpdateAiming()
    {
        if (aimInput.sqrMagnitude < 0.01f) return;

        float angleFromDown = Mathf.Atan2(aimInput.x, -aimInput.y) * Mathf.Rad2Deg;

        angleFromDown = Mathf.Clamp(angleFromDown, -aimHalfArc, aimHalfArc);

        aimPivot.rotation = Quaternion.Euler(0f, 0f, -90f + angleFromDown);
    }

    private float GetSpread(float heldTime)
    {
        float steadyProgress = Mathf.Clamp01(heldTime / steadyTime);

        return Mathf.Lerp(startingSpread, 0f, steadyProgress);
    }

    private float GetCharge(float heldTime)
    {
        return Mathf.Clamp01((heldTime - steadyTime) / chargeTime);
    }

    private Vector3 GetShotDirection(float angleOffset)
    {
        return Quaternion.Euler(0f, 0f, angleOffset) * gun.right;
    }

    private void UpdateAimLines()
    {
        float heldTime = Time.time - holdStartTime;
        float spread = GetSpread(heldTime);
        float charge = GetCharge(heldTime);
        bool isSteady = heldTime >= steadyTime;

        if (isSteady && !playedSteadySound)
        {
            playedSteadySound = true;

            if (aimAudio != null && steadySound != null)
            {
                aimAudio.PlayOneShot(steadySound);
            }
        }

        if (charge >= 1f && !playedFullChargeSound)
        {
            playedFullChargeSound = true;

            if (aimAudio != null && fullChargeSound != null)
            {
                aimAudio.PlayOneShot(fullChargeSound);
            }
        }

        Color color = isSteady ? Color.Lerp(readyColor, chargedColor, charge) : unsteadyColor;

        float width = Mathf.Lerp(lineWidth, lineWidth * 2f, charge);

        aimLineLeft.enabled = true;
        aimLineRight.enabled = !isSteady;

        DrawLine(aimLineLeft, -spread, color, width);

        if (!isSteady)
        {
            DrawLine(aimLineRight, spread, color, width);
        }
    }

    private void DrawLine(LineRenderer line, float angleOffset, Color color, float width)
    {
        Vector3 origin = gun.position;
        Vector3 direction = GetShotDirection(angleOffset);

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, lineLength, wallLayers);

        Vector3 endPoint = origin + direction * lineLength;

        if (hit.collider != null)
        {
            endPoint = new Vector3(hit.point.x, hit.point.y, origin.z);
        }

        line.SetPosition(0, origin);
        line.SetPosition(1, endPoint);
        line.startColor = color;
        line.endColor = color;
        line.startWidth = width;
        line.endWidth = width;
    }

    private void FireShot()
    {
        float heldTime = Time.time - holdStartTime;
        float spread = GetSpread(heldTime);
        float charge = GetCharge(heldTime);

        float randomOffset = Random.Range(-spread, spread);
        Vector2 direction = GetShotDirection(randomOffset);

        float shotAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float speed = Mathf.Lerp(bulletSpeed, chargedBulletSpeed, charge);

        Rigidbody2D bullet = Instantiate(bulletPrefab, gun.position, Quaternion.Euler(0f, 0f, shotAngle));

        bullet.linearVelocity = direction * speed;

        if (aimAudio != null && fireSound != null)
        {
            aimAudio.PlayOneShot(fireSound);
        }

        Destroy(bullet.gameObject, 3f);
        nextFireTime = Time.time + fireCooldown;
    }

    private void HideAimLines()
    {
        if (aimLineLeft != null) aimLineLeft.enabled = false;
        if (aimLineRight != null) aimLineRight.enabled = false;
    }

    private void OnDisable()
    {
        isHolding = false;
        HideAimLines();
    }

    private void OnRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnQuit() 
    {
        Application.Quit();
    }
}