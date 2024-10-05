using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSpriteHandler : MonoBehaviour
{
    public Sprite buttonPressed;

    private Button button;
    private Image img;
    private Sprite buttonUnpressed;

    void Awake()
    {
        button = GetComponent<Button>();
        img = GetComponent<Image>();

        buttonUnpressed = img.sprite;
        button.onClick.AddListener(OnButtonPressed);
    }

    void OnButtonPressed()
    {
        img.sprite = buttonPressed;
        StartCoroutine("OnButtonUnpressed");
    }

    IEnumerator OnButtonUnpressed()
    {
        yield return new WaitForSeconds(0.1f);
        img.sprite = buttonUnpressed;
    }
}
