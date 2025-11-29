using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingIsland : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float _moveSpeed;
    private Vector3 _nextPosition;

    private void Start()
    {
        _nextPosition = pointB.position;
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, _nextPosition, _moveSpeed*Time.deltaTime);
        if(transform.position == _nextPosition)
        {
            _nextPosition = (_nextPosition==pointA.position)?pointB.position:pointA.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")|| collision.gameObject.CompareTag("Player2"))
        {
            collision.gameObject.transform.parent = transform;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")|| collision.gameObject.CompareTag("Player2"))
        {
            collision.gameObject.transform.parent = null;
        }
    }
}