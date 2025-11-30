using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Camera))]
public class MenuCameraZoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomDuration = 0.75f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float zoomedSize = 10f;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private bool lockRotationToDefault = true;
    [SerializeField] private bool keepDefaultDepth = true;

    private Camera _camera;
    private Vector3 _defaultPosition;
    private Quaternion _defaultRotation;
    private float _defaultSize;
    private Vector3 _defaultForward;
    private Vector3 _defaultRight;
    private Vector3 _defaultUp;
    private Coroutine _currentRoutine;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _defaultPosition = transform.position;
        _defaultRotation = transform.rotation;
        _defaultSize = _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView;
        _defaultForward = _defaultRotation * Vector3.forward;
        _defaultRight = _defaultRotation * Vector3.right;
        _defaultUp = _defaultRotation * Vector3.up;
    }

    public void ZoomToTarget(Transform focus)
    {
        if (focus == null)
        {
            return;
        }

        Vector3 targetWorldPosition;
        Quaternion targetWorldRotation;

        if (lockRotationToDefault)
        {
            Vector3 worldOffset = (_defaultRight * targetOffset.x) + (_defaultUp * targetOffset.y);
            float depthOffset = keepDefaultDepth ? 0f : targetOffset.z;
            targetWorldPosition = focus.position + worldOffset + (_defaultForward * depthOffset);
            if (keepDefaultDepth)
            {
                targetWorldPosition = new Vector3(targetWorldPosition.x, targetWorldPosition.y, _defaultPosition.z);
            }
            targetWorldRotation = _defaultRotation;
        }
        else
        {
            targetWorldPosition = focus.TransformPoint(targetOffset);
            if (keepDefaultDepth)
            {
                targetWorldPosition = new Vector3(targetWorldPosition.x, targetWorldPosition.y, _defaultPosition.z);
            }
            targetWorldRotation = Quaternion.LookRotation(focus.position - targetWorldPosition, Vector3.up);
        }

        float targetSize = zoomedSize;

        BeginTransition(targetWorldPosition, targetWorldRotation, targetSize);
    }

    public void ResetZoom()
    {
        BeginTransition(_defaultPosition, _defaultRotation, _defaultSize);
    }

    private void BeginTransition(Vector3 targetPos, Quaternion targetRot, float targetSize)
    {
        if (_currentRoutine != null)
        {
            StopCoroutine(_currentRoutine);
        }

        _currentRoutine = StartCoroutine(ZoomRoutine(targetPos, targetRot, targetSize));
    }

    private IEnumerator ZoomRoutine(Vector3 targetPos, Quaternion targetRot, float targetSize)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float startSize = _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView;

        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zoomDuration);
            float curve = easeCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPos, targetPos, curve);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, curve);

            if (_camera.orthographic)
            {
                _camera.orthographicSize = Mathf.Lerp(startSize, targetSize, curve);
            }
            else
            {
                _camera.fieldOfView = Mathf.Lerp(startSize, targetSize, curve);
            }

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        if (_camera.orthographic)
        {
            _camera.orthographicSize = targetSize;
        }
        else
        {
            _camera.fieldOfView = targetSize;
        }

        _currentRoutine = null;
    }
}
