using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class ToggleAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler
    {
        private Vector3 originalScale;
        public float hoverScale = 0.9f;
        public float clickScale = 1.1f;
        public float speed = 10f;

        private Vector3 targetScale;

        void Start()
        {
            originalScale = transform.localScale;
            targetScale = originalScale;
        }

        void OnEnable()
        {
            if (originalScale != Vector3.zero)
            {
                targetScale = originalScale;
                transform.localScale = originalScale;
            }
        }

        void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * speed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetScale = originalScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            targetScale = originalScale * clickScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            targetScale = originalScale * hoverScale;
        }
    }
}