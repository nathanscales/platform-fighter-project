using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public SceneLoader sceneLoader;
    public TMP_Text txtTitle, txtPlayer1Num, txtPlayer2Num, txtPlayer3Num, txtPlayer4Num;
    public Button btnReturn;
    public Vector3 player1Spawn, player2Spawn, player3Spawn, player4Spawn;
    public bool paused;

    private TMP_Text[] playerNums;
    private int winner;
    
    void OnEnable()
    {
        txtTitle.gameObject.SetActive(false);
        btnReturn.gameObject.SetActive(false);
        Vector3[] spawnPoints = new Vector3[]{player1Spawn, player2Spawn, player3Spawn, player4Spawn};
        playerNums = new TMP_Text[]{txtPlayer1Num, txtPlayer2Num, txtPlayer3Num, txtPlayer4Num};

        for(int i=0; i<Game.maxPlayers; i++)
        {
            if(Game.players[i]) 
            {
                Game.players[i].transform.localScale = new Vector3(3.0f,3.0f,1.0f);
                Game.players[i].transform.position = spawnPoints[i];
            }
        }

        for (int i=0; i<Game.maxPlayers; i++)
        {
            playerNums[i].transform.position = new Vector3(1000.0f, 1000.0f, 0.0f);
        }
    }

    void FixedUpdate()
    {
        for (int i=0; i<Game.maxPlayers; i++)
        {
            if(Game.players[i]) 
            {
                playerNums[i].transform.position = Game.players[i].transform.position + new Vector3(2.6f,2.4f,0.0f);
            }
        }
    }

    public void OnPlayerDeath()
    {
        int playersAlive = 0;

        for (int i=0; i<Game.maxPlayers; i++)
        {
            if (Game.players[i] && !Game.players[i].GetComponent<Player>().dead)
            {
                playersAlive++;
                winner = i+1;
            }
        }

        if (playersAlive <= 1)
        {
            StartCoroutine("OnGameEnd");
        }
    }

    public void OnPause()
    {
        paused = true;
        txtTitle.text = "Paused";
        txtTitle.gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    public void OnUnpause()
    {
        paused = false;
        txtTitle.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    IEnumerator OnGameEnd()
    {
        txtTitle.text = "Player " + winner + " Wins";
        yield return new WaitForSeconds(1.5f);
        txtTitle.gameObject.SetActive(true);
        btnReturn.gameObject.SetActive(true);
    }

    private void OnReturn()
    {
        StartCoroutine("Cleanup");
        sceneLoader.FadeToScene("Menu");
    }

    IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(1.0f);
        NetworkManager.Singleton.Shutdown();
        
        for (int i=0; i<Game.maxPlayers; i++)
        {
            Destroy(Game.players[i]);
        }
    }
}
