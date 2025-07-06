using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] public Exit[] exits;
    [SerializeField] public Collider boundsCollider;

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

            GameObject[] shuffledRooms = DungeonGenerator.Instance.smallRooms;
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
                                    room.boundsCollider.bounds.center,
                                    room.boundsCollider.bounds.extents - new Vector3(0.05f,0.05f ,0.05f),
                                    prefab.transform.rotation,
                                    LayerMask.GetMask("Dungeon")
                                    );


                if (!hits.Any(hit => hit.gameObject != prefab))
                {
                    break;
                }
                //else
                //{
                //    foreach (var hit in hits)
                //    {
                //        Debug.Log(hit);

                //        Bounds bounds = hit.bounds;
                //        Vector3 center = bounds.center;
                //        Vector3 extents = bounds.extents;

                //        if(extents.x < 2 || extents.y < 2 || extents.z < 2)
                //        {
                //            Debug.Log("this one is the small one " + hit);
                //        }

                //        Vector3[] corners = new Vector3[8];

                //        // Compute the 8 corners of the bounds
                //        corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
                //        corners[1] = center + new Vector3(extents.x, -extents.y, -extents.z);
                //        corners[2] = center + new Vector3(extents.x, -extents.y, extents.z);
                //        corners[3] = center + new Vector3(-extents.x, -extents.y, extents.z);
                //        corners[4] = center + new Vector3(-extents.x, extents.y, -extents.z);
                //        corners[5] = center + new Vector3(extents.x, extents.y, -extents.z);
                //        corners[6] = center + new Vector3(extents.x, extents.y, extents.z);
                //        corners[7] = center + new Vector3(-extents.x, extents.y, extents.z);

                //        UnityEngine.Color color = UnityEngine.Color.green;
                //        float duration = 1000f; // very long duration (or call this every frame)

                //        // Bottom face
                //        Debug.DrawLine(corners[0], corners[1], color, duration);
                //        Debug.DrawLine(corners[1], corners[2], color, duration);
                //        Debug.DrawLine(corners[2], corners[3], color, duration);
                //        Debug.DrawLine(corners[3], corners[0], color, duration);

                //        // Top face
                //        Debug.DrawLine(corners[4], corners[5], color, duration);
                //        Debug.DrawLine(corners[5], corners[6], color, duration);
                //        Debug.DrawLine(corners[6], corners[7], color, duration);
                //        Debug.DrawLine(corners[7], corners[4], color, duration);

                //        // Vertical edges
                //        Debug.DrawLine(corners[0], corners[4], color, duration);
                //        Debug.DrawLine(corners[1], corners[5], color, duration);
                //        Debug.DrawLine(corners[2], corners[6], color, duration);
                //        Debug.DrawLine(corners[3], corners[7], color, duration);
                //    }
                //}

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

            DungeonGenerator.Instance.rooms.Add(room);
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
