using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BarrelsSpawner : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _direction;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private float _destroyTime;
    [SerializeField] private Barrel _barrelPrefab;
    private float _timer;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _spawnDelay)
        {
            _timer = 0f;
            Barrel barrel = Instantiate(_barrelPrefab, transform.position, transform.rotation);
            barrel.Init(_destroyTime);
            Rigidbody2D barrelRigidbody = barrel.GetComponent<Rigidbody2D>();

            Vector2 direction = new Vector2(
                Mathf.Cos(_direction * Mathf.Deg2Rad),
                Mathf.Sin(_direction * Mathf.Deg2Rad)
            ).normalized;

            barrelRigidbody.AddForce(direction * _speed, ForceMode2D.Impulse);
        }
    }
}