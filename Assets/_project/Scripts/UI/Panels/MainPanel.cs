using UnityEngine;
using UnityEngine.UI;

namespace ScanAR.UI
{
    public class MainPanel : MonoBehaviour
    {
        [SerializeField] private Button _recordBtn;
        [SerializeField] private Button _stopBtn;
        [SerializeField] private Slider _sliderFOV;


        private void Awake()
        {
            _recordBtn.onClick.AddListener(OnRecordBtnClick);
            _stopBtn.onClick.AddListener(OnStopBtnClick);
            _sliderFOV.onValueChanged.AddListener(SetFOV);
        }

        private void OnDestroy()
        {
            _recordBtn.onClick.RemoveAllListeners();
            _stopBtn.onClick.RemoveAllListeners();
            _sliderFOV.onValueChanged.RemoveListener(SetFOV);
        }

        private void OnRecordBtnClick()
        {
            //todo Начать скан местности
            ScanController.Instance.ScanStart();
            SwitchBtns();
        }

        private void OnStopBtnClick()
        {
            //todo Остановить скан
            ScanController.Instance.ScanStop();
            SwitchBtns();
        }

        private void SwitchBtns()
        {
            _recordBtn.gameObject.SetActive(!_recordBtn.gameObject.activeSelf);
            _stopBtn.gameObject.SetActive(!_stopBtn.gameObject.activeSelf);
        }

        private void SetFOV(float value)
        {
            Camera.main.fieldOfView = value;
        }
    }
}
