using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : NetworkBehaviour
{
    [SerializeField] GameObject m_Camera;
    [SerializeField] Vector3 m_CameraOffset;
    [SerializeField] float m_SensX;
    [SerializeField] float m_SensY;

    private Transform m_CameraTransform;
    private float m_DeltaX;
    private float m_DeltaY;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        m_CameraTransform = GameObject.Instantiate(m_Camera).transform;

        for (int i = 0; i < transform.childCount; i++)
        {
            var renderer = transform.GetChild(i).GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
            return;

        Vector2 delta = Mouse.current.delta.ReadValue();

        m_DeltaX += delta.x * m_SensX * Time.deltaTime;
        m_DeltaY += delta.y * m_SensY * Time.deltaTime;
        m_DeltaY = Mathf.Clamp(m_DeltaY, -60,60);

        if(m_CameraTransform != null)
        {
            m_CameraTransform.rotation = Quaternion.Euler(-m_DeltaY, m_DeltaX , 0);
            m_CameraTransform.position = transform.position + m_CameraOffset;
        }

        transform.rotation = Quaternion.Euler(0, m_DeltaX, 0);
    }
}
