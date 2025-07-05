//using Steamworks;
//using System.IO;
//using UnityEngine;
//using Unity.Netcode;

//public class VoiceChat : NetworkBehaviour
//{
//    [SerializeField]
//    private AudioSource source;

//    private MemoryStream output;
//    private MemoryStream stream;
//    private MemoryStream input;

//    private int optimalRate;
//    private int clipBufferSize;
//    private float[] clipBuffer;

//    private int playbackBuffer;
//    private int dataPosition;
//    private int dataReceived;

//    private void Start()
//    {
//        if (!IsOwner) return;

//        optimalRate = (int)SteamUser.OptimalSampleRate;

//        clipBufferSize = optimalRate * 5;
//        clipBuffer = new float[clipBufferSize];

//        stream = new MemoryStream();
//        output = new MemoryStream();
//        input = new MemoryStream();

//        source.clip = AudioClip.Create("VoiceData", 256, 1, optimalRate, true, OnAudioRead, null);
//        source.loop = true;
//        source.Play();
//    }

//    private void Update()
//    {
//        if (!IsOwner) return;

//        //SteamUser.VoiceRecord = Input.GetKey(KeyCode.V);
//        SteamUser.VoiceRecord = true;

//        if (SteamUser.HasVoiceData)
//        {
//            int compressedWritten = SteamUser.ReadVoiceData(stream);
//            stream.Position = 0;

//            SendVoiceDataServerRpc(stream.GetBuffer(), compressedWritten);
//        }
//    }

//    [ServerRpc]
//    private void SendVoiceDataServerRpc(byte[] compressed, int bytesWritten)
//    {
//        ReceiveVoiceDataClientRpc(compressed, bytesWritten);
//        Debug.Log("sending voice data");
//    }

//    [ClientRpc]
//    private void ReceiveVoiceDataClientRpc(byte[] compressed, int bytesWritten)
//    {
//        if (IsOwner) return;

//        input.Write(compressed, 0, bytesWritten);
//        input.Position = 0;

//        int uncompressedWritten = SteamUser.DecompressVoice(input, bytesWritten, output);
//        input.Position = 0;

//        byte[] outputBuffer = output.GetBuffer();
//        WriteToClip(outputBuffer, uncompressedWritten);
//        output.Position = 0;
//        Debug.Log("receiving voice data");
//    }

//    private void OnAudioRead(float[] data)
//    {
//        for (int i = 0; i < data.Length; ++i)
//        {
//            // start with silence
//            data[i] = 0;

//            // do I have anything to play?
//            if (playbackBuffer > 0)
//            {
//                // current data position playing
//                dataPosition = (dataPosition + 1) % clipBufferSize;

//                data[i] = clipBuffer[dataPosition];

//                playbackBuffer--;
//            }
//        }
//        Debug.Log("read audio");
//    }

//    private void WriteToClip(byte[] uncompressed, int iSize)
//    {
//        for (int i = 0; i < iSize; i += 2)
//        {
//            // insert converted float to buffer
//            float converted = (short)(uncompressed[i] | uncompressed[i + 1] << 8) / 32767.0f;
//            clipBuffer[dataReceived] = converted;

//            // buffer loop
//            dataReceived = (dataReceived + 1) % clipBufferSize;

//            playbackBuffer++;
//        }
//        Debug.Log("writing to clip");
//    }
//}

using Steamworks;
using System.IO;
using UnityEngine;
using Unity.Netcode;

public class VoiceChat : NetworkBehaviour
{
    [SerializeField]
    private AudioSource source;

    private MemoryStream output;
    private MemoryStream stream;
    private MemoryStream input;

    private int optimalRate;
    private int clipBufferSize;
    private float[] clipBuffer;

    private int playbackBuffer;
    private int dataPosition;
    private int dataReceived;

    private void Start()
    {
        optimalRate = (int)SteamUser.OptimalSampleRate;

        clipBufferSize = optimalRate * 5; // 5 seconds of buffer
        clipBuffer = new float[clipBufferSize];

        stream = new MemoryStream();
        output = new MemoryStream();
        input = new MemoryStream();

        source.clip = AudioClip.Create("VoiceData", optimalRate, 1, optimalRate, true, OnAudioRead, null);
        source.loop = true;
        source.Play();

        //Debug.Log("VoiceChat initialized");
    }

    private void Update()
    {
        if (!IsOwner) return;

        SteamUser.VoiceRecord = true; // Always recording for test

        if (SteamUser.HasVoiceData)
        {
            stream.SetLength(0);
            int compressedWritten = SteamUser.ReadVoiceData(stream);

            if (compressedWritten > 0)
            {
                stream.Position = 0;
                SendVoiceDataServerRpc(stream.GetBuffer(), compressedWritten);
                //Debug.Log($"Sent voice data: {compressedWritten} bytes");
            }
        }
    }

    [ServerRpc]
    private void SendVoiceDataServerRpc(byte[] compressed, int bytesWritten)
    {
        ReceiveVoiceDataClientRpc(compressed, bytesWritten);
        //Debug.Log("Forwarded voice data to clients");
    }

    [ClientRpc]
    private void ReceiveVoiceDataClientRpc(byte[] compressed, int bytesWritten)
    {
        if (IsOwner || compressed == null || bytesWritten == 0) return;

        input.SetLength(0);
        input.Write(compressed, 0, bytesWritten);
        input.Position = 0;

        output.SetLength(0);
        int uncompressedWritten = SteamUser.DecompressVoice(input, bytesWritten, output);

        if (uncompressedWritten > 0)
        {
            byte[] outputBuffer = output.GetBuffer();
            WriteToClip(outputBuffer, uncompressedWritten);
            //Debug.Log($"Received and wrote voice: {uncompressedWritten} bytes");
        }
    }

    private void OnAudioRead(float[] data)
    {
        for (int i = 0; i < data.Length; ++i)
        {
            data[i] = 0;

            if (playbackBuffer > 0)
            {
                dataPosition = (dataPosition + 1) % clipBufferSize;
                data[i] = clipBuffer[dataPosition];
                playbackBuffer--;
            }
        }
    }

    private void WriteToClip(byte[] uncompressed, int size)
    {
        for (int i = 0; i < size; i += 2)
        {
            short sample = (short)(uncompressed[i] | (uncompressed[i + 1] << 8));
            float converted = sample / 32767.0f;

            clipBuffer[dataReceived] = converted;
            dataReceived = (dataReceived + 1) % clipBufferSize;

            playbackBuffer++;
        }
    }
}
