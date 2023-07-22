using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;

public class UnityCloudBuildPostProcessor
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
    {
        // Убедитесь, что сборка предназначена для iOS
        if (buildTarget == BuildTarget.iOS)
        {
            // Путь к файлу Info.plist
            string plistPath = buildPath + "/Info.plist";

            // Создаем объект для работы с файлом Info.plist
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            // Добавляем разрешение для открытия ссылок
            plist.root.SetString("NSAppTransportSecurity", "<dict><key>NSAllowsArbitraryLoads</key><true/></dict>");

            // Записываем изменения обратно в файл Info.plist
            plist.WriteToFile(plistPath);
        }
    }
}
