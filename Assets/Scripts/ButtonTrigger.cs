using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    [Header("Co ma otwierać?")]
    [SerializeField] private bool _OneTime; 
    [SerializeField] private GameObject doorObject; 

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
        if (other.CompareTag("Player"))
        {
            if (_OneTime)
            {
                Debug.Log("Player on pressure plate");
                doorObject.SetActive(false);
                mySprite.sprite = pressedSprite;
            }
            else
            {
                Debug.Log("Player on pressure plate");
                doorObject.SetActive(false);
                mySprite.sprite = pressedSprite;
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_OneTime)
            {
                
            }
            else
            {
                doorObject.SetActive(true);
                mySprite.sprite = defaultSprite;
            }
        }
    }
}