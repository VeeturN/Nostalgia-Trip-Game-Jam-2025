using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform of the object the camera should follow. If left empty, the script will try to find a GameObject with tag 'Player'.")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("Offset from the target's position. Default keeps camera 10 units back on Z for 2D scenes.")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Tooltip("Approximate time (in seconds) the camera takes to reach the target position. Lower = snappier, higher = smoother/slower.")]
    [Min(0f)]
    public float smoothTime = 0.15f;

    // Velocity used by SmoothDamp (do not modify from inspector)
    private Vector3 currentVelocity = Vector3.zero;

    // Try to find the player if no target assigned in the inspector
    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    // Use LateUpdate so camera follows after all character movement is applied
    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;
        // Smoothly move the camera using SmoothDamp (framerate-independent)
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
    }
}
