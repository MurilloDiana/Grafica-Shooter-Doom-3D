using UnityEngine;

public class RestartButton : MonoBehaviour
{
    public void Restart()
    {
        GameManager manager = GameManager.Instance != null ? GameManager.Instance : FindObjectOfType<GameManager>();
        if (manager != null)
        {
            manager.RestartLevel();
        }
    }
}
