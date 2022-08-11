using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public enum State
{
    Idle, Move, Attack
}

public class EnemyAI : MonoBehaviour
{
    public List<Vector2> actions;
    
    [SerializeField] private Transform player;
    [SerializeField] private Transform point1Transform;
    [SerializeField] private Transform point2Transform;
    [SerializeField] private float idleDuration;

    private ActionTimer idleTimer;
    private Seeker seeker;
    private Character character;
    private Vector2 currentTarget;
    private Vector2 currentDirection;
    private Vector2 directionToTarget;
    private Vector3 point1;
    private Vector3 point2;
    private State state;

    private readonly Vector2 climbLadderRight = new Vector2(0.8320503f, 0.5547003f);
    private readonly Vector2 climbLadderLeft = new Vector2(-0.8320503f, 0.5547003f);
    private readonly Vector2 goDownLadderRight = new Vector2(0.8320503f, -0.5547003f);
    private readonly Vector2 goDownLadderLeft = new Vector2(-0.8320503f, -0.5547003f);
    private readonly Vector2 climbStairsRight = new Vector2(0.7071068f, 0.7071068f);
    private readonly Vector2 climbStairsLeft = new Vector2(-0.7071068f, 0.7071068f);
    private readonly Vector2 nullVector2 = new Vector2(-9999, -9999);
    
    private void Awake()
    {
        actions = new List<Vector2>();
        character = GetComponent<Character>();
        seeker = GetComponent<Seeker>();
        currentTarget = nullVector2;
        point1= point1Transform.position;
        point2 = point2Transform.position;
        state = State.Idle;
        idleTimer = new ActionTimer(StartWandering, idleDuration);
    }
    
    private void Update()
    {
        if (!player.GetComponent<Character>().isDead)
        {
            directionToTarget = (player.position - transform.position).normalized;
            idleTimer?.Tick(Time.deltaTime);

            if (character.CanSeePlayer(directionToTarget) || character.gotHitThisFrame)
            {
                StartCoroutine(FindPath(transform.position, player.transform.position));

                if (character.CanHitPlayer(directionToTarget))
                {
                    state = State.Attack;
                    character.SetMoveDirection(0);
                    character.Attack(player.position);
                    return;
                }
            }
        }

        if (state == State.Idle) return;

        HandleMovement();
    }

    private IEnumerator FindPath(Vector2 start, Vector2 end)
    {
        Path path = seeker.StartPath(start, end, OnPathComplete);
        yield return StartCoroutine(path.WaitForPath());
    }

    private void OnPathComplete(Path path)
    {
        if (path.error)
            Debug.Log("No path found. " + path.errorLog);
        else
        {
            actions.Clear();
            List<Vector3> pathVectors3 = path.vectorPath;
            foreach (var vector in pathVectors3)
                actions.Add(vector);
            NextTarget();
            state = State.Move;
        }
    }

    private void StartWandering()
    {
        Vector2 endPoint = Vector2.Distance(point1, transform.position) > Vector2.Distance(point2, transform.position) ? point1 : point2;
        StartCoroutine(FindPath(transform.position, endPoint));
    }

    private void HandleMovement()
    {
        if ((character.isOnLadder && currentDirection == Vector2.right) || 
        (character.isOnLadder && currentDirection == Vector2.left))
        {
            if (transform.position.y < currentTarget.y)
                character.SetClimbDirection(1);
            else
                character.SetClimbDirection(-1);
            return;
        }
        
        if (currentDirection == Vector2.right || currentDirection == climbStairsRight)
        {
            if (currentTarget.x > transform.position.x + 0.3f)
            {
                character.SetMoveDirection(1);
                character.SetClimbDirection(0);
            }
            else
                NextTarget();
        }
        else if (currentDirection == Vector2.left || currentDirection == climbStairsLeft)
        {
            if (currentTarget.x + 0.3f < transform.position.x)
            {
                character.SetMoveDirection(-1);
                character.SetClimbDirection(0);
            }
            else
                NextTarget();
        }
        else if (currentDirection == Vector2.up)
        {
            character.SetMoveDirection(0);
            character.SetClimbDirection(1);
            if (currentTarget.y <= transform.position.y)
                NextTarget();
        }
        else if (currentDirection == Vector2.down)
        {
            character.SetMoveDirection(0);
            character.SetClimbDirection(-1);
            if (currentTarget.y >= transform.position.y - 2)
                if (character.IsGrounded(0.2f))
                    NextTarget();
        }
        else if (currentDirection == climbLadderRight)
        {
            character.SetMoveDirection(1);
            if (currentTarget.x <= transform.position.x)
                NextTarget();
        }
        else if (currentDirection == climbLadderLeft)
        {
            character.SetMoveDirection(-1);
            if (currentTarget.x >= transform.position.x)
                NextTarget();
        }
        else if (currentDirection == goDownLadderLeft)
        {
            character.SetMoveDirection(-1);
            character.SetClimbDirection(-1); 
            if (currentTarget.x >= transform.position.x)
                NextTarget();
        }
        else if (currentDirection == goDownLadderRight)
        {
            character.SetMoveDirection(1);
            character.SetClimbDirection(-1);
            if (currentTarget.x <= transform.position.x)
                NextTarget();
        }
        else if (currentDirection.x > 0)
        {
            character.SetMoveDirection(1);
        }
        else
        {
            NextTarget();
        }
    }

    private void NextTarget()
    {
        if (actions.Count < 2)
        {
            character.SetMoveDirection(0);
            state = State.Idle;
            idleTimer = new ActionTimer(StartWandering, idleDuration);
            return;
        }
        currentTarget = actions[1];
        currentDirection = (currentTarget - actions[0]).normalized;
        actions.RemoveAt(0);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Doors"))
            character.Interact();
    }
}
