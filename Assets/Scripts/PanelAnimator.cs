using System.Collections;
using UnityEngine;

public class PanelAnimator : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 startPos;
    private Vector3 hideLeftTargetPos;
    private Vector3 hideRightTargetPos;
    private Vector3 hideTopTargetPos;
    private Vector3 hideDownTargetPos;
    
    public float speed = 30f;
    public float hideDistance = 15f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.position;
        hideLeftTargetPos = startPos + Vector3.left * hideDistance;
        hideRightTargetPos = startPos + Vector3.right * hideDistance;
        hideTopTargetPos = startPos + Vector3.up * hideDistance;
        hideDownTargetPos = startPos + Vector3.down * hideDistance;
    }

    private void OnEnable()
    {
        // Reset position when panel becomes active
        if (rectTransform != null)
        {
            rectTransform.position = startPos;
        }
    }

    public void HideAtStart()
    {
        rectTransform.position = hideRightTargetPos;
    }

    public void ShowFromRight()
    {
        StopAllCoroutines();
        rectTransform.position = hideRightTargetPos;
        StartCoroutine(ShowCoroutine());
    }
    
    public void ShowFromLeft()
    {
        StopAllCoroutines();
        rectTransform.position = hideLeftTargetPos;
        StartCoroutine(ShowCoroutine());
    }
    
    public void HideToLeft()
    {
        StopAllCoroutines();
        StartCoroutine(HideLeftCoroutine());
    }
    
    public void HideToRight()
    {
        StopAllCoroutines();
        StartCoroutine(HideRightCoroutine());
    }
    
    public void ShowFromTop()
    {
        StopAllCoroutines();
        rectTransform.position = hideTopTargetPos;
        StartCoroutine(ShowCoroutine());
    }
    
    public void ShowFromBottom()
    {
        StopAllCoroutines();
        rectTransform.position = hideDownTargetPos;
        StartCoroutine(ShowCoroutine());
    }
    
    public void HideToTop()
    {
        StopAllCoroutines();
        StartCoroutine(HideTopCoroutine());
    }
    
    public void HideToBottom()
    {
        StopAllCoroutines();
        StartCoroutine(HideDownCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        while (Vector3.Distance(rectTransform.position, startPos) > 0.1f)
        {
            yield return null;
            rectTransform.position = Vector3.MoveTowards(rectTransform.position, startPos, speed * Time.unscaledDeltaTime);
        }
        rectTransform.position = startPos;
    }

    private IEnumerator HideLeftCoroutine()
    {
        while (Vector3.Distance(rectTransform.position, hideLeftTargetPos) > 0.1f)
        {
            yield return null;
            rectTransform.position = Vector3.MoveTowards(rectTransform.position, hideLeftTargetPos, speed * Time.unscaledDeltaTime);
        }
        rectTransform.position = hideLeftTargetPos;
        gameObject.SetActive(false);
    }
    
    private IEnumerator HideRightCoroutine()
    {
        while (Vector3.Distance(rectTransform.position, hideRightTargetPos) > 0.1f)
        {
            yield return null;
            rectTransform.position = Vector3.MoveTowards(rectTransform.position, hideRightTargetPos, speed * Time.unscaledDeltaTime);
        }
        rectTransform.position = hideRightTargetPos;
        gameObject.SetActive(false);
    }
    
    private IEnumerator HideTopCoroutine()
    {
        while (Vector3.Distance(rectTransform.position, hideTopTargetPos) > 0.1f)
        {
            yield return null;
            rectTransform.position = Vector3.MoveTowards(rectTransform.position, hideTopTargetPos, speed * Time.unscaledDeltaTime);
        }
        rectTransform.position = hideTopTargetPos;
        gameObject.SetActive(false);
    }
    
    private IEnumerator HideDownCoroutine()
    {
        while (Vector3.Distance(rectTransform.position, hideDownTargetPos) > 0.1f)
        {
            yield return null;
            rectTransform.position = Vector3.MoveTowards(rectTransform.position, hideDownTargetPos, speed * Time.unscaledDeltaTime);
        }
        rectTransform.position = hideDownTargetPos;
        gameObject.SetActive(false);
    }
}
