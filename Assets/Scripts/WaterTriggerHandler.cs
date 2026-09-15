using UnityEngine;

public class WaterTriggerHandler : MonoBehaviour
{
    [SerializeField] private LayerMask waterMask;
    private EdgeCollider2D edgeColl;
    private InteractableWater water;

    private void Awake() {
        edgeColl = GetComponent<EdgeCollider2D>();
        water = GetComponent<InteractableWater>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if ((waterMask.value & (1 << other.gameObject.layer)) > 0)
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                int multiplier = 1;
                if (rb.linearVelocity.y < 0)
                {
                    multiplier = -1;
                } else
                {
                    multiplier = 1;
                }
                float vel = rb.linearVelocity.y * water.ForceMultiplier;
                vel = Mathf.Clamp(Mathf.Abs(vel), 0f, water.maxForce);
                vel *= multiplier;

                water.Splash(other, vel);
            }
        }
    }
}
