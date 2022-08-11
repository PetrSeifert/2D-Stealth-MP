using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [HideInInspector] public GameObject shooter;

    [SerializeField] private List<AudioClip> arrowHits;
    [SerializeField] private float flySpeed;
    [SerializeField] private int damage;
    [SerializeField] private float lifeSpan;
    
    private Vector3 flyDirection;
    private ActionTimer actionTimer;
    private Rigidbody2D rigidbody;
    private int groundLayer;
    private int playerLayer;
    private int enemyLayer;

    private void Awake()
    {
        flyDirection = transform.right;
        actionTimer = new ActionTimer(() => { Destroy(this.gameObject); }, lifeSpan);
        rigidbody = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.NameToLayer("Ground");
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    private void Update()
    {
        actionTimer.Tick(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        rigidbody.MovePosition(transform.position + flyDirection * flySpeed * Time.deltaTime);
    }

    private void DealDamage(GameObject target)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        damageable?.TakeDamage(damage);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == shooter
            || other.gameObject.layer != playerLayer
            && other.gameObject.layer != enemyLayer
            && other.gameObject.layer != groundLayer) return;

        if (other.GetComponent<Character>() != null)
            if (other.GetComponent<Character>().isDead) return;

        int clipIndex = Random.Range(0, arrowHits.Count);
        AudioClip arrowHit = arrowHits[clipIndex];
        AudioSource.PlayClipAtPoint(arrowHit, transform.position);
        DealDamage(other.gameObject);
        Destroy(this.gameObject);
    }
}
