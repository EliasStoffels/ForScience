using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool connected;
    public bool inGame;
    public bool isHost;
    public ulong myClientId;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void HostCreated()
    {

    }

    public void ConnectedClient()
    {

    }

    public void Disconnected()
    {
        
    }

    public void Quit()
    {
        Application.Quit();
    }
}
