using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;


#if NETFX_CORE
using Windows.Networking;
using Windows.Networking.Connectivity;
using System.IO;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Foundation;
using System.Threading.Tasks;
#endif

namespace HoloGroup.Networking.Internal.Sockets
{
    public class TCPsocket : IDisposable
    {
        private ICustomTCP _socketBehaviour;
        private string _port;
        private string _ip;
#if NETFX_CORE
    private StreamSocket _socket;
    private StreamSocketListener _socketListener;
#else
        private TcpClient _socket;
        private TcpListener _socketListener;
#endif

        private TCPsocket() { }
        public TCPsocket(ICustomTCP socketBehaviour, ETcpSocketType socketType, int port, string ip = "")
        {
            if (socketType == ETcpSocketType.Socket && ip == "")
                throw new ArgumentException("IP can not be null then SocketType is Socket");

            _socketBehaviour = socketBehaviour;
            _ip = ip;
            _port = port.ToString();

            if (socketType == ETcpSocketType.ListenerSocket)
                OpenSocketListener();
            else
                OpenSocket();

        }

        private void OpenSocketListener()
        {
#if NETFX_CORE
        try
        {
            _socketListener = new StreamSocketListener();
            _socketListener.ConnectionReceived += SocketListener_ConnectionReceived;

            IAsyncAction listenerOpenning = _socketListener.BindServiceNameAsync(_port);
            AsyncActionCompletedHandler listenStarted = SocketListener_ListenerOpened;
            listenerOpenning.Completed += listenStarted;
        }
        catch (Exception e)
        {
            _socketBehaviour.OnSocketListenerStartingError(e);
        }
#else
            try
            {
                _socketListener = new TcpListener(int.Parse(_port));
                _socketListener.Start();
                _socketListener.BeginAcceptTcpClient(SocketListener_ConnectionReceived, _socketListener);
                _socketBehaviour.OnListenerOpened(true);
            }
            catch (Exception e)
            {
                _socketBehaviour.OnListenerOpened(false);
                _socketBehaviour.OnSocketListenerStartingError(e);
            }
#endif
        }

        private void OpenSocket()
        {
#if NETFX_CORE
        try
        {
            _socket = new StreamSocket();
            HostName serverHost = new HostName(_ip);

            IAsyncAction connect = _socket.ConnectAsync(serverHost, _port);
            AsyncActionCompletedHandler connected;
            connected = ConnectCompleted;
            connect.Completed = connected;
        }
        catch (Exception e)
        {
            _socketBehaviour.OnSocketStartingError(e);
        }
#else
            try
            {
                _socket = new TcpClient();
                _socket.BeginConnect(_ip, int.Parse(_port), ConnectCompleted, _socket);
            }
            catch (Exception e)
            {
                _socketBehaviour.OnSocketStartingError(e);
            }
#endif
        }

        public void Dispose()
        {
#if NETFX_CORE
        if (_socket != null)
            _socket.Dispose();
        if (_socketListener != null)
            _socketListener.Dispose();
#else
            if (_socket != null)
                _socket.Close();
            if (_socketListener != null)
                _socketListener.Stop();
#endif
            _socket = null;
            _socketListener = null;
        }

        #region TCP_INTERNAL_METHODS

#if NETFX_CORE

    private void SocketListener_ListenerOpened(IAsyncAction asyncInfo, AsyncStatus status)
    {
        if (status == AsyncStatus.Completed)
            _socketBehaviour.OnListenerOpened(true);
        else
            _socketBehaviour.OnListenerOpened(false);
    }

    private async void SocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        DataWriter networkWriter = new DataWriter(args.Socket.OutputStream);
        DataReader networkReader = new DataReader(args.Socket.InputStream);

        bool needCloseListener = await _socketBehaviour.ListenerTransferingProcess(networkWriter, networkReader);
        
        networkWriter.Dispose();
        networkReader.Dispose();
        if (needCloseListener)
        {
            _socketListener.Dispose();
            _socketListener = null;
        }
        args.Socket.Dispose();
    }

    private async void ConnectCompleted(IAsyncAction asyncInfo, AsyncStatus status)
    {
        if (status == AsyncStatus.Completed)
        {
            DataWriter networkWriter = new DataWriter(_socket.OutputStream);
            DataReader networkReader = new DataReader(_socket.InputStream);

            await _socketBehaviour.SocketTransferingProcess(true, networkWriter, networkReader);

            networkWriter.Dispose();
            networkReader.Dispose();
            _socket.Dispose();
            _socket = null;
        }
        else
        {
            _socket.Dispose();
            _socket = null;
            await _socketBehaviour.SocketTransferingProcess(false);
        }
    }

#else
        private async void SocketListener_ConnectionReceived(IAsyncResult res)
        {
            TcpClient client = _socketListener.EndAcceptTcpClient(res);
            NetworkStream stream = client.GetStream();

            bool needCloseListener = await _socketBehaviour.ListenerTransferingProcess(stream);

            stream.Dispose();
            client.Close();
            if (needCloseListener)
            {
                _socketListener.Stop();
                _socketListener = null;
            }
            else
                _socketListener.BeginAcceptTcpClient(SocketListener_ConnectionReceived, _socketListener);
        }

        private async void ConnectCompleted(IAsyncResult res)
        {
            if (_socket.Connected)
            {
                NetworkStream stream = _socket.GetStream();

                await _socketBehaviour.SocketTransferingProcess(true, stream);

                stream.Dispose();
                _socket.Close();
                _socket = null;
            }
            else
            {
                _socket.Close();
                _socket = null;
                await _socketBehaviour.SocketTransferingProcess(false);
            }
        }
#endif
        #endregion TCP_INTERNAL_METHODS
    }

    public enum ETcpSocketType
    {
        ListenerSocket,
        Socket
    }
}