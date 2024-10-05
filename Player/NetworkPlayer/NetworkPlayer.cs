using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NetworkPlayer : Player
{
    public float abilityCooldown;
    
    private ulong clientID;

    public ulong GetClientID()
    {
        return clientID;
    }

    public void SetClientID(ulong clientID)
    {
        this.clientID = clientID;
    }

    void OnDestroy()
    {
        int playerNum = ArrayUtility.IndexOf(Game.players, this.gameObject);
        LobbyManager lobbyManager = GameObject.FindObjectOfType<LobbyManager>();

        if (playerNum != -1) {Game.players[playerNum] = null;}
        if(lobbyManager) {lobbyManager.SendMessage("UpdateLobby");}    
    }
}
