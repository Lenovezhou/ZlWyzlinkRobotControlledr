using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR

public static class NetworkExtensions {
    const int BUFFER_SIZE = 256;
    static byte[] buffer = new byte[BUFFER_SIZE];

    static public void BeginWrite(this NetworkStream stream, string message)
    {
        Encoding.ASCII.GetBytes(message, 0, message.Length, buffer, 0);
        stream.BeginWrite(buffer, 0, message.Length, (IAsyncResult ar) => {
            Debug.Log("Sent message: " + message);
            stream.EndWrite(ar);
        }, null);
    }

    static public void BeginRead(this NetworkStream stream, Action<string> callback)
    {
        stream.BeginRead(buffer, 0, buffer.Length, (IAsyncResult ar) => {
            if (ar.IsCompleted)
            {
                int bytesRead = stream.EndRead(ar);
                string result = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Debug.Log("Received message: " + result);
                callback(result);
            }
            else
            {
                stream.EndRead(ar);
            }
        }, null);
    }
}

#endif