using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    private AudioSource m_microphoneAudioSource = null;
    public byte[] RecordingData { get; set; } // the buffer.
    static private string m_recordingDevice = ""; // hold the name of the default recording device.
    public int RecordingFrequency { get; private set; } // How many samples in a second?
    private const int m_recordingSeconds = 3;
    void InitMicrophone()
    {
        RecordingFrequency = 8000;
        m_microphoneAudioSource = GetComponent<AudioSource>(); // need the attached audio source to get audio data.
        foreach (string micDevice in Microphone.devices)
        {
            m_recordingDevice = micDevice;
        }
    }

    // Use this for initialization
    void Start () {
        InitMicrophone(); // find a microphone
        m_microphoneAudioSource.clip = Microphone.Start(m_recordingDevice, false, m_recordingSeconds, RecordingFrequency);
    }

    // Update is called once per frame
    void Update () {
        // check if the mic is recording, if so, get some bytes and store it in a buffer, maybe?
		if (Microphone.IsRecording(null))
        {
            int numSamples = Microphone.GetPosition(null);
            if (numSamples > 0)
            {
                // we have data.  Promote it to a data buffer, restart the microphone...
                float[] samples = new float[numSamples];
                m_microphoneAudioSource.clip.GetData(samples, 0); // copy the float samples over to a temp buffer;

                // convert from 32-bit float to PCM-8 audio sample format.
                byte[] PCM8Samples = new byte[numSamples];
                for ( int i = 0; i < numSamples; i++)
                {
                    PCM8Samples[i] = (byte)(samples[i] * 255);
                }
                RecordingData = PCM8Samples;

                // restart the Mic
                Microphone.End(null);
                m_microphoneAudioSource.clip = Microphone.Start(m_recordingDevice, false, m_recordingSeconds, RecordingFrequency);
            }
        }
	}
}
