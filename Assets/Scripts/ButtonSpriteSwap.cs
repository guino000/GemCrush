using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ButtonSpriteSwap : MonoBehaviour {

    public List<Sprite> buttonModeSprites;
    private bool buttonMode = false;

    public void ChangeButtonMode()
    {
        buttonMode = !buttonMode;

        if (!buttonMode)
            gameObject.GetComponent<Image>().sprite = buttonModeSprites[0];
        else
            gameObject.GetComponent<Image>().sprite = buttonModeSprites[1];
    }

}
