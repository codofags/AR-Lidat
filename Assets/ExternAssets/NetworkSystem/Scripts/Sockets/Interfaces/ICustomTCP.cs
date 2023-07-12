using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.Threading.Tasks;

#if NETFX_CORE
using Windows.Storage.Streams;
#endif

namespace HoloGroup.Networking.Internal.Sockets
{
    public interface ICustomTCP
    {
        void OnSocketStartingError(Exception e);
        void OnSocketListenerStartingError(Exception e);
        void OnListenerOpened(bool successfully);

#if NETFX_CORE
    Task<bool> ListenerTransferingProcess(DataWriter networkWriter, DataReader networkReader); // async
    Task SocketTransferingProcess(bool connectSuccessful, DataWriter networkWriter = null, DataReader networkReader = null);
#else
        Task<bool> ListenerTransferingProcess(NetworkStream networkStream);
        Task SocketTransferingProcess(bool connectSuccessful, NetworkStream networkStream = null);
#endif
    }
}