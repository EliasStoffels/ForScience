using UnityEngine;

public class SteamworksIntegration : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        try
        {
            //Steamworks.SteamClient.Init(480);
            PrintName();
            PrintFriends();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    // Update is called once per frame
    private void PrintName()
    {
        Debug.Log(Steamworks.SteamClient.Name);
    }

    private void PrintFriends()
    {
        foreach(var friend in Steamworks.SteamFriends.GetFriends())
        {
            Debug.Log(friend.Name);
        }
    }

    private void Update()
    {
        Steamworks.SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        Steamworks.SteamClient.Shutdown();
    }
}
