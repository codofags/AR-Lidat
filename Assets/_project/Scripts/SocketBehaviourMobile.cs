using HoloGroup.Networking.Internal.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class SocketBehaviourMobile : ICustomTCP
{
    public byte[] DataForSend;

    public Task<bool> ListenerTransferingProcess(NetworkStream networkStream)
    {
        throw new NotImplementedException();
    }

    public void OnListenerOpened(bool successfully)
    {
        throw new NotImplementedException();
    }

    public void OnSocketListenerStartingError(Exception e)
    {
        throw new NotImplementedException();
    }

    public void OnSocketStartingError(Exception e)
    {
        throw e;
    }

    public async Task SocketTransferingProcess(bool connectSuccessful, NetworkStream networkStream = null)
    {
        if (!connectSuccessful)
            throw new Exception("Bad connect to server");

        try
        {

            networkStream.Write(DataForSend, 0, DataForSend.Length);

            byte[] answer = new byte[4];
            networkStream.Read(answer, 0, 4);
            Debug.Log(BitConverter.ToInt32(answer));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
