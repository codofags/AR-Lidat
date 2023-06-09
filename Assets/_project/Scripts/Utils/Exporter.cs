using UnityEngine;
using UnityEditor;
using System.IO;
using Autodesk.Fbx;

public static class Exporter
{
    public static bool ExportFBX(GameObject gameObject, string path)
    {
        // Создаем экземпляр класса FBXExporter
        //var fbxExporter = new Autodesk.Fbx.Eporter();


        //// Экспортируем модель в формат FBX
        //bool success = fbxExporter.Export(gameObject, path);

        //return success;
        return false;
    }
}

public class ExportFBX : MonoBehaviour
{
    [MenuItem("Custom/Export Model to FBX")]
    private static void ExportModelToFBX()
    {
        // Получаем выбранную модель
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
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
        bool success = Exporter.ExportFBX(selectedObject, path);
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

