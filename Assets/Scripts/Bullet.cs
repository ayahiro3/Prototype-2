using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Hit " + collision.gameObject.name);
        Destroy(gameObject);
    }
}