using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class FallingIsland : MonoBehaviour
{
    [FormerlySerializedAs("fallDelay")]
    [Header("Ustawienia platformy")]
    [SerializeField] private float _fallDelay = 1.5f;     // po jakim czasie zacznie spadać
    [SerializeField] private float _respawnDelay = 2f;    // po jakim czasie się zrespi
    [SerializeField] private Rigidbody2D _rb;//to jest nasza fizyka w tym obiekcie
    
    //te dwie linijki odpowiadają za pozycję startową platformy
    private Vector3 _startPosition; 
    private Quaternion _startRotation;
    
    private bool _isFalling = false;//platforma spada?
    private bool _isRespawning = false;//platforma respi się?
    private bool _invisible = false;

    void Start()
    {
        //tutaj zabezpieczenie aby na pewno było przypisywane Rigidbody2D
        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        _rb.bodyType = RigidbodyType2D.Kinematic;//tutaj aby stan początkowy był nieruchomy(kinematic)
        //po ustawieniu zostały przypisane pozycje aby w tym miejscy się respawnowała
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    void Update()
    {
        // tutaj jak minie wyznaczony pułap to dopiero zaczyna się czar restartu
        if (!_isRespawning && _invisible)
        {
            StartCoroutine(RespawnPlatform());//tutaj jest czerowne bo rider mówi "jak będzie w lubi będzie bardzo kosztowne dla kompa" ale w tym przypdaku raz na spadek to robimy wiec ok
        }
    }

    public void OnBecameInvisible()
    {
        if (!_isRespawning && _isFalling)
            _invisible = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isFalling && (collision.gameObject.CompareTag("Player")||collision.gameObject.CompareTag("Player2")))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f) 
                {
                    StartCoroutine(FallAfterDelay());
                    break;
                }
            }
        }
    }

    
    private IEnumerator FallAfterDelay()
    {
        _isFalling = true;
        yield return new WaitForSeconds(_fallDelay);
        GetComponent<Collider2D>().enabled = false;
        _rb.bodyType = RigidbodyType2D.Dynamic; 
        _rb.AddForce(new Vector2(0, -500));
    }

    //to jest proces respawnowania
    private IEnumerator RespawnPlatform()
    {
        _isRespawning = true;
        GetComponent<Collider2D>().enabled = true;


        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        GetComponent<SpriteRenderer>().enabled = false;//tutaj tak samo jak respawn platform
        GetComponent<Collider2D>().enabled = false;
        
        yield return new WaitForSeconds(_respawnDelay);
        
        transform.position = _startPosition;
        transform.rotation = _startRotation;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        _invisible = false;
        
        _isFalling = false;
        _isRespawning = false;
    }
}
