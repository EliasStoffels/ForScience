using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class DungeonGenerator : NetworkBehaviour
{
    [SerializeField] public GameObject[] smallRooms;
    [SerializeField] public GameObject[] mediumRooms;
    [SerializeField] public GameObject[] largeRooms;

    [SerializeField] Vector3 m_Limits;

    public List<Room> rooms = new List<Room>();
    private GameObject m_CurrentRoom;

    public static int m_Seed;
    private NetworkVariable<int> networkSeed = new(writePerm: NetworkVariableWritePermission.Server);

    private bool m_HasGenerated = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            m_Seed = UnityEngine.Random.Range(0, int.MaxValue);
            networkSeed.Value = m_Seed;
        }

        networkSeed.OnValueChanged += (_, newSeed) =>
        {
            m_Seed = newSeed;
            if (!m_HasGenerated)
            {
                m_HasGenerated = true;
                StartCoroutine(GenerateMaze(m_Seed));
            }
        };

        if (networkSeed.Value != 0 && !m_HasGenerated)
        {
            m_HasGenerated = true;
            StartCoroutine(GenerateMaze(networkSeed.Value));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject);
    }

    IEnumerator GenerateMaze(int seed)
    {
        System.Random rng = new System.Random(seed);
        m_CurrentRoom = Instantiate( smallRooms[ rng.Next( 0,smallRooms.Length )] );
        m_CurrentRoom.transform.parent = transform;
        var originalRoom = m_CurrentRoom;

        int faileSave = 0;
        while(m_CurrentRoom != null && faileSave < 10 )
        {
            Room room = m_CurrentRoom.GetComponent<Room>();
            rooms.Add(room);
            room.generator = this;
            room.SpawnNext(rng);

            ++faileSave;

            var nextRoom = rooms.Find(r => r.HasOpenExit());
            m_CurrentRoom = nextRoom != null ? nextRoom.gameObject : null;
            if (m_CurrentRoom == originalRoom)
            {
                Debug.Log("orginal room again?????");
            }
        }

        foreach(Room room in rooms)
        {
            room.CloseRemaining();
        }

        yield return null;
    }
}
