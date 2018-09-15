using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    static private string m_recordingDevice = ""; // hold the name of the default recording device.
    private const int m_recordingFrequency = 8000; // How many samples in a second?
    private AudioClip m_recordedSound; // whatever we are recording.
    void InitMicrophone()
    {
        foreach (string micDevice in Microphone.devices)
        {
            m_recordingDevice = micDevice;
        }
    }

    // Use this for initialization
    void Start () {
        InitMicrophone(); // find a microphone
        m_recordedSound = Microphone.Start(m_recordingDevice, false, 1, 8000);
    }

    // Update is called once per frame
    void Update () {
        // check if the mic is recording, if so, get some bytes and store it in a buffer, maybe?
		if (Microphone.IsRecording(m_recordingDevice))
        {

        }
	}
}
