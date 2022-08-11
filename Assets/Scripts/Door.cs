using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Door : MonoBehaviour
{
    public string keyCode = "";

    [SerializeField] private Sprite[] spriteSheet;
    [SerializeField] private AudioClip doorOpening;
    [SerializeField] private AudioClip doorClosing;

    private bool isClosed = true;
    private SpriteRenderer renderer;
    private BoxCollider2D boxCollider;
    private AudioSource audio;

    private void Awake()
    {
        audio = GetComponentInChildren<AudioSource>();
        renderer = gameObject.GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        gameObject.layer = 3;
    }

    [ContextMenu("Interact")]
    public void Interact()
    {
        if (!isClosed && ObstacleInDoorArea()) return;
        isClosed = !isClosed;
        ApplyState();
    }

    private void ApplyState()
    {
        audio.Stop();
        if (isClosed)
        {
            audio.PlayOneShot(doorClosing);
            renderer.sprite = spriteSheet[1];
            boxCollider.size = new Vector2(boxCollider.size.x, boxCollider.size.y + 0.05f);
            gameObject.layer = 3;   //ground
        }
        else
        {
            audio.PlayOneShot(doorOpening);
            renderer.sprite = spriteSheet[0];
            boxCollider.size = new Vector2(boxCollider.size.x, boxCollider.size.y - 0.05f);
            gameObject.layer = 0;   //default
        }
        boxCollider.isTrigger = !isClosed;
    }

    private bool ObstacleInDoorArea()
    {
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        boxCollider.OverlapCollider(filter, results);
        return results.Any(col => !col.isTrigger);
    }
}
