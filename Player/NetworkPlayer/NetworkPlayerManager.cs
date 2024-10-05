using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEditor;

public class NetworkPlayerManager : NetworkBehaviour
{
    public GameObject networkPlayer, networkHitbox;
    public Sprite arrow;
    public RuntimeAnimatorController frostbolt;
    public bool approved, startingHost;

    private bool sendingPositions, sentFromThisClient;
    private ulong localID;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        sendingPositions = false;
    }

    void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;        
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        approved = (SceneManager.GetActiveScene().name == "Lobby");
        response.Approved = approved;
    }

    /*
     * Requests NetworkPlayers from Host when client connects
     */
    void OnClientConnected(ulong clientID)
    {
        approved = false;
        this.localID = clientID;

        if(!IsServer)
        {
            StartCoroutine("LoadHostLobby", clientID);
        }
    }

    IEnumerator LoadHostLobby(ulong clientID)
    {
        GameObject.FindObjectOfType<LobbyManager>().ResetLobby();
        yield return new WaitForSecondsRealtime(1.0f);
        GetHostLobbyServerRpc(clientID);
    }

    [ServerRpc(RequireOwnership=false)]
    private void GetHostLobbyServerRpc(ulong clientID)
    {
        ulong[] target = new ulong[1]{clientID};
        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = target;

        Character[] isPlayer = new Character[Game.maxPlayers];

        Player player;
        for(int i=0; i<Game.maxPlayers; i++)
        {
            if(Game.players[i])
            {
                player = Game.players[i].GetComponent<Player>();
                isPlayer[i] = player.character;
            }
            else
            {
                isPlayer[i] = Character.None;
            }
        }

        SendLobbyClientRpc(isPlayer, Game.selectedMap, rpcParams);
    }

    [ClientRpc]
    private void SendLobbyClientRpc(Character[] isPlayer, string map, ClientRpcParams rpcParams = default)
    {
        Game.selectedMap = map;

        for(int i=0; i<isPlayer.Length; i++)
        {   
            if (isPlayer[i] != Character.None)
            {
                InstantiatePlayer(i, isPlayer[i], NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    /*
     * Removing all NetworkPlayers associated with client when they disconnect
     */
    void OnClientDisconnect(ulong clientID)
    {
        if(IsServer)
        {
            NetworkPlayer networkPlayer;

            for(int i=0; i<Game.maxPlayers; i++)
            {
                if (Game.players[i] != null)
                {
                    networkPlayer = Game.players[i].GetComponent<NetworkPlayer>();

                    if(networkPlayer && (networkPlayer.GetClientID() == clientID))
                    {  
                        RemovePlayer(i);
                    }
                }
            }
        }
    }

    /*
     * Adding a new NetworkPlayer to clients
     */
    public void AddPlayer(int i)
    {
        ulong localID = NetworkManager.Singleton.LocalClientId;

        if(IsServer) 
        {
            AddPlayerClientRpc(i, Character.Knight, localID);
        }
        else
        {
            AddPlayerServerRpc(i, Character.Knight, localID);
        }
    }

    [ServerRpc(RequireOwnership=false)]
    private void AddPlayerServerRpc(int i, Character character, ulong clientID)
    {
        AddPlayerClientRpc(i, character, clientID);
    }

    [ClientRpc]
    private void AddPlayerClientRpc(int i, Character character, ulong clientID)
    {
        if (!Game.players[i]) {
            InstantiatePlayer(i, character, clientID);
        }
    }

    private void InstantiatePlayer(int i, Character character, ulong clientID)
    {
        Game.players[i] = Instantiate(networkPlayer);
        NetworkPlayer player = Game.players[i].GetComponent<NetworkPlayer>();

        player.SetClientID(clientID);
        player.inputType = InputType.Network;
        player.character = character;
        player.GetComponent<PlayerSpriteHandler>().OnCharacterChanged(character);
        GameObject.FindWithTag("LobbyManager").SendMessage("SetPlayerPosition", i);
    }

    /*
     * Removes NetworkPlayers from clients when the local player it represents
     * is destroyed
     */
    public void RemovePlayer(int player)
    {
        if(IsServer)
        {
            RemovePlayerClientRpc(player);
        }
        else
        {
            RemovePlayerServerRpc(player);
        }
    }

    [ServerRpc(RequireOwnership=false)]
    private void RemovePlayerServerRpc(int player)
    {
        RemovePlayerClientRpc(player);
    }

    [ClientRpc]
    private void RemovePlayerClientRpc(int player)
    {
        if (Game.players[player] && Game.players[player].GetComponent<NetworkPlayer>()) 
        {
            Destroy(Game.players[player]);
            Game.players[player] = null;
        }
    }

    public void OnCharacterChanged(int player)
    {
        if (IsServer)
        {
            OnCharacterChangedClientRpc(player);
        } 
        else 
        {
            OnCharacterChangedServerRpc(player);
        }
    }

    [ServerRpc(RequireOwnership=false)]
    private void OnCharacterChangedServerRpc(int player)
    {
        OnCharacterChangedClientRpc(player);
    }

    [ClientRpc]
    private void OnCharacterChangedClientRpc(int player)
    {
        if (Game.players[player].tag == "NetworkPlayer")
        {
            Game.players[player].GetComponent<Player>().OnCharacterChanged();
        }
    }

    [ClientRpc]
    public void OnGameStartClientRpc()
    {
        GameObject.FindObjectOfType<LobbyManager>().StartGame();
    }

    /*
     * Updating NetworkPlayer positions for all clients
     */
    public void StartPositionUpdating()
    {
        sendingPositions = true;
        StartCoroutine("SendLocalPositions");
    }

    public void StopPositionUpdating()
    {
        sendingPositions = false;
    }

    IEnumerator SendLocalPositions()
    {
        while(sendingPositions) 
        {
            for(int i=0; i<Game.maxPlayers; i++)
            {
                if(Game.players[i] && Game.players[i].tag == "Player")
                {
                    if(IsServer)
                    {
                        SendLocalPositionClientRpc(i, Game.players[i].transform.position);
                    } 
                    else 
                    {
                        SendLocalPositionServerRpc(i, Game.players[i].transform.position);
                    }
                }
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    [ClientRpc]
    private void SendLocalPositionClientRpc(int i, Vector2 pos)
    {
        if (Game.players[i].tag == "NetworkPlayer")
        {
            Game.players[i].transform.position = pos;
        }
    }

    [ServerRpc(RequireOwnership=false)]
    private void SendLocalPositionServerRpc(int i, Vector2 pos)
    {
        SendLocalPositionClientRpc(i, pos);
    }

    public void UpdateHealth(int playerNum, float health)
    {
        if(IsServer)
        {
            UpdateHealthClientRpc(playerNum, health);
        }
        else
        {
            UpdateHealthServerRpc(playerNum, health);
        }
    }

    public void UpdateSprite(string trigger, GameObject player)
    {
        int playerNum = ArrayUtility.IndexOf(Game.players, player);

        if (IsServer)
        {
            UpdateSpriteClientRpc(trigger, playerNum);
        }
        else
        {
            UpdateSpriteServerRpc(trigger, playerNum);
        }
    }

    [ClientRpc]
    private void UpdateSpriteClientRpc(string trigger, int player)
    {
        Game.players[player].GetComponent<Animator>().SetTrigger(trigger);
    }

    [ServerRpc(RequireOwnership=false)]
    private void UpdateSpriteServerRpc(string trigger, int player)
    {
        UpdateSpriteClientRpc(trigger, player);
    }

    [ClientRpc]
    private void UpdateHealthClientRpc(int playerNum, float health)
    {
        Game.players[playerNum].GetComponent<Player>().health = health;
    }

    [ServerRpc(RequireOwnership=false)]
    private void UpdateHealthServerRpc(int playerNum, float health)
    {
        UpdateHealthClientRpc(playerNum, health);
    }

    public void UpdateCooldown(int player, ulong clientID)
    {
        GetCooldownServerRpc(player, NetworkManager.Singleton.LocalClientId, clientID);
    }

    [ServerRpc(RequireOwnership=false)]
    private void GetCooldownServerRpc(int player, ulong senderID, ulong clientID)
    {
        if (senderID != clientID)
        {
            ulong[] target = new ulong[1]{clientID};
            ClientRpcParams rpcParams = default;
            rpcParams.Send.TargetClientIds = target;

            GetCooldownClientRpc(player, senderID, rpcParams);
        }
    }

    [ClientRpc]
    private void GetCooldownClientRpc(int player, ulong senderID, ClientRpcParams rpcParams = default)
    {
        float cooldown = Game.players[player].GetComponent<PlayerInputHandler>().ability2.remainingCooldown;
        ReturnCooldownServerRpc(player, cooldown, senderID);
    }

    [ServerRpc(RequireOwnership=false)]
    private void ReturnCooldownServerRpc(int player, float cooldown, ulong senderID)
    {
        ulong[] target = new ulong[1]{senderID};
        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = target;

        ReturnCooldownClientRpc(player, cooldown, rpcParams);
    }

    [ClientRpc]
    private void ReturnCooldownClientRpc(int player, float cooldown, ClientRpcParams rpcParams = default)
    {
        Game.players[player].GetComponent<NetworkPlayer>().abilityCooldown = cooldown;
    }


    public void SpawnProjectile(string type, int direction, Vector3 pos)
    {
        sentFromThisClient = true;

        if(IsServer)
        {
            SpawnProjectileClientRpc(type, direction, pos, localID);
        }
        else
        {
            SpawnProjectileServerRpc(type, direction, pos, localID);
        }
    }

    [ClientRpc]
    private void SpawnProjectileClientRpc(string type, int direction, Vector3 pos, ulong senderID)
    {
        if (sentFromThisClient == false)
        {
            Hitbox hitbox = (Instantiate(networkHitbox).GetComponent<Hitbox>());

            hitbox.spriteRenderer.flipX = (direction==1) ? false : true;

            switch(type)
            {
            case "arrow":
                hitbox.spriteRenderer.sprite = arrow;

                Vector2 size = hitbox.spriteRenderer.sprite.bounds.size;
                hitbox.boxCollider.size = size; 
                hitbox.boxCollider.offset = new Vector2(0.0f, 0.65f);

                hitbox.gameObject.transform.localScale = new Vector3(2.5f, 2.5f, 1.0f);
                hitbox.gameObject.transform.position = pos + new Vector3(0.6f*direction,-0.59f,0.0f);

                hitbox.piercing = true;
                hitbox.movement = new Vector3(0.3f*direction,0.0f,0.0f);
                break;
            case "frostbolt":
                hitbox.animator.runtimeAnimatorController = frostbolt;

                hitbox.boxCollider.size = new Vector2(0.24f, 0.14f);
                hitbox.boxCollider.offset = new Vector2(0.02f, 0.0f);

                hitbox.gameObject.transform.localScale = new Vector3(1.75f, 1.75f, 1.0f);
                hitbox.gameObject.transform.position = pos + new Vector3(0.5f*direction,0.725f,0.0f);

                hitbox.movement = new Vector3(0.3f*direction,0.0f,0.0f);
                break;
            }
        }
        else
        {
            sentFromThisClient = false;
        }
    }

    [ServerRpc(RequireOwnership=false)]
    private void SpawnProjectileServerRpc(string type, int direction, Vector3 pos, ulong senderID)
    {
        SpawnProjectileClientRpc(type, direction, pos, senderID);
    }
}