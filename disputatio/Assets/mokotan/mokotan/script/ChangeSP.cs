using UnityEngine;
using Fungus;
using UnityEngine.UI;
using UnityEngine.Sprites;
public class ChangeSP : MonoBehaviour
{
     public Flowchart flowchart;

    public Button slot;
    public Sprite[] sprite;


    public void OnChangeButtonImage()
    {
        string sceneName = flowchart.GetStringVariable("CurrentScene");
        // slot = 
        switch (sceneName)
        {
            case "Opening":
                slot.image.sprite = sprite[0];
                break;
        }


    }
}