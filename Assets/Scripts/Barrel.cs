using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    private float _destroyTime;
    float timer;
    private Rigidbody2D rb;
    
    public void Init(float destroyTime)
    {
        _destroyTime = destroyTime;
    }
    // Start is called before the first frame update
    void Start()
    {
        timer = 0f;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > _destroyTime)
        {
            Destroy(gameObject);
        }
        
    }
    
}