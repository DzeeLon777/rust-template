using UnityEngine;

public class Example : MonoBehaviour
{
    private readonly string? playerName;
    private readonly int health;
    private readonly bool isAlive;

    private void Start()
    {
        // Намеренные ошибки для демонстрации работы анализатора
        playerNmae = "Player1"; // Опечатка в имени переменной
        helth = 100; // Опечатка в имени переменной
        isAliv = true; // Опечатка в имени переменной

        // Пример с неправильным методом
        InvokeRepeating("UpdateHelth", 0f, 1f); // Опечатка в имени метода

        // Пример с неправильным пространством имен
        System.Collection.Generic.List<string> items = new System.Collection.Generic.List<string>(); // Опечатка в пространстве имен
    }

    /// <summary>
    /// Правильное имя метода
    /// </summary>
    private void UpdateHelth()
    {
        if (helth <= 0) // Опечатка в имени переменной
        {
            isAliv = false; // Опечатка в имени переменной
        }
    }
}
