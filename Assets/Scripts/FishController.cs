using UnityEngine;
using UnityEngine.InputSystem;

public class FishController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float swimSpeed = 3f;
    [SerializeField] private Vector2 swimBoundsMin = new Vector2(-8f, -4f);
    [SerializeField] private Vector2 swimBoundsMax = new Vector2(8f, 4f);

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

    public static event System.Action OnFishermanEaten;
    public static event System.Action OnFishKilled;

    private Vector2 moveInput;
    private float currentScale;
    private int hitsTaken = 0;
    private bool isDead = false;

    private void Start()
    {
        currentScale = startScale;
        transform.localScale = Vector3.one * currentScale;
    }

    // Bind this to a "Move" action (Vector2, WASD composite) on the fish's PlayerInput
    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void Update()
    {
        if (isDead) return;

        float currentSpeed = Mathf.Max(swimSpeed - (currentScale * speedLossPerScale), 0.5f);

        Vector2 nextPos = (Vector2)transform.position + moveInput.normalized * currentSpeed * Time.deltaTime;
        nextPos.x = Mathf.Clamp(nextPos.x, swimBoundsMin.x, swimBoundsMax.x);
        nextPos.y = Mathf.Clamp(nextPos.y, swimBoundsMin.y, swimBoundsMax.y);
        transform.position = nextPos;

        FaceMoveDirection();
    }

    private void FaceMoveDirection()
    {
        if (Mathf.Abs(moveInput.x) < 0.01f) return;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(moveInput.x);
        transform.localScale = scale;
    }

    private void Grow()
    {
        currentScale = Mathf.Min(currentScale + growthPerFood, maxScale);

        float signX = Mathf.Sign(transform.localScale.x == 0 ? 1 : transform.localScale.x);
        transform.localScale = new Vector3(signX * currentScale, currentScale, currentScale);

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
}