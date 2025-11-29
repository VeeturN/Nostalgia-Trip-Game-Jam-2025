using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))] 
public class RetroMovementRaycast : MonoBehaviour
{
    [Header("Ustawienia Ruchu")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;

    [Header("Ustawienia Drabiny")]
    public float climbSpeed = 5f;
    public LayerMask ladderLayer;

    [Header("Wykrywanie Ziemi (Raycast)")]
    public LayerMask groundLayer; 

    private Rigidbody2D rb;
    private BoxCollider2D col;
    
    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    
    // Zmienne drabiny
    private bool isTouchingLadder;
    private bool isClimbing;
    private float defaultGravity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        rb.freezeRotation = true; 
        rb.gravityScale = 10f; 
        
        defaultGravity = rb.gravityScale;
    }

    void Update()
    {

        horizontalInput = Input.GetAxisRaw("Horizontal_P1");
        verticalInput = Input.GetAxisRaw("Vertical_P1"); 
        
        if (Input.GetButtonDown("Jump_P1"))
        {
            if (isClimbing)
            {
                isClimbing = false;
                rb.gravityScale = defaultGravity;
                
                rb.velocity = new Vector2(rb.velocity.x, 0); 
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            else if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0); 
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }

    void FixedUpdate()
    {
        CheckGround();
        CheckLadder();
        
        if (isTouchingLadder && Mathf.Abs(verticalInput) > 0.1f)
        {
            isClimbing = true;
        }
        else if (!isTouchingLadder && isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = defaultGravity;
        }

        if (isClimbing)
        {
            MoveLadder();
        }
        else
        {
            MoveNormal();
        }
    }

    private void MoveNormal()
    {
        rb.gravityScale = defaultGravity;
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    private void MoveLadder()
    {
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(0, verticalInput * climbSpeed);
    }

    private void CheckLadder()
    {
        isTouchingLadder = col.IsTouchingLayers(ladderLayer);
    }

    private void CheckGround()
    {
        float playerHalfHeight = col.bounds.extents.y;
        float playerHalfWidth = col.bounds.extents.x;
        float rayLength = playerHalfHeight + 0.1f;
        Vector2 center = col.bounds.center;
        
        bool hitLeft = Physics2D.Raycast(center + Vector2.left * (playerHalfWidth * 0.8f), Vector2.down, rayLength, groundLayer);
        bool hitRight = Physics2D.Raycast(center + Vector2.right * (playerHalfWidth * 0.8f), Vector2.down, rayLength, groundLayer);
        
        isGrounded = (hitLeft || hitRight) && rb.velocity.y <= 0.1f;
    }

    void OnDrawGizmos()
    {
        if (col != null)
        {
            float h = col.bounds.extents.y;
            float w = col.bounds.extents.x;
            float rayLen = h + 0.1f;
            Vector2 center = col.bounds.center;

            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawRay(center + Vector2.left * (w * 0.8f), Vector2.down * rayLen);
            Gizmos.DrawRay(center + Vector2.right * (w * 0.8f), Vector2.down * rayLen);
        }
    }
}