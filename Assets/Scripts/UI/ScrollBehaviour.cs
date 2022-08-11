using UnityEngine;
using UnityEngine.UI;

public class ScrollBehaviour : MonoBehaviour
{
    private Image image;
    [SerializeField] private Sprite defalutScrollSprite;

    void Awake(){
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(image.sprite.name);
        if (image.sprite.name == "Scroll1_5")
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    void OnEnable()
    {
        image.sprite = defalutScrollSprite;
        for (int i = 0; i < transform.childCount; i++)      
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
