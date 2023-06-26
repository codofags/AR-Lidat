using UnityEngine;

public static class Extensions
{
    public static Color GetRandomColor()
    {
        // Генерируем случайные значения для красного, зеленого и синего каналов
        float red = Random.Range(0f, 1f);
        float green = Random.Range(0f, 1f);
        float blue = Random.Range(0f, 1f);

        // Создаем новый цвет на основе сгенерированных значений
        Color randomColor = new Color(red, green, blue);

        return randomColor;
    }
}