using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class SceneLoader : NetworkBehaviour
{
    private Animator animator;
    private string sceneName;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void FadeToScene(string sceneName)
    {
        if (!IsClient)
        {
            this.sceneName = sceneName;
            animator.SetTrigger("FadeOut");
        } 
        else if (IsServer)
        {
            FadeToSceneClientRpc(sceneName);
        }
    }

    public void FadeToScene(string sceneName, float speed)
    {
        animator.SetFloat("animSpeed", speed);
        FadeToScene(sceneName);
        animator.SetFloat("animSpeed", 1.0f);
    }

    [ClientRpc]
    public void FadeToSceneClientRpc(string sceneName)
    {
        this.sceneName = sceneName;
        animator.SetTrigger("FadeOut");
    }

    public void OnFadeComplete()
    {
        SceneManager.LoadScene(sceneName);
    }
}