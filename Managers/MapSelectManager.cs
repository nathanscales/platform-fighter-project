using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MapSelectManager : NetworkBehaviour
{
    public SceneLoader sceneLoader;
    private NetworkPlayerManager networkPlayerManager;

    void Awake()
    {
        networkPlayerManager = GameObject.FindObjectOfType<NetworkPlayerManager>();
    }

    void OnMap1Selected()
    {
        Game.selectedMap = "map1";
        GoToLobby();
    }

    void OnMap2Selected()
    {
        Game.selectedMap = "map2";
        GoToLobby();
    }

    void OnMap3Selected()
    {
        Game.selectedMap = "map3";
        GoToLobby();
    }

    void OnMap4Selected()
    {
        Game.selectedMap = "map4";
        GoToLobby();
    }

    void GoToLobby()
    {
        if (networkPlayerManager.startingHost) 
        {
            NetworkManager.Singleton.StartHost();
            networkPlayerManager.startingHost = false;
        }

        sceneLoader.FadeToScene("Lobby");
    }

    void OnReturnToMenu()
    {
        sceneLoader.FadeToScene("Menu");
    }
}
