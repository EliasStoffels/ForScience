using UnityEngine;

public class Exit : MonoBehaviour
{
    [SerializeField] private GameObject m_WallToClose;

    public bool isConnectedOrClosed;
    public void Close()
    {
        m_WallToClose.SetActive(true);
        isConnectedOrClosed = true;
    }
}
