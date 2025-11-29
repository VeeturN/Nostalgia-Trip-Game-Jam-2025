using System.Collections; 
using UnityEngine;

public class PlayerMovementAddForce : MonoBehaviour
{
    [Header("Ustawienia Gracza")]
    
    [Header("Input")]
    [SerializeField] private string _horizontalAxis = "Horizontal_P1"; 
    [SerializeField] private string _verticalAxis = "Vertical_P1"; 
    [SerializeField] private string _jumpButton = "Jump_P1";           
    [SerializeField] private string _grappleButton = "Fire1_P1";       
    [SerializeField] private string _dashButton = "Fire2_P1"; 

    [Header("Ruch Fizyczny (AddForce)")]
    [SerializeField] private float _maxMoveSpeed = 8f;      // Maksymalna prędkość biegu
    [SerializeField] private float _acceleration = 60f;     // Jak szybko się rozpędza (Siła)
    [SerializeField] private float _groundLinearDrag = 10f; // Hamowanie na ziemi (Tarcie)
    [SerializeField] private float _airLinearDrag = 2f;     // Hamowanie w powietrzu
    [SerializeField] private float _jumpForce = 15f;

    [Header("Dash (Zryw - Impulse)")]
    [SerializeField] private float _dashForce = 20f;      // Siła dasha (teraz jako Impuls)
    [SerializeField] private float _dashDuration = 0.2f; 
    
    [Header("Wall Mechanics (Ściany)")]
    [SerializeField] private float _wallSlideSpeed = 2f;      
    [SerializeField] private Vector2 _wallJumpForce = new Vector2(10f, 15f); 
    [SerializeField] private float _wallJumpStopInputTime = 0.2f; 
    [SerializeField] private LayerMask _wallLayer; 
    
    [Header("Spider-Man Mode (Lina)")]
    [SerializeField] private float _swingForce = 50f;          
    [SerializeField] private float _grappleShootSpeed = 60f; 
    
    [Header("Spider-Man Mode (Lot swobodny)")]
    [SerializeField] private float _momentumAirControl = 10f;  
    [SerializeField] private float _momentumBrakeForce = 35f;  
    [SerializeField] private float _momentumDrag = 0.5f; 

    [Header("Auto-Aim (Haki)")]
    [SerializeField] private float _scanRadius = 15f; 
    [SerializeField] private LayerMask _hookLayer;   
    [SerializeField] private LayerMask _obstacleLayer; 
    [SerializeField] private GameObject _aimReticle;   

    [Header("Wykrywanie Ziemi")]
    [SerializeField] private LayerMask _groundLayer; 

    // Komponenty
    private Rigidbody2D _rb;
    private DistanceJoint2D _ropeJoint;
    private LineRenderer _lineRenderer;

    private float _horizontalInput;
    private float _verticalInput; 
    
    // Stany
    private bool _isSwinging = false;      
    private bool _isGrapplingRope = false; 
    private bool _momentumMode = false; 
    
    private bool _jumpRequest = false; 
    private bool _grappleRequest = false; 
    private bool _grappleRelease = false; 
    private bool _isGrounded;         
    
    // Stany Ścian
    private bool _isTouchingWall;
    private bool _isWallSliding;
    private int _wallDirection; 
    private float _wallJumpInputTimer;

    // Stany Dasha
    private bool _isDashing;       
    private bool _canDash = true;  
    private float _facingDirection = 1; 

    private Vector2 _grapplePoint;         
    private Vector2 _ropeStartPosition;    
    private float _ropeFlightTimeElapsed;  
    private float _defaultDrag; 
    private float _defaultGravity; 
    private Transform _currentBestHook; 

    private float _playerHalfWidth;
    private float _playerHalfHeight;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _ropeJoint = GetComponent<DistanceJoint2D>();
        _lineRenderer = GetComponent<LineRenderer>();
        
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        _playerHalfWidth = col.bounds.extents.x;
        _playerHalfHeight = col.bounds.extents.y;

        _rb.freezeRotation = true; 
        _defaultGravity = _rb.gravityScale; 
        
