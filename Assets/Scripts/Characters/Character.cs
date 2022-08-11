using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;

public class Character : MonoBehaviour, IDamageable
{
    public event EventHandler OnHealthChanged;
    public bool isDead;

    [HideInInspector] public bool isOnLadder;
    [HideInInspector] public bool gotHitThisFrame;

    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip meleeAttackClip;
    [SerializeField] private AudioClip rangedAttackClip;
    [SerializeField] private Tilemap ladderTilemap;
    [SerializeField] private GridLayout gridLayout;

    [SerializeField] private bool dropItem;
    [SerializeField] private GameObject itemToDrop;
    [SerializeField] private int meleeDamage;
    [SerializeField] private int maxHealths;
    [SerializeField] private bool rangedWeapon;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float climbSpeed;
    [SerializeField] private float stepOffset;
    [SerializeField] private float stepSmooth;
    [SerializeField] private float meleeAttackRange;
    [SerializeField] private float rangedAttackRange;
    [SerializeField] private float viewDistance;

    [SerializeField] private BoxCollider2D physicsCollider;
    [SerializeField] private Transform stepRayUpper;
    [SerializeField] private Transform stepRayLower;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private PauseMenuController pauseMenuController;


    [SerializeField] private AudioSource footstepsSource;
    [SerializeField] private AudioSource otherSource;
    private Animator animator;
    private Rigidbody2D characterRigidbody;
    private Vector3 attackPoint;
    private Vector3 shotDirection;
    private LayerMask defaultMask;
    private LayerMask groundMask;
    private LayerMask ladderMask;
    private int playerLayer;
    private int targetLayer;
    private float walkDirection;
    private float climbDirection;
    private int currentHealths;
    private bool isAttacking;
    private Vector2 lookingDirection = new Vector2(1, 0);
    private string keyCode;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterRigidbody = GetComponent<Rigidbody2D>();
        playerLayer = LayerMask.NameToLayer("Player");
        targetLayer = LayerMask.NameToLayer(gameObject.layer == playerLayer ? "Enemy" : "Player");
        defaultMask = LayerMask.GetMask(LayerMask.LayerToName(targetLayer), "Ground");
        groundMask = LayerMask.GetMask("Ground", "LadderPlatform");
        ladderMask = LayerMask.GetMask("Ladder");
        currentHealths = maxHealths;
    }

    private void Update()
    {
        if (isDead) return;
        gotHitThisFrame = false;
        if (walkDirection != 0 && IsGrounded(0.6f) && !footstepsSource.isPlaying)
        {
            footstepsSource.Play();
        }
        else if (walkDirection == 0 || !IsGrounded(0.6f))
        {
            footstepsSource.Stop();
        }
    }

    private void FixedUpdate()
    {
        if (isDead || isAttacking) return;
        HandleMovement();
        animator.SetBool("isWalking", characterRigidbody.velocity.x != 0);
        animator.SetBool("isJumping", !IsGrounded(0.6f));
        animator.SetBool("isClimbing", climbDirection != 0);
    }

    private void HandleMovement()
    {
        Move();
        Climb();
        StepClimb();
    }

    private void Move()
    {
        if (isOnLadder) return;
        if (walkDirection < 0 && transform.localScale.x > 0 || walkDirection > 0 && transform.localScale.x < 0) FlipPlayer();

        if (walkDirection != 1 && walkDirection != -1 && IsGrounded(0.2f))
            characterRigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        else
            characterRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (IsGrounded(0.6f))
            characterRigidbody.gravityScale = 4;
        else
            characterRigidbody.gravityScale = 2;

        characterRigidbody.velocity = new Vector2(walkDirection * walkSpeed, characterRigidbody.velocity.y);
    }

    private void Climb()
    {
        TileBase ladderTile = GetLadder();
        if (ladderTile != null)
        {
            if (climbDirection != 0 && !isOnLadder)
            {
                if (!IsGrounded(0.2f) || climbDirection != -1)
                {
                    isOnLadder = true;
                    animator.SetBool("isOnLadder", true);
                    characterRigidbody.gravityScale = 0;
                    transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), transform.position.y, transform.position.z);
                }
            }

            if (isOnLadder)
            {
                if (climbDirection == 0)
                    animator.enabled = false;
                else
                    animator.enabled = true;
                characterRigidbody.velocity = new Vector2(0, climbDirection * climbSpeed);
            }
            if (IsGrounded(0.2f))
            {
                isOnLadder = false;
                animator.enabled = true;
                animator.SetBool("isOnLadder", false);
            }
        }
        else
        {
            isOnLadder = false;
            animator.enabled = true;
            animator.SetBool("isOnLadder", false);
        }

        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - transform.localScale.y - 0.1f), Vector2.down, 0.1f, groundMask);
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y - transform.localScale.y - 0.1f), Vector2.down * 0.1f, Color.red, 1f);
        if (hit && climbDirection == -1)
            hit.collider.gameObject.GetComponent<OneWayPlatform>()?.LetCharacterFallThrough(gameObject);
    }

    public void SetMoveDirection(float direction)
    {
        walkDirection = direction;
    }

    private void FlipPlayer()
    {
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        lookingDirection = -lookingDirection;
    }

    public void SetClimbDirection(float direction)
    {
        climbDirection = direction;
    }

    private void StepClimb()
    {
        Vector3 facingDirection = new Vector3(transform.localScale.x, 0, 0);
        stepRayUpper.position = new Vector3(stepRayLower.position.x, stepRayLower.position.y + stepOffset, stepRayLower.position.z);
        if (walkDirection == 0) return;
        RaycastHit2D hit = Physics2D.Raycast(stepRayLower.position, facingDirection, 0.1f, groundMask);
        Debug.DrawRay(stepRayLower.position, facingDirection * 0.1f, Color.red, 1);
        if (!hit) return;
        hit = Physics2D.Raycast(stepRayUpper.position, facingDirection, 0.1f, groundMask);
        Debug.DrawRay(stepRayUpper.position, facingDirection * 0.1f, Color.green, 1);
        if (!hit)
            transform.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
    }

    public void Jump()
    {
        if (IsGrounded(0.2f) && !isDead && !isAttacking)
            characterRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public bool IsGrounded(float distance)
    {
        Vector2 direction = Vector2.down;

        RaycastHit2D hit = Physics2D.BoxCast(new Vector3(physicsCollider.bounds.center.x, physicsCollider.bounds.center.y - physicsCollider.bounds.size.y / 2 + 0.1f), new Vector3(physicsCollider.bounds.size.x - 0.1f, 0.1f), 0f, direction, distance, groundMask);

        return hit.collider != null;
    }

    private TileBase GetLadder()
    { 
        TileBase tile1 = ladderTilemap.GetTile(gridLayout.WorldToCell(new Vector3(Mathf.FloorToInt(transform.position.x - 0.5f), transform.position.y)));
        TileBase tile2 = ladderTilemap.GetTile(gridLayout.WorldToCell(new Vector3(Mathf.CeilToInt(transform.position.x - 0.5f), transform.position.y)));
        if (tile1 == null || tile2 == null)
        {
            tile1 = ladderTilemap.GetTile(gridLayout.WorldToCell(new Vector3(Mathf.FloorToInt(transform.position.x - 0.5f), transform.position.y - physicsCollider.bounds.size.y / 2 + physicsCollider.offset.y)));
            tile2 = ladderTilemap.GetTile(gridLayout.WorldToCell(new Vector3(Mathf.CeilToInt(transform.position.x - 0.5f), transform.position.y - physicsCollider.bounds.size.y / 2 + physicsCollider.offset.y)));
        }
        if (tile1 == null || tile2 == null)
            return null;
        return tile1;
    }

    public void SwitchWeapon()
    {
        rangedWeapon = !rangedWeapon;
    }

    public void Attack(Vector3 targetPosition)
    {
        if (isAttacking || !IsGrounded(0.2f) || isOnLadder || isDead) return;
        isAttacking = true;
        characterRigidbody.velocity = new Vector2(0, 0);
        attackPoint = GetAttackPoint();
        if (rangedWeapon) StartRangedAttack(targetPosition);
        else StartMeleeAttack();
    }

    private void StartMeleeAttack()
    {
        otherSource.PlayOneShot(meleeAttackClip);
        Collider2D other = Physics2D.OverlapCircle(attackPoint, meleeAttackRange, defaultMask);
        if (!other)
        {
            Debug.Log("No hit");
            animator.SetTrigger("AttackMelee");
            return;
        }
        if (other.gameObject.layer != targetLayer)
        {
            animator.SetTrigger("AttackMelee");
            return;
        }
        if (transform.localScale.x == other.transform.localScale.x && gameObject.layer == playerLayer)
            animator.SetTrigger("AttackBackstab");
        else 
            animator.SetTrigger("AttackMelee");
    }

    void StartRangedAttack(Vector3 targetPosition)
    {
        otherSource.PlayOneShot(rangedAttackClip);
        animator.SetTrigger("AttackRanged");
        if (transform.localScale.x == 1 && targetPosition.x < attackPoint.x) targetPosition.x = attackPoint.x;
        else if (transform.localScale.x == -1 && targetPosition.x > attackPoint.x) targetPosition.x = attackPoint.x;
        shotDirection = CalculateShotDirection(targetPosition);
    }

    public void FinishAttack(bool rangedAttack)
    {
        isAttacking = false;
        if (!isDead)
        {
            if (rangedAttack)
            {
                ShootArrow(shotDirection);
            }
            else
            {
                Collider2D other = Physics2D.OverlapCircle(attackPoint, meleeAttackRange, defaultMask);
                if (!other) return;
                if (other.gameObject.layer != targetLayer) return;
                IDamageable damageable = other?.GetComponent<IDamageable>();
                int damage = meleeDamage;
                if (transform.localScale.x == other.transform.localScale.x && gameObject.layer == playerLayer)
                    damage = 9999;
                damageable.TakeDamage(damage);
            }
        }
    }

    private void ShootArrow(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject arrow = Instantiate(arrowPrefab, attackPoint, Quaternion.AngleAxis(angle, Vector3.forward));
        arrow.GetComponent<Arrow>().shooter = gameObject;
    }

    private Vector3 CalculateShotDirection(Vector3 targetPosition)
    {
        Vector3 heading = targetPosition - attackPoint;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;
        return direction;
    }

    private Transform GetClosestTarget(Collider2D[] targets)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (Collider2D potentialTarget in targets)
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (!(dSqrToTarget < closestDistanceSqr)) continue;
            closestDistanceSqr = dSqrToTarget;
            bestTarget = potentialTarget.transform;
        }

        return bestTarget;
    }

    public void TakeDamage(int damage)
    {
        gotHitThisFrame = true;
        currentHealths -= damage;
        if (currentHealths <= 0)
        {
            currentHealths = 0;
            Die();
        }
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Die()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Player"))
            pauseMenuController.ShowDeathScreen();
        
        if (dropItem)
        {
            GameObject gameObject = Instantiate(itemToDrop, new Vector3(transform.position.x, Mathf.RoundToInt((transform.position.y - 1) * 2) / 2), Quaternion.identity);
            Key key = gameObject.GetComponent<Key>();
            if (key != null) key.keyCode = "1";
        }

        if (Physics2D.Raycast(transform.position, lookingDirection, 2f, groundMask)) FlipPlayer();
        isDead = true;
        animator.SetBool("isDead", true);
        characterRigidbody.velocity = new Vector3(0, 0);
        characterRigidbody.gravityScale = 2;
        animator.enabled = true;
        if (GetComponent<EnemyAI>() != null)
            Destroy(GetComponent<EnemyAI>());
        if (GetComponent<EnemyHider>() != null)
            Destroy(GetComponent<EnemyHider>());
    }

    public void Heal(int value)
    {
        currentHealths += value;
        if (currentHealths > maxHealths) currentHealths = maxHealths;
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthPercent() => (float)currentHealths / maxHealths;

    public void RestoreHealths()
    {
        currentHealths = maxHealths;
    }

    public void Interact()
    {
        Door door = GetDoors();
        if (door == null) return;
        Debug.Log($"{door.keyCode}, {keyCode}, {door.keyCode.Equals(keyCode)}");
        if (door.keyCode == "" || door.keyCode.Equals(keyCode))//nic nebo klíč z inventáře
            door.Interact();
        //else if(x != null) x.Interact();
    }

    private Door GetDoors()
    {
        Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        return (from other in results where other.CompareTag("Doors") select other.GetComponent<Door>()).FirstOrDefault();
    }

    private void OnDrawGizmosSelected() 
    {
        int lookDirection;
        if (transform.localScale.x > 0)
            lookDirection = 1;
        else
            lookDirection = -1;
        Gizmos.DrawSphere(new Vector3(transform.position.x + physicsCollider.size.x / 2 * lookDirection, transform.position.y), meleeAttackRange);
    }

    public Vector3 GetAttackPoint()
    {
        int lookDirection;
        if (transform.localScale.x > 0)
            lookDirection = 1;
        else
            lookDirection = -1;
        return new Vector3(transform.position.x + physicsCollider.size.x / 2 * lookDirection, transform.position.y);
    } 

    public bool IsRangedWeaponActive() => rangedWeapon;

    public float GetWalkDirection() => walkDirection;

    // Methods for AI
    public bool CanHitPlayer(Vector3 directionToTarget)
    {
        if (!IsGrounded(0.2f) || isOnLadder || isDead) return false;
        attackPoint = GetAttackPoint();
        if (rangedWeapon)
        {
            RaycastHit2D hit = Physics2D.Raycast(attackPoint, directionToTarget, rangedAttackRange, defaultMask);
            if (hit) return targetLayer == hit.collider.gameObject.layer;
            return false;
        }

        Collider2D other = Physics2D.OverlapCircle(attackPoint, meleeAttackRange, defaultMask);
        if (other) return other.gameObject.layer == targetLayer;
        return false;
    }

    public bool CanSeePlayer(Vector3 directionToTarget)
    {
        if (!isOnLadder)
            if (transform.localScale.x < 0 && directionToTarget.x > 0 ||
                transform.localScale.x > 0 && directionToTarget.x < 0)
                return false;
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, viewDistance, defaultMask);
        if (hit) return targetLayer == hit.collider.gameObject.layer;
        return false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameObject.layer != playerLayer) return;
        Key key = other.GetComponent<Key>();
        if (key != null)
        {
            keyCode = key.keyCode;
            key.Collect();
        } 
    }
}
