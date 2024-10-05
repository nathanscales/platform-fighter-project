using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public PlayerInput playerInput;
    public GameObject hitbox;
    public float jumpForce;
    public Sprite rangerArrow;
    public RuntimeAnimatorController sorcererProjectile;
    public Ability ability1, ability2;

    private Player player;
    private Rigidbody2D rb;
    private PlayerSpriteHandler spriteHandler;
    private NetworkPlayerManager networkPlayerManager;

    void Awake()
    {
        player = GetComponent<Player>();
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        spriteHandler = GetComponent<PlayerSpriteHandler>();
        networkPlayerManager = GameObject.FindObjectOfType<NetworkPlayerManager>();
    }

    void FixedUpdate() 
    {
        transform.Translate(Vector2.right * player.moveDirection * player.speed * Time.deltaTime);
    }

    public void OnGameStart(Character character)
    {
        switch(character)
        {
            case Character.Knight:
                ability1 = new KnightAbility1(player);
                ability2 = new KnightAbility2(player);
                break;
            case Character.Rogue:
                ability1 = new RogueAbility1(player);
                ability2 = new RogueAbility2(player);
                break;
            case Character.Sorcerer:
                ability1 = new SorcererAbility1(player, sorcererProjectile);
                ability2 = new SorcererAbility2(player);
                break;
            case Character.Ranger:
                ability1 = new RangerAbility1(player, rangerArrow);
                ability2 = new RangerAbility2(player);
                break;
        }
    }

    void OnPause()
    {
        if (!player.inLobby) 
        {
            GameManager gameManager = GameObject.FindObjectOfType<GameManager>();
            gameManager.SendMessage((!gameManager.paused) ? "OnPause" : "OnUnpause");
        }
    }

    void OnMove(InputValue value) 
    {
        if (!player.inLobby) {
            float moveValue = value.Get<Vector2>().x;
            player.moveDirection = (moveValue < 0) ? -1 : (moveValue > 0) ? 1 : 0;
        }
    }

    void OnJump() 
    {
        if (!player.inLobby && player.canJump && player.jumps >= 1) 
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            player.jumps--;
        }
    }

    void OnAbility1()
    {
        if(player.inLobby)
        {
            GetComponent<Player>().OnCharacterChanged();
        }
        else
        {
            StartCoroutine(UseAbility(ability1, "Ability1"));
        }
    }

    void OnAbility2()
    {
        if(player.inLobby)
        {
            Destroy(this.gameObject);
        }
        else
        {
            StartCoroutine(UseAbility(ability2, "Ability2"));
        }
    }

    IEnumerator UseAbility(Ability ability, string message)
    {
        if (player.canUseAbilities && (ability.remainingCooldown == 0) && (!player.airborne || ability.usableWhileAirborne))
        {
            Debug.Log(message + " used");
            
            float windup = (player.airborne) ? ability.airborneWindup : ability.windup;
            if (!ability.usableWhileAirborne) {player.canJump = false;}
            if (ability.preventsSpriteFlip) {player.canFlipSprite = false;}
            player.canUseAbilities = false;

            // puts ability on cooldown
            StartCoroutine("CooldownTimer", ability);

            // stop player and prevent user input
            if (ability.preventsMovement)
            {
                player.StartCoroutine("StopMovement", windup);
                playerInput.DeactivateInput();
            }

            // update player sprite
            spriteHandler.SendMessage("On"+message+"Used");

            yield return new WaitForSeconds(windup);

            if (ability.setImmune) {player.immune = true;}
            if (ability.modSpeed > 0.0f) {player.speed = player.maxSpeed + ability.modSpeed;}

            // instantiate hitbox for ability
            Hitbox hit = (Instantiate(hitbox).GetComponent<Hitbox>());
            hit.source = player;

            // ability is used
            ability.OnUse(hit);

            if(networkPlayerManager.IsClient && hit.projectileType != null) {
                networkPlayerManager.SpawnProjectile(hit.projectileType, ability.direction, player.transform.position);
            }

            yield return new WaitForSeconds(ability.duration);

            // allow user input again
            playerInput.ActivateInput();

            player.speed = player.maxSpeed;
            player.immune = false;
            player.canJump = true;
            player.canUseAbilities = true;
            player.canFlipSprite = true;
            if (!ability.hitboxPersists) {hit.hit = true;}
        }
    }

    IEnumerator CooldownTimer(Ability ability)
    {
        ability.remainingCooldown = ability.cooldown;
        while (ability.remainingCooldown > 0)
        {
            yield return new WaitForSeconds(1.0f);
            ability.remainingCooldown--;
        }
    }
}