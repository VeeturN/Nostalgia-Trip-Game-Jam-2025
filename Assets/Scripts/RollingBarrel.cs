using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class RollingBarrel : MonoBehaviour
{
    [Header("Ustawienia Beczki")]
    public float pushForce = 5f;      
    public float maxSpeed = 15f;      
    
    // [Header("Zniszczenia")]
    // public float killSpeed = 2f;     
    // public string enemyTag = "Enemy"; 

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        rb.gravityScale = 3f; 
        rb.mass = 1f;         
        rb.drag = 0.5f;     
        rb.angularDrag = 0.1f;
    }

    void FixedUpdate()
    {
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 direction = collision.transform.position.x < transform.position.x ? Vector2.right : Vector2.left;
            rb.AddForce(direction * pushForce, ForceMode2D.Force);
        }
    }
}