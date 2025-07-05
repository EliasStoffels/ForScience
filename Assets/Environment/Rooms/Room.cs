using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] public Exit[] exits;
    [SerializeField] public Collider collider;

    public DungeonGenerator generator;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public bool HasOpenExit()
    {
        return exits.Any(e => !e.isConnectedOrClosed);
    }

    public void CloseRemaining()
    {
        foreach(Exit exit in exits)
        {
            if(!exit.isConnectedOrClosed)
            {
                exit.Close();
            }
        }
    }

    public void SpawnNext(System.Random rng)
    {
        foreach (var exit in exits)
        {
            if (exit.isConnectedOrClosed) continue;

            GameObject[] shuffledRooms = generator.smallRooms;
            Shuffle(shuffledRooms, rng);

            GameObject prefab = null;
            Room room = null;
            Exit roomExit = null;

            foreach(var rngRoom in shuffledRooms)
            {
                prefab = Instantiate(rngRoom, Vector3.zero, Quaternion.identity);
                room = prefab.GetComponent<Room>();

                roomExit = room.exits[rng.Next(room.exits.Length)];

                prefab.transform.rotation = exit.transform.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(roomExit.transform.localRotation);
                Vector3 offset = roomExit.transform.position - prefab.transform.position;
                prefab.transform.position = exit.transform.position - offset;
                prefab.transform.parent = transform.parent;

                Physics.SyncTransforms();

                Collider[] hits = Physics.OverlapBox(
                                    room.collider.bounds.center,
                                    room.collider.bounds.extents - new Vector3(0.05f,0.05f ,0.05f),
                                    prefab.transform.rotation,
                                    LayerMask.GetMask("Dungeon")
                                    );

                foreach(var hit in hits)
                {
                    Debug.Log(hit);
                }

                if (!hits.Any(hit => hit.gameObject != prefab))
                {
                    break;
                }

                Destroy(prefab);
                prefab = null;
                room = null;
                roomExit = null;
            }

            if(prefab == null)
            {
                exit.Close();
                exit.isConnectedOrClosed = true;
                return;
            }

            roomExit.isConnectedOrClosed = true;
            exit.isConnectedOrClosed = true;

            generator.rooms.Add(room);
        }
    }

    public static void Shuffle<T>(T[] array, System.Random rng)
    {
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (array[n], array[k]) = (array[k], array[n]);
        }
    }
}
