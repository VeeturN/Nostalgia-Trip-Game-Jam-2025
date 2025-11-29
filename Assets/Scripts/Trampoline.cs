using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingTrap : MonoBehaviour
{
    [SerializeField] private float impactSpeed = 10f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")||collision.gameObject.CompareTag("Player2"))
        {
            GameObject player = collision.collider.gameObject;
            if (player != null)
            {
                Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();
                rb2d.velocity = Vector2.zero;
                rb2d.AddForce(Vector2.up * impactSpeed, ForceMode2D.Impulse);
            }
        }
    }
}