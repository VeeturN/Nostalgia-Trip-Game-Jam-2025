using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Banana : MonoBehaviour
{
    [SerializeField]private int BananaValue = 1;
    [SerializeField] private string _playerTag = "Player";
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(_playerTag))
        {
           RetroMovement.CurrentScore += BananaValue;
            
            Destroy(gameObject);
        }
    }
}
