using UnityEngine;
using UnityEngine.AI;

public class DingDongBehavior : MonoBehaviour
{
    [SerializeField] private NavMeshAgent m_Agent;
    [SerializeField] private float m_VisionDistance;
    [SerializeField] private float m_VisionAngle;

    private GameObject m_Target = null;

    private Vector3 m_LastTargetPosition = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        if(m_Target == null)
        {
            var hits = Physics.OverlapSphere(transform.position, m_VisionDistance, LayerMask.GetMask("Player"));
            foreach (var hit in hits)
            {
                Vector3 toPlayer = hit.transform.position - transform.position;
                if (Physics.Raycast(transform.position, toPlayer, m_VisionDistance, LayerMask.GetMask("Wall")))
                    continue;

                if (Mathf.Abs(Vector3.Angle(transform.position, toPlayer)) < m_VisionAngle)
                {
                    m_Target = hit.gameObject;
                    break;
                }

            }
        }

        if (m_Target != null)
        {
            if (Physics.Raycast(transform.position, m_Target.transform.position - transform.position, m_VisionDistance, LayerMask.GetMask("Wall")))
            {
                m_LastTargetPosition = m_Target.transform.position;
                m_Target = null;
            }
            else
                m_Agent.destination = m_Target.transform.position;
        }
        
        if(m_LastTargetPosition != Vector3.zero)
        {
            m_Agent.destination = m_LastTargetPosition;
            m_LastTargetPosition = Vector3.zero;
        }

    }
}
