using System;
using UnityEngine;

public class CallPanel : MonoBehaviour
{
    [SerializeField] private GameObject _incomingPanel;
    [SerializeField] private GameObject _crashPanel;
    [SerializeField] private GameObject _establishingPanel;
    [SerializeField] private GameObject _callingPanel;

    public void Show(CallState callState)
    {
        _callingPanel.SetActive(callState == CallState.Calling);
        _incomingPanel.SetActive(callState == CallState.Incoming);
        _crashPanel.SetActive(callState == CallState.Crash);
        _establishingPanel.SetActive(callState == CallState.Establishing);
    }
}
