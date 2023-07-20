using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HistoryContent : MonoBehaviour
{
    [SerializeField] private TMP_Text _name;

    internal void Init(string name)
    {
        _name.text = name;
    }
}
