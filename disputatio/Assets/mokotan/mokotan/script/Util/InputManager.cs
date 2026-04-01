using Fungus;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    DialogInput dialogInput;

    public void TurnOffEnable()
    {
        if (dialogInput != null)
            dialogInput.enabled = false;
    }

    public void TurnOnEnable()
    {
        if (dialogInput != null)
            dialogInput.enabled = true;
    }
}

