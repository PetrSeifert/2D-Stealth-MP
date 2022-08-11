using UnityEngine;

public class EnemyHider : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer barRenderer;
    [SerializeField] private SpriteRenderer backgroundBarRenderer;

    private Vector3 heading1;
    private Vector3 heading2;
    private Vector3 direction1;
    private Vector3 direction2;
    private Vector3 origin;
    private Vector3 endPoint1;
    private Vector3 endPoint2;
    private float distance1;
    private float distance2;
    private LayerMask groundMask;
    private SpriteRenderer characterRenderer;

    private void Awake()
    {
        groundMask = LayerMask.GetMask("Ground");
        characterRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        origin = new Vector3(player.position.x, player.position.y + 1);
        endPoint1 = new Vector3(transform.position.x, transform.position.y + 1);
        endPoint2 = new Vector3(transform.position.x, transform.position.y - 1);
        heading1 = endPoint1 - origin;
        heading2 = endPoint2 - origin;
        direction1 = heading1.normalized;
        direction2 = heading2.normalized;
        distance1 = heading1.magnitude;
        distance2 = heading2.magnitude;
        if (Physics2D.Raycast(origin, direction1, distance1, groundMask) && Physics2D.Raycast(origin, direction2, distance2, groundMask))
        {
            if (characterRenderer.enabled)
            {
                characterRenderer.enabled = false;
                barRenderer.enabled = false;
                backgroundBarRenderer.enabled = false;
            }
        }
        else
        {
            if (!characterRenderer.enabled)
            {
                characterRenderer.enabled = true;
                barRenderer.enabled = true;
                backgroundBarRenderer.enabled = true;
            } 
        }
    }
}
