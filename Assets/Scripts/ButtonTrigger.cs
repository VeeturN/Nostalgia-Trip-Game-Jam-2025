using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Co ma otwierać?")]
    public GameObject doorObject; 

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
            Debug.Log("Player on pressure plate");
            if (doorObject != null) doorObject.SetActive(false);
            if (pressedSprite != null) mySprite.sprite = pressedSprite;
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (doorObject != null) doorObject.SetActive(true);
            mySprite.sprite = defaultSprite;
        }
    }
}