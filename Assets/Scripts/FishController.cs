using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FishController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float swimSpeed = 3f;
    [SerializeField] private float facingOffset = 0f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 9f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private Vector2 dashDirection;
    private float dashEndTime;
    private float nextDashTime;

    [Header("Growth")]
    [SerializeField] private float startScale = 0.5f;
    [SerializeField] private float maxScale = 3f;
    [SerializeField] private float growthPerFood = 0.15f;
    [SerializeField] private float scaleThresholdToEatFisherman = 2.5f;
    [SerializeField, Tooltip("Optional: slow the fish down as it grows, for balance")]
    private float speedLossPerScale = 0f;

    [Header("Health")]
    [SerializeField] private int hitsToKill = 1;
    [SerializeField] private string javelinTag = "Javelin";
    [SerializeField] private string foodTag = "Food";

    [Header("Water")]
    [SerializeField] private float waterSurfaceY = 2f;
    [SerializeField] private float airGravityScale = 1f;
    [SerializeField] private float airTurnSpeed = 180f;

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip eatSfx;
    [Header("UI")]
    [SerializeField] private Slider fullnessBar;

    public static event System.Action OnFishermanEaten;
    public static event System.Action OnFishKilled;

    private Vector2 moveInput;
    private float currentScale;
    private int hitsTaken = 0;
    private bool isDead = false;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentScale = startScale;
        transform.localScale = Vector3.one * currentScale;
        UpdateFullnessBar();
    }

    // Bind this to a "Move" action (Vector2, WASD composite) on the fish's PlayerInput
    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        bool isUnderwater = rb.position.y <= waterSurfaceY;

        if (!isUnderwater)
        {
            rb.gravityScale = airGravityScale;
            FaceAirVelocity();
            return;
        }

        rb.gravityScale = 0f;

        if (Time.time < dashEndTime)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            float angle = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg;
            rb.SetRotation(angle + facingOffset);
            return;
        }

        float currentSpeed = Mathf.Max(swimSpeed - currentScale * speedLossPerScale, 0.5f);

        Vector2 movement = Vector2.ClampMagnitude(moveInput, 1f);
        rb.linearVelocity = movement * currentSpeed;

        FaceMoveDirection();
    }

    private void FaceMoveDirection()
    {
        if (moveInput.sqrMagnitude < 0.01f) return;
        float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
        rb.SetRotation(angle + facingOffset);
    }

    private void Grow()
    {
        currentScale = Mathf.Min(currentScale + growthPerFood, maxScale);

        transform.localScale = Vector3.one * currentScale;

        UpdateFullnessBar();

        if (currentScale >= scaleThresholdToEatFisherman)
        {
            EatFisherman();
        }
    }

    private void EatFisherman()
    {
        if (isDead) return;
        isDead = true;
        OnFishermanEaten?.Invoke();
        Debug.Log("Fish grew big enough to eat the fisherman. Fish player wins.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag(foodTag))
        {
            Destroy(other.gameObject);
            Grow();
            PlayEatSfx();
        }
    }

    // Javelins hit via Rigidbody2D collision (see Bullet.cs), so the fish
    // needs a non-trigger Collider2D alongside the trigger one used for food.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag(javelinTag))
        {
            TakeHit();
        }
    }

    private void TakeHit()
    {
        hitsTaken++;
        if (hitsTaken >= hitsToKill)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        OnFishKilled?.Invoke();
        Debug.Log("Fish killed by javelin. Fisherman player wins.");
        Destroy(gameObject);
    }

    private void FaceAirVelocity()
    {
        Vector2 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude < 0.01f) return;

        float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg + facingOffset;
        float nextAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, airTurnSpeed * Time.fixedDeltaTime);

        rb.SetRotation(nextAngle);
    }

    private void PlayEatSfx()
    {
        if (audioSource != null && eatSfx != null)
        {
            audioSource.PlayOneShot(eatSfx);
        }
    }

    private void UpdateFullnessBar()
    {
        if (fullnessBar == null) return;

        fullnessBar.value = Mathf.InverseLerp(startScale, scaleThresholdToEatFisherman, currentScale);
    }

    private void OnDash(InputValue value)
    {
        if (!value.isPressed || isDead) return;

        if (rb.position.y > waterSurfaceY) return;
        if (Time.time < dashEndTime || Time.time < nextDashTime) return;

        if (moveInput.sqrMagnitude >= 0.01f)
        {
            dashDirection = moveInput.normalized;
        }
        else
        {
            float angle = (rb.rotation - facingOffset) * Mathf.Deg2Rad;
            dashDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        dashEndTime = Time.time + dashDuration;
        nextDashTime = dashEndTime + dashCooldown;
    }
}