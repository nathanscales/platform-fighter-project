using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public int playerNum;
    public TMP_Text txtPlayerNum, txtAbilityCooldown, txtAbilityReady, txtHealth;
    public GameObject imgBackground, imgHeart;

    private Player player;
    private PlayerInputHandler inputHandler;
    private NetworkPlayerManager networkPlayerManager;
    private bool active;

    void Awake()
    {
        if (Game.players[playerNum])
        {
            SetActive(true);
            player = Game.players[playerNum].GetComponent<Player>();
            inputHandler = player.GetComponent<PlayerInputHandler>();
            networkPlayerManager = GameObject.FindObjectOfType<NetworkPlayerManager>();
            OnAbilityReady();
        }
        else
        {
            SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if (active && player != null)
        {
            txtHealth.text = (player.health > 0) ? player.health+"" : "0";
            
            if (player.gameObject.tag == "NetworkPlayer")
            {
                networkPlayerManager.UpdateCooldown(playerNum, ((NetworkPlayer)player).GetClientID());
                txtAbilityCooldown.text = ((NetworkPlayer)player).abilityCooldown+"";
            }
            else
            {
                txtAbilityCooldown.text = inputHandler.ability2.remainingCooldown+"";
            }

            if (float.Parse(txtAbilityCooldown.text) <= 0)
            {
                OnAbilityReady();
            }
            else
            {
                OnAbilityUsed();
            }
        }
    }

    private void SetActive(bool b)
    {
        active = b;
        txtPlayerNum.gameObject.SetActive(b);
        txtAbilityCooldown.gameObject.SetActive(b);
        txtAbilityReady.gameObject.SetActive(b);
        txtHealth.gameObject.SetActive(b);
        imgBackground.gameObject.SetActive(b);
        imgHeart.gameObject.SetActive(b);
    }

    private void OnAbilityReady()
    {
        txtAbilityCooldown.gameObject.SetActive(false);
        txtAbilityReady.gameObject.SetActive(true);
    }

    private void OnAbilityUsed()
    {
        txtAbilityCooldown.gameObject.SetActive(true);
        txtAbilityReady.gameObject.SetActive(false);
    }
}
