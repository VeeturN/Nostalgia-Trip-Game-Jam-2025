using UnityEngine;

public class SeesawBooster : MonoBehaviour
{
    public float impactMultiplier = 2f; 

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D otherRb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (otherRb != null)
        {
                Vector2 contactPoint = collision.GetContact(0).point;
                Vector2 localPoint = transform.InverseTransformPoint(contactPoint);
                
                float direction = localPoint.x > 0 ? -1f : 1f;
                
                float force = otherRb.mass * 1 * impactMultiplier;
                
                rb.AddTorque(force * direction, ForceMode2D.Impulse);
            
        }
    }
}