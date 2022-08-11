using UnityEngine;

public class Win : MonoBehaviour
{
    [SerializeField] private GameObject winScreen;
    
    private int playerLayer;

    void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
    }

    void OnTriggerEnter2D(Collider2D collidier)
    {
        if (collidier.gameObject.layer == playerLayer)
        {
            winScreen.SetActive(true);
            //TODO: Play win sound
        }
    }
}
