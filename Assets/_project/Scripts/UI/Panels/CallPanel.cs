using UnityEngine;
using UnityEngine.UI;

public class CallPanel : MonoBehaviour
{
    [SerializeField] private GameObject _incomingPanel;
    [SerializeField] private GameObject _crashPanel;
    [SerializeField] private GameObject _establishingPanel;
    [SerializeField] private GameObject _callingPanel;

    [Space]
    [SerializeField] private Sprite _muteSprite;
    [SerializeField] private Sprite _unmuteSprite;
    [SerializeField] private Sprite _muteSoundSprite;
    [SerializeField] private Sprite _unmuteSoundSprite;

    public void Show(CallState callState)
    {
        gameObject.SetActive(true);
        _callingPanel.SetActive(callState == CallState.Calling);
        _incomingPanel.SetActive(callState == CallState.Incoming);
        _crashPanel.SetActive(callState == CallState.Crash);
        _establishingPanel.SetActive(callState == CallState.Establishing);
    }

    public void Hide()
    {
        var caller = FindAnyObjectByType<VRTeleportation_Caller>();
        caller.StopRecord();
        gameObject.SetActive(false);
    }

    public void SwitchMute(Image image)
    {
        image.sprite = image.sprite != _muteSprite ? _muteSprite : _unmuteSprite;
    }

    public void SwitchSound(Image image)
    {
        image.sprite = image.sprite != _muteSoundSprite ? _muteSoundSprite : _unmuteSoundSprite;
    }
}
