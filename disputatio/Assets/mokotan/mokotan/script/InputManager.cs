using Fungus;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    DialogInput dialogInput;
    [SerializeField]
    public static MenuDialog ActiveMenuDialog { get; set; }

    public void TurnOffEnable()
    {
        dialogInput.enabled = false;
    }

    public void TurnOnEnable()
    {
        dialogInput.enabled = true;
    }
    }
    
    

