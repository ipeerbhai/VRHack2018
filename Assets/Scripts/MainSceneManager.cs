using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneManager : MonoBehaviour {

    private webTransactionManager m_webManager;
    private SoundManager m_soundManager;
	// Use this for initialization
	void Start () {
        // get references to the inited monobehaviors.
        GameObject web = GameObject.Find("WebManager");
        m_webManager = (webTransactionManager)web.GetComponent(typeof(webTransactionManager));
        m_soundManager = (SoundManager)GameObject.Find("SoundManager").GetComponent(typeof(SoundManager)); // one-liner get of the monobehavior
        m_webManager.Scene = this;
    }
	
    public void ForwardClipForPlay(byte[] buffer)
    {
        m_soundManager.PlaybackBuffers.Enqueue(buffer);
    }

	// Update is called once per frame
	void Update () {
        // Check the sound manager and see if it has data for us.
        if ((m_soundManager.RecordingData != null) && (m_soundManager.RecordingData.Length > 0))
        {
            TransferData Data = TransferData.Empty();
            Data.TypeOfByte = ByteCodes.sound;
            Data.SoundData = m_soundManager.RecordingData;
            m_webManager.QueueData(Data);
        }
    }
}
