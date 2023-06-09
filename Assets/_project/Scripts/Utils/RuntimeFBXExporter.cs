using UnityEngine;
using UnityEditor;
using System.IO;

public class RuntimeFBXExporter : MonoBehaviour
{
    private GameObject objectToExport;

    private void Awake()
    {
        objectToExport = gameObject;
    }

    public void ExportToFBX()
    {
        if (objectToExport == null)
        {
            Debug.Log("Не выбрана модель для экспорта.");
            return;
        }

        // Запрашиваем путь сохранения файла FBX
        string path = EditorUtility.SaveFilePanel("Export FBX", "", "model", "fbx");
        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("Экспорт отменен.");
            return;
        }

        // Создаем папку для экспорта, если она не существует
        string folderPath = Path.GetDirectoryName(path);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Экспортируем модель в формат FBX
        bool success = Exporter.ExportFBX(objectToExport, path);
        if (success)
        {
            Debug.Log("Модель успешно экспортирована в формат FBX: " + path);
        }
        else
        {
            Debug.Log("Не удалось экспортировать модель в формат FBX.");
        }
    }
}
