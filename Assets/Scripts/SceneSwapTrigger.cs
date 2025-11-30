using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class SceneSwapTrigger : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private string targetSceneName = "";
    [SerializeField] private bool triggerOnce = true;

    [Header("Players Required Inside Trigger")]
    [SerializeField] private GameObject playerOne;
    [SerializeField] private GameObject playerTwo;

    private Collider2D _collider;
    private readonly HashSet<GameObject> _playersInside = new HashSet<GameObject>();
    private bool _hasTriggered;

    private void Reset()
    {
        _collider = GetComponent<Collider2D>();
        if (_collider != null)
        {
            _collider.isTrigger = true;
        }
    }

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        if (_collider != null && !_collider.isTrigger)
        {
            _collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TrackPlayer(other.gameObject, true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TrackPlayer(other.gameObject, false);
    }

    private void TrackPlayer(GameObject candidate, bool isInside)
    {
        if (candidate == null)
        {
            return;
        }

        if (candidate == playerOne || candidate == playerTwo)
        {
            if (isInside)
            {
                _playersInside.Add(candidate);
                TrySwitchScene();
            }
            else
            {
                _playersInside.Remove(candidate);
            }
        }
    }

    private void TrySwitchScene()
    {
        if (_hasTriggered && triggerOnce)
        {
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"{nameof(SceneSwapTrigger)} on {name} has no target scene assigned.");
            return;
        }

        if (playerOne == null || playerTwo == null)
        {
            Debug.LogWarning($"{nameof(SceneSwapTrigger)} on {name} requires both players to be assigned.");
            return;
        }

        if (_playersInside.Contains(playerOne) && _playersInside.Contains(playerTwo))
        {
            _hasTriggered = true;
            ScenesController.Instance.ChangeScene(targetSceneName);
        }
    }
}
