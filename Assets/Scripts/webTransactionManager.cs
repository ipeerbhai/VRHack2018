using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class webTransactionManager : MonoBehaviour {

    private System.Guid m_thisClient; // Track which client this is.
    private WebSocketSharpUnityMod.WebSocket m_connection; // use this connection to a websocket service.
    private Coroutine m_runningTransfer; // Hold a reference to the running send coroutine
    static private string m_RecordingDevice = ""; // hold the name of the default recording device.

    public byte[] Data { get; set; } // an outbound buffer we want to send over the websocket.


    void MsgHandler(object sender, WebSocketSharpUnityMod.MessageEventArgs e)
    {
        System.Console.WriteLine(e.ToString());
    }

    void InitMicrophone()
    {
        foreach (string micDevice in Microphone.devices)
        {
            m_RecordingDevice = micDevice;
        }
    }

    // Use this for initialization
    void Start () {
        m_thisClient = System.Guid.NewGuid(); // create a random number we can send...
        InitMicrophone(); // find a microphone

        m_connection = new WebSocketSharpUnityMod.WebSocket("ws://127.0.0.1:3000"); // connect to a server running on me.
        m_connection.OnMessage += MsgHandler; // register an event handler.
        m_runningTransfer = StartCoroutine(Transfer()); // Once in a while, try sending data over the socket.
    }

    IEnumerator Transfer()
    {
        while (true)
        {
            if (Data.Length != null)
            {
                m_connection.Send(Data);
                Data = null;
            }  
            yield return new WaitForSeconds(0.5f);
        }
    }

    
	
	// Update is called once per frame
	void Update () {
		
	}
}
