using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telepathy;
using UnityEngine;

public class NetworkBehviour : MonoBehaviour
{
    public static NetworkBehviour Instance;

    public Action<byte[]> OnModelReceived;

    [SerializeField] private int _networkPortTCP;
    [SerializeField] private int _operationsPerUpdateCount = 1000;
    [SerializeField] private string _applicationServerIp;

    private Client _clientTCP;
    private int _connectionTimeOutMs = 10000;
    private float _tcpKeepAliveNextTime;
    private float _tcpKeepAliveDelaySec = 30;

    private ModelReceiver _modelReceiver;

    public bool IsConnected { get { return _clientTCP.Connected; } }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            DestroyImmediate(gameObject);

        _clientTCP = new Client(100000);
        _clientTCP.ReceiveTimeout = 1000 * 60; // 1 minute


        _clientTCP.OnConnected = () => OnConnectedToServer();
        _clientTCP.OnData = (message) => OnDataReceived(message);
        _clientTCP.OnDisconnected = () => OnDisconnected();
    }

    private void Update()
    {
        _clientTCP.Tick(_operationsPerUpdateCount);


        if (_clientTCP.Connected)
        {
            if (Time.time > _tcpKeepAliveNextTime)
            {
                SendKeepAlive();

                _tcpKeepAliveNextTime += _tcpKeepAliveDelaySec;
            }
        }
    }

    private void OnApplicationQuit()
    {
        //_clientUDP.Disconnect();
        _clientTCP.Disconnect();
    }


    public async Task<bool> Connect()
    {
        _clientTCP.Connect(_applicationServerIp, _networkPortTCP, null);

        int timer = 0;
        while (!_clientTCP.Connected)
        {
            await Task.Delay(100);
            timer += 100;

            if (timer > _connectionTimeOutMs)
                return false;
        }

        _tcpKeepAliveNextTime = Time.time;
        return true;
    }

    private void SendKeepAlive()
    {
        SendNetworkMessage(BitConverter.GetBytes((int)MessageType.KEEP_ALIVE));
    }

    public void SendNetworkMessage(byte[] data)
    {
        if (!_clientTCP.Connected)
            return;

        if (!_clientTCP.Send(new ArraySegment<byte>(data)))
        {
            Debug.LogError($"Sending TCP error. data size: {data.Length}/{_clientTCP.MaxMessageSize}");
            throw new Exception();
        }
    }

    public void Disconnect()
    {
        if (_clientTCP.Connected)
            _clientTCP.Disconnect();
    }

    public async Task SendModel(byte[] modelData, string name, Action<float> onPercentChange)
    {
        Debug.Log("Connected");
        await Task.Delay(500);

        var helloMessage = new List<byte>();
        helloMessage.AddRange(BitConverter.GetBytes((int)MessageType.HELLO));
        helloMessage.AddRange(BitConverter.GetBytes(true));

        SendNetworkMessage(helloMessage.ToArray());

        await Task.Delay(500);

        var infoMessage = new List<byte>();
        infoMessage.AddRange(BitConverter.GetBytes((int)MessageType.ModelFromPhone));
        infoMessage.AddRange(BitConverter.GetBytes(0)); // model info

        var namaArray = System.Text.Encoding.UTF8.GetBytes(name);
        infoMessage.AddRange(BitConverter.GetBytes(namaArray.Length));
        infoMessage.AddRange(namaArray);

        infoMessage.AddRange(BitConverter.GetBytes(modelData.Length));
        SendNetworkMessage(infoMessage.ToArray());

        await Task.Delay(500);

        int sendedBytes = 0;

        do
        {
            if (!IsConnected)
            {
                Debug.Log("Disconnected while transfering");
                break;
            }

            var chunckMsg = new List<byte>();
            chunckMsg.AddRange(BitConverter.GetBytes((int)MessageType.ModelFromPhone));
            chunckMsg.AddRange(BitConverter.GetBytes(1)); // model data

            var chunckLenght = (modelData.Length - sendedBytes < 90000) ? modelData.Length - sendedBytes : 90000;
            chunckMsg.AddRange(BitConverter.GetBytes(chunckLenght));
            chunckMsg.AddRange(new ArraySegment<byte>(modelData, sendedBytes, chunckLenght));

            SendNetworkMessage(chunckMsg.ToArray());
            sendedBytes += chunckLenght;
            Debug.Log($"Sended: {sendedBytes}/{modelData.Length}");
            onPercentChange?.Invoke(((float)sendedBytes / (float)modelData.Length) * 100);
            await Task.Delay(500);
        }
        while (sendedBytes < modelData.Length);

        Debug.Log("Sended ALL");

    }

    private void OnConnectedToServer()
    {
    }

    private async void OnDisconnected()
    {

    }

    private void OnDataReceived(ArraySegment<byte> bufferSegment)
    {
        var arrayData = new byte[bufferSegment.Count];
        Buffer.BlockCopy(bufferSegment.Array, 0, arrayData, 0, arrayData.Length);

        int offset = 0;
        var messageType = (MessageType)BitConverter.ToInt32(arrayData, offset);
        offset += 4;

        switch (messageType)
        {
            case MessageType.KEEP_ALIVE:
                break;
            case MessageType.GotModelsListFromServer:
                HandleModelList(arrayData, offset);
                break;
            case MessageType.GotModelFromServer:
                if (_modelReceiver == null)
                    _modelReceiver = new ModelReceiver();

                if(_modelReceiver.HandleMessage(arrayData, offset))
                {
                    if(_modelReceiver.IsReceiveCompleted)
                    {
                        OnModelReceived?.Invoke(_modelReceiver.FullModelData);
                        _modelReceiver = null;
                    }
                }
                break;

            default:
                throw new Exception($"Unhandled message type: {messageType}");
        }
    }


    private void HandleModelList(byte[] data, int offset)
    {
        var count = BitConverter.ToInt32(data, offset);
        offset += 4;

        List<string> names = new List<string>();

        for (int i = 0; i < count; ++i)
        {
            var nameLenght = BitConverter.ToInt32(data, offset);
            offset += 4;

            var nameData = new byte[nameLenght];
            Buffer.BlockCopy(data, offset, nameData, 0, nameLenght);
            offset += nameLenght;

            var name = System.Text.Encoding.UTF8.GetString(nameData);
            names.Add(name);

            Debug.Log(name);
        }
        
    }

    [ContextMenu("GetModelList")]
    public void SendGetModelList()
    {
        var buffer = new List<byte>();
        buffer.AddRange(BitConverter.GetBytes((int)MessageType.GetModelsListFromVR));

        SendNetworkMessage(buffer.ToArray());
    }

    public string ModelNemTest;
    public Material DeserializeMaterial;
    [ContextMenu("GetModel")]
    public void GetModel()
    {
        OnModelReceived += async (byte[] d) =>
        {
            Debug.Log($"Received model {d.Length}");

            await new WaitForUpdate();

            var serializer = new ModelSerializer();
            serializer.MaterialForDeserialize = DeserializeMaterial;

            serializer.Deserialize(d, 0);
        };

        var buffer = new List<byte>();

        buffer.AddRange(BitConverter.GetBytes((int)MessageType.GetModelFromVR));

        var nameData = System.Text.Encoding.UTF8.GetBytes(ModelNemTest);
        buffer.AddRange(BitConverter.GetBytes(nameData.Length));
        buffer.AddRange(nameData);

        SendNetworkMessage(buffer.ToArray());
    }
}

