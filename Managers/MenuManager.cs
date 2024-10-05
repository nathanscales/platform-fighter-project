using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Networking;
using TMPro;

public class MenuManager : NetworkBehaviour
{
    public SceneLoader sceneLoader;
    public TMP_Text txtConnecting;
    
    private NetworkPlayerManager networkPlayerManager;
    private bool canUseButtons;

    void Awake()
    {
        networkPlayerManager = GameObject.FindObjectOfType<NetworkPlayerManager>();
    }

    void OnEnable() 
    {
        txtConnecting.gameObject.SetActive(false);
        canUseButtons = true;
    }

    void OnStartLocalGame()
    {
        if (canUseButtons)
        {
            canUseButtons = false;
            sceneLoader.FadeToScene("MapSelect");
        }
    }

    void OnHostOnlineGame()
    {
        if (canUseButtons)
        {
            canUseButtons = false;
            networkPlayerManager.startingHost = true;
            sceneLoader.FadeToScene("MapSelect");
        }
    }

    void OnJoinOnlineGame()
    {
        if (canUseButtons)
        {
            canUseButtons = false;
            NetworkManager.Singleton.StartClient();
            StartCoroutine("AwaitApproval");
        }
    }

    private IEnumerator AwaitApproval()
    {
        float timeElapsed = 0.0f;

        txtConnecting.text = "Connecting...";
        txtConnecting.gameObject.SetActive(true);

        while (!networkPlayerManager.approved && timeElapsed < NetworkManager.NetworkConfig.ClientConnectionBufferTimeout)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            timeElapsed += Time.deltaTime;
        }

        if (networkPlayerManager.approved) 
        {
            sceneLoader.FadeToScene("Lobby");
        }
        else
        {
            txtConnecting.text = "Connection Failed!";
            NetworkManager.Singleton.Shutdown();
            canUseButtons = true;
        }

        yield return new WaitForSeconds(2.5f);
        txtConnecting.gameObject.SetActive(false);
    }

    void OnExit()
    { 
        Application.Quit();
    }
}