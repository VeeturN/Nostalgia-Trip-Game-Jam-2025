using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace UI
{
    public class TextColorRandomizer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private bool includeChildren = true;
        [SerializeField] private bool loopSequentially = true;

        private static readonly Color32[] Palette =
        {
            new Color32(0xFF, 0xD4, 0x40, 0xFF),
            new Color32(0xD9, 0x44, 0x85, 0xFF),
            new Color32(0x3E, 0x99, 0xC9, 0xFF)
        };

        private TMP_Text _textComponent;
        private Color _originalColor;
        private int _colorIndex;

        private void Awake()
        {
            _textComponent = includeChildren
                ? GetComponentInChildren<TMP_Text>()
                : GetComponent<TMP_Text>();

            if (_textComponent != null)
            {
                _originalColor = _textComponent.color;
            }
            else
            {
                Debug.LogWarning($"{nameof(TextColorRandomizer)} on {name} did not find a TextMeshPro component.");
            }

            
        }

        private void OnEnable()
        {
            if (_textComponent != null)
            {
                _textComponent.color = _originalColor;
            }

            if (loopSequentially && Palette.Length > 0)
            {
                _colorIndex = Random.Range(0, Palette.Length);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_textComponent == null || Palette.Length == 0)
            {
                return;
            }

            Color newColor;
            if (loopSequentially)
            {
                newColor = Palette[_colorIndex % Palette.Length];
                _colorIndex = (_colorIndex + 1) % Palette.Length;
            }
            else
            {
                int safety = 0;
                newColor = Palette[Random.Range(0, Palette.Length)];
                while (ApproximatelyEqual(newColor, _textComponent.color) && safety < 10)
                {
                    newColor = Palette[Random.Range(0, Palette.Length)];
                    safety++;
                }
            }

            newColor.a = _originalColor.a;
            _textComponent.color = newColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_textComponent != null)
            {
                _textComponent.color = _originalColor;
            }
        }

        private static bool ApproximatelyEqual(Color a, Color b)
        {
            const float threshold = 0.01f;
            return Mathf.Abs(a.r - b.r) < threshold &&
                   Mathf.Abs(a.g - b.g) < threshold &&
                   Mathf.Abs(a.b - b.b) < threshold &&
                   Mathf.Abs(a.a - b.a) < threshold;
        }
    }
}
