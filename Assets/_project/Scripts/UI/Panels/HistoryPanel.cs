using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryPanel : MonoBehaviour
{
    [SerializeField] private HistoryContent _content;
    [SerializeField] private Transform _container;

    private void Awake()
    {
        VRTeleportation_NetworkBehviour.Instance.OnGetModels += CreateHistory;
    }

    private void CreateHistory(List<string> names)
    {
        for (int i = 0; i < _container.childCount; i++)
        {
            Destroy(_container.GetChild(i).gameObject);
        }
        _container.DetachChildren();

        names.Reverse();

        foreach (string name in names)
        {
            var content = Instantiate(_content, _container);
            content.Init(name);
        }
    }
}