public enum MessageType
{
    KEEP_ALIVE = 255,

    HELLO = 0,
    ModelFromPhone = 2,
    GetModelsListFromVR = 3,
    GetModelFromVR = 4,

    SendMaodelsListToVR = 10,
    SendModelToVR = 11,

    GotModelsListFromServer = 20,
    GotModelFromServer = 21
}

public class ModelReceiver
{
    private enum ModelMessageType
    {
        ModelInfo = 0,
        ModelDataPart = 1
    }

    public string ModelName;
    public byte[] FullModelData;

    public bool IsReceiveCompleted { get; private set; } = false;

    private int _fullModelLenght;
    private int _fullModelOffset = 0;
    public bool HandleMessage(byte[] data, int offset)
    {
        var msgType = (ModelMessageType)BitConverter.ToInt32(data, offset);
        offset += 4;

        switch (msgType)
        {
            case ModelMessageType.ModelInfo:
                HandleInfo(data, offset);
                break;
            case ModelMessageType.ModelDataPart:
                HandleData(data, offset);
                break;
            default:
                break;
        }

        return true;
    }

    private void HandleInfo(byte[] data, int offset)
    {
        //int nameLenght = BitConverter.ToInt32(data, offset);
        //offset += 4;

        //var nameArray = new byte[nameLenght];
        //Buffer.BlockCopy(data, offset, nameArray, 0, nameLenght);
        //offset += nameLenght;

        //var name = System.Text.Encoding.UTF8.GetString(nameArray);
        var modelLenght = BitConverter.ToInt32(data, offset);


        //ModelName = name;
        _fullModelLenght = modelLenght;

        FullModelData = new byte[_fullModelLenght];

        Debug.Log($"Start receive model: . Lenght: {_fullModelLenght}");
    }

    private void HandleData(byte[] data, int offset)
    {
        var chunkLenght = BitConverter.ToInt32(data, offset);
        offset += 4;

        Buffer.BlockCopy(data, offset, FullModelData, _fullModelOffset, chunkLenght);
        _fullModelOffset += chunkLenght;

        if (FullModelData.Length == _fullModelOffset)
            IsReceiveCompleted = true;

        Debug.Log($"Receiving model: . {_fullModelOffset}/{_fullModelLenght}");
    }
}