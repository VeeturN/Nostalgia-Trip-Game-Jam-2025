using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    [Header("Co ma otwierać?")]
    [SerializeField] private bool _OneTime; 
    [SerializeField] private GameObject doorObject; 
    [SerializeField] private bool Reverse;

    [Header("Wygląd przycisku")]
    public Sprite pressedSprite;  
    
    private Sprite defaultSprite; 
    private SpriteRenderer mySprite;

    void Start()
    {
        mySprite = GetComponent<SpriteRenderer>();
        defaultSprite = mySprite.sprite;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Player2"))
        {
            if (_OneTime)
            {
                if (Reverse)
                {
                    Debug.Log("Player on pressure plate");
                    doorObject.SetActive(false);
                    mySprite.sprite = pressedSprite;
                }
                else
                {
                    Debug.Log("Player on pressure plate");
                    doorObject.SetActive(true);
                    mySprite.sprite = pressedSprite;
                }

                
            }
            else
            {
                if (Reverse)
                {
                    Debug.Log("Player on pressure plate");
                    doorObject.SetActive(false);
                    mySprite.sprite = pressedSprite;
                }
                else
                {
                    Debug.Log("Player on pressure plate");
                    doorObject.SetActive(true);
                    mySprite.sprite = pressedSprite;
                }
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")  || other.CompareTag("Player2"))
        {
            if (_OneTime)
            {
                
            }
            else
            {
                if (Reverse)
                {
                    doorObject.SetActive(true);
                    mySprite.sprite = defaultSprite;
                }
                else
                {
                    doorObject.SetActive(false);
                    mySprite.sprite = defaultSprite;  
                }
                
            }
        }
    }
}