using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ( !collision.gameObject.CompareTag("Food"))
        {
            Destroy(gameObject);
        }
    }
}