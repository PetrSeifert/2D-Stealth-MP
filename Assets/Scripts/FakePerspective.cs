using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakePerspective : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float multiplier = 0;
    Transform transform;
    void Start()
    {
        transform = GetComponent<Transform>();
    }
    void Update()
    {
        transform.position = new Vector3(player.position.x * multiplier, player.position.y * multiplier, transform.position.z);
    }
}
