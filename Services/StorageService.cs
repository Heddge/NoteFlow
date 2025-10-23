// Services/StorageService.cs
using System;
using System.IO;

namespace NoteFlow.Services
{
    public static class StorageService
    {
        private static readonly string _appDataPath;
        private static readonly string _notesPath;

        static StorageService()
        {
            // ⭐ ГАРАНТИРОВАННЫЙ ПУТЬ В ПАПКЕ APP DATA ⭐
            _appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "NoteFlow");
            
            _notesPath = Path.Combine(_appDataPath, "Notes");
            
            // Создаем папки при первом использовании
            Directory.CreateDirectory(_notesPath);
            
            Console.WriteLine($"Storage initialized: {_notesPath}");
        }

        public static string SaveNote(string content)
        {
            try
            {
                var fileName = $"note_{DateTime.Now:ddMMyyyy_HHmmss}.txt";
                var filePath = Path.Combine(_notesPath, fileName);
                
                File.WriteAllText(filePath, content);
                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сохранения: {ex.Message}");
            }
        }

        public static string GetStorageInfo()
        {
            return $"Файлы сохраняются в: {_notesPath}";
        }
    }
}