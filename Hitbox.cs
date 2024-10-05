using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Hitbox : NetworkBehaviour
{
    public NetworkObject networkObject;
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D boxCollider;
    public Animator animator;

    public string projectileType;
    public float damage, damageIncreasePerSecond;
    public Player source;
    public bool followsSource, hit, piercing;
    public Vector3 offsetFromSource;
    public Vector3 movement;

    void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (hit && !piercing) {Destroy(gameObject);}
        if (damageIncreasePerSecond > 0) {damage = Mathf.Ceil(damage + (damageIncreasePerSecond * Time.deltaTime));}

        if (followsSource) 
        {
            gameObject.transform.position = source.transform.position + offsetFromSource;
        }
        else if (movement != null)
        {
            gameObject.transform.position += movement;
        }
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
