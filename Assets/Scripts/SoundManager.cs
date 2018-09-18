using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    private AudioSource m_SceneAudioSource = null;
    public Queue<byte[]> PlaybackBuffers { get; set; }
    private Queue<byte[]> m_fileBuffer = null; // to save as a WAV file
    private AudioClip m_recordedClip = null;
    public byte[] RecordingData { get; set; } // the buffer.
    static private string m_recordingDevice = ""; // hold the name of the default recording device.
    public int RecordingFrequency { get; private set; } // How many samples in a second?
    private const int m_recordingSeconds = 10;
    void InitMicrophone()
    {
        RecordingFrequency = 16000;
        m_SceneAudioSource = GetComponent<AudioSource>(); // need the attached audio source to get audio data.
        m_SceneAudioSource.enabled = true;
        m_SceneAudioSource.mute = false;
        
        bool bAppHasMicPerms = Application.HasUserAuthorization(UserAuthorization.Microphone);
        if (!bAppHasMicPerms)
        {
            // we need to ask for Mic permissions?
            Application.RequestUserAuthorization(UserAuthorization.Microphone);
        }

        foreach (string micDevice in Microphone.devices)
        {
            m_recordingDevice = micDevice;
        }

        if (m_recordingDevice.Length == 0)
            throw new System.Exception("No recording device");
    }

    // Use this for initialization
    void Start () {
        InitMicrophone(); // find a microphone
        m_recordedClip = Microphone.Start(null, true, m_recordingSeconds, RecordingFrequency);
        if (m_recordedClip == null)
            throw new System.Exception("Cannot create a valid clip");
        PlaybackBuffers = new Queue<byte[]>();
    }

    private byte[] ConvertToPCM8(float[] samples)
    {
        int numSamples = samples.Length;
        byte[] PCM8Samples = new byte[numSamples];
        for (int i = 0; i < numSamples; i++)
        {
            int sample = (int)(samples[i] * 127);
            PCM8Samples[i] = (byte)(sample);
        }
        return PCM8Samples;
    }

    private float[] ConvertPCM8ToFloat(byte[] samples)
    {
        int numSamples = samples.Length;
        float[] floatSamples = new float[numSamples];
        for (int i = 0; i < numSamples; i++)
        {
            float sample = (float)samples[i] / 127f;
            floatSamples[i] = (byte)(sample);
        }
        return floatSamples;

    }

    private void PlayAudioBuffer(byte[] buffer)
    {
        m_fileBuffer.Enqueue(buffer);
        int channels = m_recordedClip.channels;
        int frequency = m_recordedClip.frequency;
        AudioClip clip = AudioClip.Create("Playback", buffer.Length, channels, frequency, false);
        float[] floatBuffer = ConvertPCM8ToFloat(buffer);
        //clip.SetData(floatBuffer, 0);
        //m_SceneAudioSource.loop = false;
        //m_SceneAudioSource.Play();
    }

    private void PlayAudioBuffer(float[] buffer)
    {
        int channels = m_recordedClip.channels;
        int frequency = m_recordedClip.frequency;
        AudioClip clip = AudioClip.Create("Playback", buffer.Length, channels, frequency, false);
        clip.SetData(buffer, 0);
        m_SceneAudioSource.loop = false;
        m_SceneAudioSource.Play();
    }

    // Update is called once per frame
    void Update () {
        // check to see if there's something to playback, if so, play it back.
        if (PlaybackBuffers.Count > 0)
        {
            if (m_SceneAudioSource.isPlaying == false)
            {
                byte[] buffer = PlaybackBuffers.Dequeue();
                PlayAudioBuffer(buffer);
            }
        }
        // check if the mic is recording, if so, get some bytes and store it in a buffer, maybe?
		if (Microphone.IsRecording(null))
        {
            int numSamples = Microphone.GetPosition(null);
            if (numSamples > 0)
            {
                // we have data.  Promote it to a data buffer, restart the microphone...
                float[] samples = new float[numSamples];
                m_recordedClip.GetData(samples, 0); // copy the float samples over to a temp buffer;

                // loopback test
                PlayAudioBuffer(samples);

                // convert from 32-bit float to PCM-8 audio sample format.
                RecordingData = ConvertToPCM8(samples);
            }
            // Restart Mic
            //Microphone.End(null);
            //m_microphoneAudioSource.clip = Microphone.Start(m_recordingDevice, false, m_recordingSeconds, RecordingFrequency);
        }
        else
        {
            m_recordedClip = Microphone.Start(m_recordingDevice, true, m_recordingSeconds, RecordingFrequency);
        }
    }

    private void WriteWavHeader(System.IO.MemoryStream stream, bool isFloatingPoint, ushort channelCount, ushort bitDepth, int sampleRate, int totalSampleCount)
    {
        stream.Position = 0;

        // RIFF header.
        // Chunk ID.
        stream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);

        // Chunk size.
        stream.Write(BitConverter.GetBytes(((bitDepth / 8) * totalSampleCount) + 36), 0, 4);

        // Format.
        stream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);



        // Sub-chunk 1.
        // Sub-chunk 1 ID.
        stream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);

        // Sub-chunk 1 size.
        stream.Write(BitConverter.GetBytes(16), 0, 4);

        // Audio format (floating point (3) or PCM (1)). Any other format indicates compression.
        stream.Write(BitConverter.GetBytes((ushort)(isFloatingPoint ? 3 : 1)), 0, 2);

        // Channels.
        stream.Write(BitConverter.GetBytes(channelCount), 0, 2);

        // Sample rate.
        stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);

        // Bytes rate.
        stream.Write(BitConverter.GetBytes(sampleRate * channelCount * (bitDepth / 8)), 0, 4);

        // Block align.
        stream.Write(BitConverter.GetBytes((ushort)channelCount * (bitDepth / 8)), 0, 2);

        // Bits per sample.
        stream.Write(BitConverter.GetBytes(bitDepth), 0, 2);

        // Sub-chunk 2.
        // Sub-chunk 2 ID.
        stream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);

        // Sub-chunk 2 size.
        stream.Write(BitConverter.GetBytes((bitDepth / 8) * totalSampleCount), 0, 4);
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    // Creates a file-headered wave memory stream out of stream
    private MemoryStream createWaveFileStream(System.IO.MemoryStream stream, int samplingRate = 16000)
    {
        MemoryStream outputStream = new MemoryStream();
        WriteWavHeader(outputStream, false, 1, 16, samplingRate, (int)stream.Length / 2);
        outputStream.Write(stream.ToArray(), 0, (int)stream.Length);
        outputStream.Flush();
        outputStream.Position = 0;
        return (outputStream);
    }


    private void SaveFile()
    {
        // consolidate the clips together into a single buffer...

    }
}
