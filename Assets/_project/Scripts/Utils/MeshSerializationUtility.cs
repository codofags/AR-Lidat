using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class MeshSerializationUtility
{
    public static byte[] SerializeMesh(Mesh mesh)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream memoryStream = new MemoryStream())
        {
            formatter.Serialize(memoryStream, mesh);
            return memoryStream.ToArray();
        }
    }

    public static Mesh DeserializeMesh(byte[] data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream memoryStream = new MemoryStream(data))
        {
            return (Mesh)formatter.Deserialize(memoryStream);
        }
    }
}
