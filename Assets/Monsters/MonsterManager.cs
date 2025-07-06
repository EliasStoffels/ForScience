using UnityEngine;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour
{
    [SerializeField] private GameObject[] m_Monsters;

    private float m_TotalDt;

    private DungeonGenerator m_Generator;

    private bool m_SpawnedDingDong = false;

    void Start()
    {
        m_Generator = DungeonGenerator.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        m_TotalDt += Time.deltaTime;
        if (m_TotalDt > 5f && !m_SpawnedDingDong)
        {
            m_SpawnedDingDong = true;
            var randomRoom = m_Generator.rooms[m_Generator.Random.Next(0,m_Generator.rooms.Count)];

            GameObject.Instantiate(m_Monsters[m_Generator.Random.Next(0, m_Monsters.Length)], randomRoom.transform.position + new Vector3(0,0.1f,0), Quaternion.identity);
        }
    }
}
