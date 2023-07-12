using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SerializeTest : MonoBehaviour
{
    public GameObject ForSerialize;
    public Material Mat;
    private byte[] _serialized;

    [ContextMenu("Serialize")]
    public void Serialize()
    {
        var serializer = new ModelSerializer();
        _serialized = serializer.Serialize(ForSerialize);
    }

    [ContextMenu("Deserialize")]
    public void Deserialize()
    {
        var serializer = new ModelSerializer();
        serializer.MaterialForDeserialize = Mat;

        serializer.Deserialize(_serialized, 0);
    }
}
