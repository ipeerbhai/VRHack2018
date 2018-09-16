using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//------------------------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------------------------

public enum ByteCodes
{
    sound = 0,
    Position = 1,
    PictureData = 2
}

public struct TransferData
{
    public ByteCodes TypeOfByte { get; set; }
    private byte ByteTypeCode { get; set; } // the enum converted to a byte.
    private byte LeftMostInt32DataLength { get; set; } // the left 8 bits of an int32 indicating how many bytes were transferred.
    private byte LeftInt32DataLength { get; set; } // the next 8 bits of an int32 indicating how many bytes were transferred.
    private byte RightInt32DataLength { get; set; } // the next 8 bits of an int32 indicating how many bytes were transferred.
    private byte RightMostInt32DataLength { get; set; } // the least significant 8 bits of an int32 indicating how many bytes were transferred.

    public byte[] SoundData { get; set; } // Pure PCM-8 samples, 8 8khz
    public byte[] PositionData { get; set; } // What the head data is
    public byte[] PictureData { get; set; } // What the picture is

    public byte[] ToDataStream()
    {
        byte[] contentBuffer = null;
        switch(TypeOfByte)
        {
            case ByteCodes.sound:
                contentBuffer = new byte[SoundData.Length + 5];
                SoundData.CopyTo(contentBuffer, 5);
                break;
            case ByteCodes.Position:
                contentBuffer = new byte[PositionData.Length + 5];
                PositionData.CopyTo(contentBuffer, 5);
                break;
            case ByteCodes.PictureData:
                contentBuffer = new byte[PictureData.Length + 5];
                PictureData.CopyTo(contentBuffer, 5);
                break;
        }
        ByteTypeCode = (byte)(int)TypeOfByte;
        int Length = contentBuffer.Length - 5;
        byte[] byteLength = BitConverter.GetBytes(Length);

        // assign the first 5 bytes as the type and length checksum ( minus 5 )
        contentBuffer[0] = ByteTypeCode;
        contentBuffer[1] = byteLength[0];
        contentBuffer[2] = byteLength[1];
        contentBuffer[3] = byteLength[2];
        contentBuffer[4] = byteLength[3];

        return contentBuffer;
    }

    static public TransferData Empty()
    {
        return(new TransferData());
    }

    private byte[] StripHeader(byte[] Data)
    {
        byte[] outputBuffer = new byte[Data.Length - 5];
        for ( int i = 5; i < Data.Length; i++)
        {
            outputBuffer[i - 5] = Data[i];
        }
        return outputBuffer;
    }

    public void FromCloud(byte[] Data)
    {
        // specific bytes are specific items...
        ByteTypeCode = Data[0];
        LeftMostInt32DataLength = Data[1];
        LeftInt32DataLength = Data[2];
        RightInt32DataLength = Data[3];
        RightMostInt32DataLength = Data[4];

        // find out the type.
        int ByteCodeTypeAsInt = (int)ByteTypeCode;
        switch (ByteCodeTypeAsInt)
        {
            case 0:
                TypeOfByte = ByteCodes.sound;
                break;
            case 1:
                TypeOfByte = ByteCodes.Position;
                break;
            case 2:
                TypeOfByte = ByteCodes.PictureData;
                break;
        }
        byte[] LengthBytes = new byte[4];
        LengthBytes[0] = LeftMostInt32DataLength;
        LengthBytes[1] = LeftInt32DataLength;
        LengthBytes[2] = RightInt32DataLength;
        LengthBytes[3] = RightMostInt32DataLength;


        // let's get the length
        int Length = BitConverter.ToInt32(LengthBytes, 0);


        // Copy the data to the correct buffer.
        switch (TypeOfByte)
        {
            case ByteCodes.sound:
                SoundData = StripHeader(Data) ;
                break;
            case ByteCodes.PictureData:
                PictureData = StripHeader(Data);
                break;
            case ByteCodes.Position:
                PositionData = StripHeader(Data);
                break;
        }
    }
}

//------------------------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------------------------

public class webTransactionManager : MonoBehaviour {

    private System.Guid m_thisClient; // Track which client this is.
    private string m_wsURI = @"ws://dev.thinkpredict.com:4000";
    private WebSocketSharpUnityMod.WebSocket m_connection; // use this connection to a websocket service.
    private Coroutine m_runningTransfer; // Hold a reference to the running send coroutine
    private Queue<TransferData> OutboundData = new Queue<TransferData>(); // heading to cloud.
    private Queue<TransferData> InboundData= new Queue<TransferData>(); // coming from cloud.
    private byte[] Data { get; set; } // an outbound buffer we want to send over the websocket.byte

    void MsgHandler(object sender, WebSocketSharpUnityMod.MessageEventArgs e)
    {
        TransferData Datum = TransferData.Empty();
        if (e.IsBinary)
            Datum.FromCloud(e.RawData);
        //System.Console.WriteLine(e.Data.ToString());
    }


    // Use this for initialization
    void Start () {
        m_thisClient = System.Guid.NewGuid(); // create a random number we can send...

        m_connection = new WebSocketSharpUnityMod.WebSocket(m_wsURI); // connect to a server running on me.
        m_connection.OnMessage += MsgHandler; // register an event handler.
        m_runningTransfer = StartCoroutine(Transfer()); // Once in a while, try sending data over the socket.
    }

    public void QueueData( TransferData Data)
    {
        OutboundData.Enqueue(Data);
    }

    IEnumerator Transfer()
    {
        m_connection.Connect();
        while (true)
        {
            if (OutboundData.Count > 0)
            {
                TransferData Data = OutboundData.Dequeue();
                {
                    m_connection.Send(Data.ToDataStream());
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    
	
	// Update is called once per frame
	void Update () {
		
	}
}
