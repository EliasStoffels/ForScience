using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    [SerializeField] private GameObject m_HostButton;
    [SerializeField] private GameObject m_Canvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Spawned()
    {
        m_HostButton.SetActive(false);
        m_Canvas.SetActive(false);
    }

    public void ToggleUi()
    {
        m_Canvas.SetActive(!m_Canvas.activeSelf);

        if(m_Canvas.activeSelf)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
