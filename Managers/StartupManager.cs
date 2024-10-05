using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupManager : MonoBehaviour
{
    public GameObject eventSystem, networkPlayerManager;

    void Awake()
    {
        DontDestroyOnLoad(eventSystem);
        SceneManager.LoadScene("Menu");
    }
}
