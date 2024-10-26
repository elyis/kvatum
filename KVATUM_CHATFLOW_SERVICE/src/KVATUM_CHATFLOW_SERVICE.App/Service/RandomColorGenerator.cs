using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace KVATUM_CHATFLOW_SERVICE.App.Service
{
    public class RandomColorGenerator
    {
        private static readonly string[] Colors = new[]
        {
            "FFFFFF", // Белый
            "000000", // Чёрный
            "333333", // Тёмно-серый
            "555555", // Более светлый серый
            "007ACC", // Синий
            "228B22", // Зелёный
            "FF4500", // Красный оранжевый
            "8A2BE2", // Пурпурный
            "483D8B", // Тёмный синий
            "008080", // Бирюзовый
            "2F4F4F", // Тёмно-серый с лёгким оттенком зелёного
            "708090", // Серо-голубой
            "FF69B4", // Ярко-розовый
            "FFD700"  // Золотистый
        };

        public static string GetRandomColor()
        {
            var random = new Random();
            return Colors[random.Next(Colors.Length)];
        }
    }
}