using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Netcode;
using TMPro;

public static class Game {
    public static int maxPlayers = 4;
    public static GameObject[] players;
    public static string selectedMap;
}

public class LobbyManager : NetworkBehaviour
{
    public SceneLoader sceneLoader;
    public Button btnStartGame;
    public TMP_Text txtPlayer1Ability1, txtPlayer2Ability1, txtPlayer3Ability1, txtPlayer4Ability1;
    public TMP_Text txtPlayer1Ability2, txtPlayer2Ability2, txtPlayer3Ability2, txtPlayer4Ability2;
    public Sprite kbQ, kbE, xboxA, xboxB, network;
    public Image player1Ability1Key, player2Ability1Key, player3Ability1Key, player4Ability1Key;
    public Image player1Ability2Key, player2Ability2Key, player3Ability2Key, player4Ability2Key;

    private TMP_Text[] playerAbility1, playerAbility2;
    private Image[] playerAbility1Key, playerAbility2Key;
    private NetworkPlayerManager networkPlayerManager;
    private Vector2 playerAnchorPoint = new Vector2(-6.0f,0.36f);
    private int playerSpacing = 4;

    void Awake() 
    {
        networkPlayerManager = GameObject.FindObjectOfType<NetworkPlayerManager>();
        Game.players = new GameObject[Game.maxPlayers];
        playerAbility1 = new TMP_Text[]{txtPlayer1Ability1, txtPlayer2Ability1, txtPlayer3Ability1, txtPlayer4Ability1};
        playerAbility2 = new TMP_Text[]{txtPlayer1Ability2, txtPlayer2Ability2, txtPlayer3Ability2, txtPlayer4Ability2};
        playerAbility1Key = new Image[]{player1Ability1Key, player2Ability1Key, player3Ability1Key, player4Ability1Key};
        playerAbility2Key = new Image[]{player1Ability2Key, player2Ability2Key, player3Ability2Key, player4Ability2Key};
    }
    
    void OnEnable()
    {
        ResetLobby();
        btnStartGame.gameObject.SetActive((!NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer));
    }

    public void ResetLobby()
    {
        for(int i=0; i<Game.maxPlayers; i++)
        {
            if(Game.players[i] != null) {Destroy(Game.players[i]);}
        }
        UpdateLobby();
    }

    void UpdateLobby()
    {
        Player player;

        for(int i=0; i<Game.maxPlayers; i++)
        {
            playerAbility1Key[i].gameObject.SetActive(true);
            playerAbility2Key[i].gameObject.SetActive(true);

            if (Game.players[i])
            {
                player = Game.players[i].GetComponent<Player>();

                switch(player.inputType)
                {
                case InputType.Keyboard:
                    playerAbility1Key[i].sprite = kbQ;
                    playerAbility2Key[i].sprite = kbE;
                    break;
                case InputType.Gamepad:
                    playerAbility1Key[i].sprite = xboxA;
                    playerAbility2Key[i].sprite = xboxB;
                    break;
                case InputType.Network:
                    playerAbility1Key[i].sprite = network;
                    playerAbility2Key[i].gameObject.SetActive(false);
                    break;
                case InputType.AI:
                    playerAbility1Key[i].gameObject.SetActive(false);
                    playerAbility2Key[i].gameObject.SetActive(false);
                    break;
                }

                switch(player.character)
                {
                case Character.Knight:
                    playerAbility1[i].text = "Swing your sword in front of you. Can be used while airborne.";
                    playerAbility2[i].text = "Swing your sword around you, hitting all nearby enemies. (15 Sec Cooldown)";
                    break;
                case Character.Rogue:
                    playerAbility1[i].text = "Quickly strike with your dagger.";
                    playerAbility2[i].text = "Leap backwards. (10 Sec Cooldown)";
                    break;
                case Character.Sorcerer:
                    playerAbility1[i].text = "Fire a frostbolt which increases damage the more it travels.";
                    playerAbility2[i].text = "Ride a wave for 5 seconds which deals damage and makes you immune to damage. (15 Sec Cooldown)";
                    break;
                case Character.Ranger:
                    playerAbility1[i].text = "Fire an arrow that damages and pierces through enemies.";
                    playerAbility2[i].text = "Roll a short distance. (5 Sec Cooldown)";
                    break;
                }
            } else {
                playerAbility1Key[i].gameObject.SetActive(false);
                playerAbility2Key[i].gameObject.SetActive(false);
                playerAbility1[i].text = "Open Slot";
                playerAbility2[i].text = "";
            }
        }
    }

    void OnPlayerJoined(PlayerInput playerInput) 
    {
        if (playerInput.devices[0].name == "Keyboard")
        {
            playerInput.gameObject.GetComponent<Player>().inputType = InputType.Keyboard;
            playerInput.SwitchCurrentActionMap("WASD");
        } else {
            playerInput.gameObject.GetComponent<Player>().inputType = InputType.Gamepad;
        }

        for(int i=0; i<Game.maxPlayers; i++) 
        {
            if(!Game.players[i]) 
            {
                Game.players[i] = playerInput.gameObject;
                SetPlayerPosition(i);
                UpdateLobby();

                if(networkPlayerManager.IsClient) { networkPlayerManager.AddPlayer(i); }

                break;
            }
        }
    }

    public void SetPlayerPosition(int playerNum)
    {
        Game.players[playerNum].transform.position = playerAnchorPoint + (Vector2.right * playerSpacing * playerNum);
        UpdateLobby();
    }

    void OnReturnToMenu()
    {
        sceneLoader.FadeToScene("menu");

        StartCoroutine("ResetLobbyAfterDelay");

    }

    IEnumerator ResetLobbyAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);
        NetworkManager.Singleton.Shutdown();
        ResetLobby();
    }

    void OnGameStart()
    {
        int playerCount = 0;

        for (int i=0; i<Game.maxPlayers; i++)
        {
            if (Game.players[i]) {playerCount++;}
        }

        if (playerCount >= 2) 
        {
            if (!networkPlayerManager.IsClient)
            {
                StartGame();
            }
            else if(networkPlayerManager.IsServer)
            {
                networkPlayerManager.OnGameStartClientRpc();
            }
        }
    }

    public void StartGame()
    {
        for (int i=0; i<Game.maxPlayers; i++)
        {
            if (Game.players[i] && Game.players[i].GetComponent<Player>().inputType != InputType.Network)
            {
                Game.players[i].GetComponent<Player>().OnGameStart();
            }
        }

        networkPlayerManager.StartPositionUpdating();
        sceneLoader.FadeToScene(Game.selectedMap);
    }
}
