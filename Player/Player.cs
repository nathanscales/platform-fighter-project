using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public enum InputType
{
    Keyboard,
    Gamepad,
    Network,
    AI
}

public enum Character
{
    None,
    Knight,
    Rogue,
    Sorcerer,
    Ranger
}

public class Player : MonoBehaviour
{
    public PlayerSpriteHandler spriteHandler;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public InputType inputType;
    public Character character;
    public bool inLobby, dead, immune, canJump, canUseAbilities, canFlipSprite;
    public int maxJumps, jumps;
    public float maxHealth, health, maxSpeed, speed, moveDirection;

    public bool airborne {
        get {
            return !Physics2D.Raycast(transform.position, -transform.up, 0.05f);
        }
    }

    private PlayerInputHandler inputHandler;
    private NetworkPlayerManager networkPlayerManager;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        spriteHandler = GetComponent<PlayerSpriteHandler>();
        inputHandler = GetComponent<PlayerInputHandler>();
        networkPlayerManager = GameObject.FindObjectOfType<NetworkPlayerManager>();

        inLobby = true;
        canJump = true;
        canUseAbilities = true;
        canFlipSprite = true;
        character = Character.Knight;
        health = maxHealth;
        speed = maxSpeed;
        jumps = maxJumps;

        if (inLobby) {GameObject.FindObjectOfType<LobbyManager>().SendMessage("UpdateLobby");}
    }

    void FixedUpdate()
    {
        if (inLobby == true && SceneManager.GetActiveScene().name != "Lobby") {inLobby = false;}
        if (health <= 0 && !dead) {OnDeath();}
    }

    public void OnCharacterChanged()
    {
        switch(character)
        {
            case Character.Knight:
                character = Character.Rogue;
                break;
            case Character.Rogue:
                character = Character.Sorcerer;
                break;
            case Character.Sorcerer:
                character = Character.Ranger;
                break;
            case Character.Ranger:
                character = Character.Knight;
                break;
        }

        spriteHandler.OnCharacterChanged(character);
        if (inLobby) {GameObject.FindObjectOfType<LobbyManager>().SendMessage("UpdateLobby");}
        if(gameObject.tag != "NetworkPlayer") {networkPlayerManager.SendMessage("OnCharacterChanged", ArrayUtility.IndexOf(Game.players, this.gameObject));}
    }

    public void OnGameStart()
    {
        if(inputHandler) {inputHandler.OnGameStart(character);}
    }

    public void AllowSpriteFlipOnLand()
    {
        while (airborne) {}
        Debug.Log("landed");
        canFlipSprite = true;
    }

    void OnCollisionEnter2D(Collision2D collision) 
    {
        if (!airborne) 
        {
            StartCoroutine("StopMovement", 0.2f);
            jumps = maxJumps;
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Hitbox")
        {
            Hitbox hitbox = collider.gameObject.GetComponent<Hitbox>();

            if(!dead && !immune && hitbox && hitbox.source != this)
            {
                spriteHandler.SendMessage("OnHit");
                health -= hitbox.damage;
                if (hitbox.movement != null) {hitbox.hit = true;}

                if(networkPlayerManager.IsClient)
                {
                    networkPlayerManager.UpdateHealth(ArrayUtility.IndexOf(Game.players, this.gameObject), health);
                }
            }
        } 
    }

    IEnumerator StopMovement(float seconds)
    {
        rb.velocity = Vector3.zero;
        speed = 0;
        rb.gravityScale = 0;

        yield return new WaitForSeconds(seconds);

        speed = maxSpeed;
        rb.gravityScale = 1;
        spriteHandler.SendMessage("OnMovementStart");
    }

    void OnBecameInvisible()
    {
        if (this.transform.position.y < 0)
        {
            health = 0;
        }
    }

    void OnDeath()
    {
        dead = true;
        if(inputHandler) {inputHandler.playerInput.DeactivateInput();}
        GameObject.FindObjectOfType<GameManager>().SendMessage("OnPlayerDeath");
        spriteHandler.SendMessage("SetDeathTrigger");
    }

    void OnDestroy()
    {
        int playerNum = ArrayUtility.IndexOf(Game.players, this.gameObject);
        if(playerNum != -1) {Game.players[playerNum] = null;}
        if(gameObject.tag != "NetworkPlayer" && networkPlayerManager) {networkPlayerManager.SendMessage("RemovePlayer", playerNum);}  
        if(inLobby) {GameObject.FindObjectOfType<LobbyManager>().SendMessage("UpdateLobby");}    
    }
}
