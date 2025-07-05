using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("parameters")] 
    [SerializeField] private const float SPEED = 5f;
    [SerializeField] private const float JUMP_VELOCITY = 5f;
    [SerializeField] private const float JUMP_BUFFER = .5f;
    [SerializeField] private const float GROUNDED_BUFFER = .5f;

    [Header("references")]
    [SerializeField] private Rigidbody m_Rigidbody;
    [SerializeField] private LayerMask m_GroundLayer;

    private PlayerActions m_Actions;
    private Vector2 m_MoveInput;
    private float m_TimeSinceGrounded = float.MaxValue;
    private float m_TimeSinceJumpInput = float.MaxValue;

    private UiController m_UiController;

    public override void OnNetworkSpawn()
    {
        if(!IsOwner)
        {
            enabled = false;
            return;
        }
    }

    private void Awake()
    {
        m_Actions = new PlayerActions();
        m_UiController = GameObject.Find("UiController").GetComponent<UiController>();
        m_UiController.Spawned();
    }
    private void OnEnable()
    {
        m_Actions.Enable();
        m_Actions.Gameplay.Movement.performed += OnMove;
        m_Actions.Gameplay.Movement.canceled += OnCancelMove;
        m_Actions.Gameplay.Jump.performed += OnJump;
        m_Actions.Gameplay.Ui.performed += OnUi;
    }

    private void OnDisable()
    {
        m_Actions.Gameplay.Movement.performed -= OnMove;
        m_Actions.Gameplay.Movement.canceled -= OnCancelMove;
        m_Actions.Gameplay.Jump.performed -= OnJump;
        m_Actions.Gameplay.Ui.performed -= OnUi;
        m_Actions.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        m_TimeSinceGrounded += Time.deltaTime;
        m_TimeSinceJumpInput += Time.deltaTime;

        // grounded check
        if (Physics.Raycast(transform.position, Vector3.down, 0.1f, m_GroundLayer) && m_Rigidbody.linearVelocity.y < 2f)
        {
            m_TimeSinceGrounded = 0f;
        }

        // handle movement input
        Vector3 forwardVelocity = transform.forward * m_MoveInput.y;
        Vector3 sidewaysVelocity = transform.right * m_MoveInput.x;

        Vector3 totalVelocity = forwardVelocity + sidewaysVelocity;
        totalVelocity = totalVelocity.normalized * SPEED;

        // handle jump input
        if (m_TimeSinceJumpInput < JUMP_BUFFER && m_TimeSinceGrounded < GROUNDED_BUFFER)
        {
            totalVelocity.y = JUMP_VELOCITY;
            m_TimeSinceJumpInput = float.MaxValue;
            m_TimeSinceGrounded = float.MaxValue;
        }
        else
            totalVelocity.y = m_Rigidbody.linearVelocity.y;

        // set velocity
        m_Rigidbody.linearVelocity = totalVelocity;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        m_MoveInput = context.ReadValue<Vector2>();
    }
    private void OnCancelMove(InputAction.CallbackContext context)
    {
        m_MoveInput = Vector3.zero;
    }
    private void OnJump(InputAction.CallbackContext obj)
    {
        m_TimeSinceJumpInput = 0f;
    }

    private void OnUi(InputAction.CallbackContext obj)
    {
        m_UiController.ToggleUi();
    }
}
