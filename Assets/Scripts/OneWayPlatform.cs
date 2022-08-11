using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private List<BoxCollider2D> excludedColliders;

    private void Awake()
    {
        excludedColliders = new List<BoxCollider2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void LetCharacterFallThrough(GameObject character)
    {
        BoxCollider2D characterCollider = character.GetComponent<BoxCollider2D>();
        Physics2D.IgnoreCollision(characterCollider, boxCollider);
        excludedColliders.Add(characterCollider);
    }

    private void Update()
    {
        foreach (var excludedCollider in excludedColliders.ToArray())
        {
            //Debug.Log($"{excludedCollider.transform.position.y - excludedCollider.bounds.size.y / 2 + excludedCollider.offset.y} {transform.position.y + boxCollider.bounds.size.y / 2}");
            if (excludedCollider.transform.position.y - excludedCollider.bounds.size.y / 2 + excludedCollider.offset.y >= transform.position.y + boxCollider.bounds.size.y / 2)
            {
                Physics2D.IgnoreCollision(excludedCollider, boxCollider, false);
                excludedColliders.Remove(excludedCollider);
            }
        }
    }
}
