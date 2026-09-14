using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Food : MonoBehaviour
{
    [SerializeField] private float lifetime = 8f; // despawn if never eaten

    private void Start()
    {
        // Make sure this GameObject's Collider2D is set to "Is Trigger" in the Inspector
        if (lifetime > 0f)
        {
            Destroy(gameObject, lifetime);
        }
    }
}