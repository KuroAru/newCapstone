using UnityEngine;

public class MiniGameExit : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            MiniGameManager.Instance.Win();
        }
    }
}
