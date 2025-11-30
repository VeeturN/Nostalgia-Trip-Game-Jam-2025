using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EndPanelTrigger : MonoBehaviour
{
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private GameObject playerOne;
    [SerializeField] private GameObject playerTwo;

    private bool _hasTriggered;
    private Collider2D _collider;
    private readonly System.Collections.Generic.HashSet<GameObject> _playersInside = new System.Collections.Generic.HashSet<GameObject>();

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
        if (_collider != null)
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
        if (_hasTriggered && triggerOnce)
        {
            return;
        }

        if (candidate == playerOne || candidate == playerTwo)
        {
            if (isInside)
            {
                _playersInside.Add(candidate);
            }
            else
            {
                _playersInside.Remove(candidate);
            }

            TryShowEndPanel();
        }
    }

    private void TryShowEndPanel()
    {
        if (_hasTriggered && triggerOnce)
        {
            return;
        }

        if (playerOne == null || playerTwo == null)
        {
            Debug.LogWarning($"{nameof(EndPanelTrigger)} on {name} requires both player references.");
            return;
        }

        if (_playersInside.Contains(playerOne) && _playersInside.Contains(playerTwo))
        {
            var controller = UIController.UIControllerInstance;
            if (controller != null)
            {
                controller.ShowEndPanel();
                _hasTriggered = true;
            }
            else
            {
                Debug.LogWarning($"{nameof(EndPanelTrigger)} on {name} could not find an active UIController instance.");
            }
        }
    }
}
