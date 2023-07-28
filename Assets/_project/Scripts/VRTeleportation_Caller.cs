using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRTeleportation_Caller : MonoBehaviour
{
    public Action<byte[], int> OnSamplePartRecorded;

    [SerializeField] private AudioSource _output;
    [SerializeField] private float _chunkLenght = 0.1f;

    private AudioClip _inputClip;

    private int _lastSamplePosition = 0;
    private float _timer;
    private bool _isRecordEnabled = false;

    private Queue<(byte[], int)> _chunksQueue = new Queue<(byte[], int)>();

    private Coroutine _playerRoutine;

    private int _outgoingChunkNumber = 0;
    private int _incomingChunkNumber = 0;
    private bool _isMuted = false;

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        OnSamplePartRecorded += (d, s) => { VRTeleportation_NetworkBehviour.Instance.SendCallFrame(d,s, _outgoingChunkNumber++); };

        VRTeleportation_NetworkBehviour.Instance.OnAudioFrameReceived += (chunkNumber, channels, frame) =>
        {
            if(chunkNumber > _incomingChunkNumber)
            {
                Debug.Log("Audio chunk");
                _incomingChunkNumber = chunkNumber;
                PlaySamplePart(channels, frame);
            }
        };

        VRTeleportation_NetworkBehviour.Instance.OnIncomingCall += () =>
        {
            UIController.Instance.CallPanel.Show(CallState.Incoming);
        };

        VRTeleportation_NetworkBehviour.Instance.OnConnectedToCall += (active) =>
        {
            if (active)
            {
                UIController.Instance.CallPanel.Show(CallState.Calling);
                StartRecord();
            }
            else
            {
                UIController.Instance.CallPanel.Show(CallState.Crash);
                StopRecord();
            }
        };
    }

    public void MuteActive()
    {
        _isMuted = !_isMuted;
    }

    public async void StartRecord()
    {
        _inputClip = Microphone.Start(null, true, 5, 44100);
        while (Microphone.GetPosition(null) < 0) { }
        _isRecordEnabled = true;
        Debug.Log("Start Recording");
        //_playerRoutine = StartCoroutine(PlayerRoutine());
    }

    public void StopRecord()
    {
        _inputClip = null;
        _isRecordEnabled = false;

        _lastSamplePosition = 0;
        _timer = 0;

        _outgoingChunkNumber = 0;
        _incomingChunkNumber = 0;

        Microphone.End(null);
        if (_playerRoutine != null)
        {
            StopCoroutine(_playerRoutine);
            _playerRoutine = null;
        }
    }

    private void FixedUpdate()
    {
        _timer += Time.deltaTime;

        if (_isRecordEnabled)
        {
            if (_timer > _chunkLenght)
            {

                _timer = 0;
                int pos = Microphone.GetPosition(null);
                int diff = pos - _lastSamplePosition;
                if (diff > 0 && !_isMuted)
                {
                    float[] samples = new float[diff * _inputClip.channels];
                    _inputClip.GetData(samples, _lastSamplePosition);
                    byte[] ba = ToByteArray(samples);
                        OnSamplePartRecorded?.Invoke(ba, _inputClip.channels);
                }

                _lastSamplePosition = pos;
            }
        }
    }





    private byte[] ToByteArray(float[] floatArray)
    {
        int len = floatArray.Length * 4;
        byte[] byteArray = new byte[len];
        int pos = 0;
        foreach (float f in floatArray)
        {
            byte[] data = System.BitConverter.GetBytes(f);
            System.Array.Copy(data, 0, byteArray, pos, 4);
            pos += 4;
        }
        return byteArray;
    }






    private IEnumerator PlayerRoutine()
    {
        while (true)
        {
            if (_output.isPlaying)
            {
                yield return new WaitForFixedUpdate();
            }
            else
            {
                if (_chunksQueue.Count > 0)
                {
                    var chunk = _chunksQueue.Dequeue();
                    PlaySamplePart(chunk.Item2, chunk.Item1);
                }
                else
                {
                    yield return new WaitForFixedUpdate();
                }
            }
        }
    }

    public void PlaySamplePart(int channels, byte[] audioData)
    {
        var array = ToFloatArray(audioData, out int samplesCount);

        var clip = AudioClip.Create("temp", samplesCount, channels, 44100, false);
        clip.SetData(array, 0);

        _output.clip = clip;
        _output.Play();
    }

    public void SoundActive()
    {
        if (!_output.isPlaying)
        {
            _output.Play();
        }
        else
        {
            _output.volume = 0;
        }
    }

    private float[] ToFloatArray(byte[] byteArray, out int samplesCount)
    {
        int len = byteArray.Length / 4;
        float[] floatArray = new float[len];
        for (int i = 0; i < byteArray.Length; i += 4)
        {
            floatArray[i / 4] = System.BitConverter.ToSingle(byteArray, i);
        }

        samplesCount = len;
        return floatArray;
    }

}
