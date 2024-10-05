using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpriteHandler : MonoBehaviour
{
    public RuntimeAnimatorController knight, rogue, sorcerer, ranger;
    public Animator animator;

    private Player player;
    private SpriteRenderer spriteRenderer;
    private NetworkPlayerManager networkPlayerManager;
    private Vector3 lastPosition;
    private float movement;
    private float moveMinimum = 0.05f; 

    void Awake()
    {
        player = GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        networkPlayerManager = GameObject.FindObjectOfType<NetworkPlayerManager>();

        OnCharacterChanged(Character.Knight);
    }

    void FixedUpdate()
    {
        movement = transform.position.x - lastPosition.x;
        animator.SetBool("Moving", Mathf.Abs(movement) > moveMinimum);

        if (player.airborne)
        {
            animator.SetBool("Airborne", true);
            animator.SetBool("CanWalk", false);
        }
        else
        {
            animator.SetBool("Airborne", false);
        }

        animator.SetBool("Falling", transform.position.y < lastPosition.y);
        animator.SetBool("InLobby", player.inLobby);

        if (player.canFlipSprite) 
        {
            spriteRenderer.flipX = (movement < 0) ? true : (movement > 0) ? false : spriteRenderer.flipX;
        }

        lastPosition = transform.position;
    }

    public void OnMovementStart()
    {
        animator.SetBool("CanWalk", true);
    }

    public void OnCharacterChanged(Character character)
    {
        gameObject.transform.localScale = new Vector3(5.0f, 5.0f, 1.0f);

        switch(character)
        {
            case Character.Knight:
                animator.runtimeAnimatorController = knight;
                break;
            case Character.Rogue:
                animator.runtimeAnimatorController = rogue;
                break;
            case Character.Sorcerer:
                animator.runtimeAnimatorController = sorcerer;
                gameObject.transform.localScale = new Vector3(5.5f, 5.5f, 1.0f);
                break;
            case Character.Ranger:
                animator.runtimeAnimatorController = ranger;
                break;
        }
    }

    void SetTrigger(string trigger)
    {
        if (!networkPlayerManager.IsClient)
        {
            animator.SetTrigger(trigger);
        }
        else
        {
            networkPlayerManager.UpdateSprite(trigger, this.gameObject);
        }
    }

    void OnAbility1Used()
    {
        SetTrigger("OnAbility1");
    }

    void OnAbility2Used()
    {
        SetTrigger("OnAbility2");
    }

    void OnHit()
    {
        SetTrigger("OnHit");
    }

    void SetDeathTrigger()
    {
        animator.SetTrigger("OnDeath");
    }
}