        // Zwiększamy masę dla lepszej stabilności fizyki, jeśli potrzebne, 
        // ale standardowo 1 jest ok przy tych wartościach sił.
    }

    void Start()
    {
        _ropeJoint.enabled = false;
        _lineRenderer.enabled = false;
        _defaultDrag = 0f; // Ustawiamy na 0, bo sami liczymy Drag
        _rb.drag = 0f;
        
        _aimReticle.SetActive(false);
    }

    void Update()
    {
        if (_isDashing) 
            return; 

        _horizontalInput = Input.GetAxisRaw(_horizontalAxis);
        _verticalInput = Input.GetAxisRaw(_verticalAxis);

        if (_horizontalInput != 0) 
        {
            _facingDirection = Mathf.Sign(_horizontalInput);
        }

        if (Input.GetButtonDown(_jumpButton))
            _jumpRequest = true;
        if (Input.GetButtonDown(_grappleButton)) 
            _grappleRequest = true;
        if (Input.GetButtonUp(_grappleButton)) 
            _grappleRelease = true;
        
        // Dash 
        if (Input.GetButtonDown(_dashButton) && _canDash)
        {
            StartCoroutine(PerformDash());
        }

        if (!_isSwinging && !_isGrapplingRope) 
            FindBestHook();
        else
            _aimReticle.SetActive(false);

        // RESETOWANIE STANÓW NA ZIEMI
        if (_isGrounded && !_isSwinging && !_isGrapplingRope)
        {
            _momentumMode = false;
            // Tutaj nie resetujemy draga Rigidbody, bo używamy własnego systemu draga
            _canDash = true; 
        }
        
        // Reset dasha na ścianie
        if (_isWallSliding) 
        {
            _canDash = true; 
        }

        if (_grappleRequest)
        {
            StartGrapple(); 
            _grappleRequest = false;
        }
        else if (_grappleRelease)
        {
            StopGrapple();
            _grappleRelease = false;
        }

        if (_isGrapplingRope) 
            AnimateRopeFlying();
        else if (_isSwinging)
        {
            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.SetPosition(1, _ropeJoint.connectedAnchor);
        }
    }

    void FixedUpdate()
    {
        if (_isDashing) return; 

        CheckGround();
        CheckWall();

        if (_wallJumpInputTimer > 0) 
            _wallJumpInputTimer -= Time.fixedDeltaTime;

        if (_isSwinging)
        {
            if (_horizontalInput != 0) 
                _rb.AddForce(new Vector2(_horizontalInput * _swingForce, 0));
            _jumpRequest = false; 
            _isWallSliding = false;
        }
        else if (_momentumMode && !_isWallSliding)
        {
            HandleMomentumAirControl();
            _jumpRequest = false; 
        }
        else
        {
            HandleWallLogic();
            HandleGroundMovement(); // To teraz używa AddForce
            ApplyLinearDrag();      // To hamuje postać, gdy nie ma inputu
        }
    }

    // Nowa funkcja do hamowania (zamiast rb.drag, który psuje grawitację)
    void ApplyLinearDrag()
    {
        // Hamujemy tylko po osi X, żeby nie spowalniać spadania (grawitacji)
        if (Mathf.Abs(_horizontalInput) < 0.4f || (_horizontalInput != 0 && Mathf.Sign(_horizontalInput) != Mathf.Sign(_rb.velocity.x)))
        {
            float drag = _isGrounded ? _groundLinearDrag : _airLinearDrag;
            
            // Aplikujemy siłę przeciwną do prędkości
            _rb.AddForce(new Vector2(-_rb.velocity.x * drag, 0));
        }
    }

    private IEnumerator PerformDash()
    {
        _isDashing = true;
        _canDash = false; 
        
        float originalGravity = _rb.gravityScale;
        _rb.gravityScale = 0f; 
        
        // Reset prędkości przed dashem dla spójności, albo dodajemy do obecnej
        _rb.velocity = Vector2.zero; 
        
        Vector2 dashDir;
        if (_horizontalInput == 0 && _verticalInput == 0)
        {
            dashDir = new Vector2(_facingDirection, 0);
        }
        else
        {
            dashDir = new Vector2(_horizontalInput, _verticalInput).normalized;
        }

        // ZMIANA: Używamy AddForce Impulse zamiast ustawiania velocity
        _rb.AddForce(dashDir * _dashForce, ForceMode2D.Impulse);

        if (_isSwinging) StopGrapple(); 

        yield return new WaitForSeconds(_dashDuration);
        
        _rb.gravityScale = originalGravity; 
        _isDashing = false;
        _momentumMode = false; 
        // Po dashu zerujemy velocity, żeby nie leciał dalej siłą rozpędu (chyba że wolisz inaczej)
        _rb.velocity = Vector2.zero; 
    }

    void HandleWallLogic() {
        bool pushingWall = (_wallDirection == 1 && _horizontalInput > 0) || (_wallDirection == -1 && _horizontalInput < 0);
        
        // Zmienione: logika zjeżdżania też na siłach lub limicie prędkości
        if (_isTouchingWall && !_isGrounded && _rb.velocity.y < 0 && pushingWall) 
        {
            _isWallSliding = true; 
            _momentumMode = false;
            
            // Jeśli spadamy za szybko przy ścianie, hamujemy siłą przeciwną do grawitacji
            if (_rb.velocity.y < -_wallSlideSpeed) 
            {
                // Zamiast ustawiać velocity, dodajemy siłę w górę równoważącą grawitację
                // Ale dla prostoty w tym miejscu "clamp" velocity jest bezpieczniejszy dla stabilności
                _rb.velocity = new Vector2(_rb.velocity.x, -_wallSlideSpeed);
            }
        }
        else
        {
            _isWallSliding = false;
        }
        if (_jumpRequest && (_isWallSliding || (_isTouchingWall && !_isGrounded))) 
            PerformWallJump();
    }

    void PerformWallJump() {
        // Resetujemy prędkość, żeby skok był zawsze taki sam niezależnie od spadania
        _rb.velocity = Vector2.zero; 
        
        float jumpDir = -_wallDirection;
        Vector2 force = new Vector2(_wallJumpForce.x * jumpDir, _wallJumpForce.y);
        
        // Wall Jump już był na AddForce (Impulse)
        _rb.AddForce(force, ForceMode2D.Impulse);
        
        _wallJumpInputTimer = _wallJumpStopInputTime;
        _jumpRequest = false;
        _momentumMode = false;
    }

    void HandleGroundMovement() {
        if (_wallJumpInputTimer > 0) 
            return;

        // ZMIANA NA ADDFORCE
        // 1. Dodajemy siłę
        _rb.AddForce(new Vector2(_horizontalInput * _acceleration, 0));

        // 2. Limitujemy prędkość (Clamp), żeby nie rozpędzał się w nieskończoność
        if (Mathf.Abs(_rb.velocity.x) > _maxMoveSpeed)
        {
            // Zachowujemy obecny znak prędkości, ale ucinamy do max speed
            _rb.velocity = new Vector2(Mathf.Sign(_rb.velocity.x) * _maxMoveSpeed, _rb.velocity.y);
        }

        if (_jumpRequest && _isGrounded)
        {
            // Resetujemy prędkość Y przed skokiem dla spójnej wysokości
            _rb.velocity = new Vector2(_rb.velocity.x, 0);
            
            // ZMIANA: Skok to teraz czysty impuls siły
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
            
            _jumpRequest = false;
        }
        else
        {
            _jumpRequest = false;
        }
    }

    void FindBestHook() {
        Collider2D[] hooks = Physics2D.OverlapCircleAll(transform.position, _scanRadius, _hookLayer);
        float closestDistance = Mathf.Infinity; 
        Transform bestTarget = null;
        foreach (var hook in hooks) 
        {
            float distance = Vector2.Distance(transform.position, hook.transform.position);
            Vector2 dir = hook.transform.position - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance, _obstacleLayer);
            if (hit.collider == null)
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance; 
                    bestTarget = hook.transform;
                }
            }
        }
        _currentBestHook = bestTarget;
        if (_currentBestHook != null && _aimReticle != null)
        {
            _aimReticle.SetActive(true);
            _aimReticle.transform.position = _currentBestHook.position;
        }
        else if (_aimReticle != null)
        {
            _aimReticle.SetActive(false);
        }
    }

    void HandleMomentumAirControl() {
        bool isBraking = (_horizontalInput > 0 && _rb.velocity.x < -0.1f) || (_horizontalInput < 0 && _rb.velocity.x > 0.1f);
        if (isBraking)
            _rb.AddForce(new Vector2(_horizontalInput * _momentumBrakeForce, 0));
        else if (_horizontalInput != 0) 
            _rb.AddForce(new Vector2(_horizontalInput * _momentumAirControl, 0));
        
        _rb.velocity = Vector2.ClampMagnitude(_rb.velocity, 30f);
    }

    void AnimateRopeFlying() {
        _ropeFlightTimeElapsed += Time.deltaTime;
        float totalDistance = Vector2.Distance(_ropeStartPosition, _grapplePoint); 
        float t = _ropeFlightTimeElapsed / (totalDistance / _grappleShootSpeed); 
        Vector2 currentPos = Vector2.Lerp(_ropeStartPosition, _grapplePoint, t); 
        _lineRenderer.SetPosition(0, transform.position); _lineRenderer.SetPosition(1, currentPos);
        if (t >= 1f)
        {
            _isGrapplingRope = false; 
            _isSwinging = true;
            _ropeJoint.connectedAnchor = _grapplePoint;
            _ropeJoint.distance = Vector2.Distance(transform.position, _grapplePoint);
            _ropeJoint.enabled = true;
        }
    }

    void StartGrapple() {
        if (_isGrapplingRope || _isSwinging) 
            return;
        
        if (_currentBestHook != null) 
        {
            _momentumMode = true; 
            // W trybie AddForce nie używamy rb.drag do hamowania, więc tu resetujemy lub ustawiamy minimalny
            _rb.drag = 0f; 
            
            _grapplePoint = _currentBestHook.position;
            _ropeStartPosition = transform.position; _ropeFlightTimeElapsed = 0f;
            _isGrapplingRope = true;
            _lineRenderer.enabled = true; 
            _lineRenderer.SetPosition(0, _ropeStartPosition); 
            _lineRenderer.SetPosition(1, _ropeStartPosition);
        }
    }

    void StopGrapple() {
        _isSwinging = false;
        _isGrapplingRope = false;
        _ropeJoint.enabled = false; 
        _lineRenderer.enabled = false; 
        
        if(_aimReticle != null) 
            _aimReticle.SetActive(false);
    }

    private void CheckGround() {
        float rayLength = _playerHalfHeight + 0.1f; 
        bool hitLeft = Physics2D.Raycast(transform.position + Vector3.left * (_playerHalfWidth * 0.9f), Vector2.down, rayLength, _groundLayer); 
        bool hitRight = Physics2D.Raycast(transform.position + Vector3.right * (_playerHalfWidth * 0.9f), Vector2.down, rayLength, _groundLayer); 
        _isGrounded = (hitLeft || hitRight) && _rb.velocity.y <= 0.1f;
    }

    private void CheckWall() {
        float rayDist = _playerHalfWidth + 0.1f; 
        bool rightWall = Physics2D.Raycast(transform.position, Vector2.right, rayDist, _wallLayer);
        bool leftWall = Physics2D.Raycast(transform.position, Vector2.left, rayDist, _wallLayer);
        if (rightWall)
        {
            _isTouchingWall = true; 
            _wallDirection = 1;
        } 
        else if (leftWall)
        {
            _isTouchingWall = true;
            _wallDirection = -1;
        }
        else
        {
            _isTouchingWall = false;
            _wallDirection = 0;
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow; 
        Gizmos.DrawWireSphere(transform.position, _scanRadius);
    }
}