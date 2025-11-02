// Services/StorageService.cs
using System;
using System.IO;

namespace NoteFlow.Services
{
    public static class StorageService
    {
        private static readonly string _myDocumentsPath;
        public static readonly string _notesPath;

        static StorageService()
        {
            _myDocumentsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "NoteFlow");
            
            _notesPath = Path.Combine(_myDocumentsPath, "Notes");
            
            // Создаем папки при первом использовании
            Directory.CreateDirectory(_notesPath);
            
            Console.WriteLine($"Storage initialized: {_notesPath}");
        }

        public static string SaveNote(string content, string title)
        {
            try
            {
                string fileName = "";

                if (!string.IsNullOrEmpty(title) || !string.IsNullOrWhiteSpace(title))
                    fileName = $"{title}";
                else
                    fileName = "Новая заметка";
                
                var filePath = Path.Combine(_notesPath, fileName);
                File.WriteAllText(filePath, content);
                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сохранения: {ex.Message}");
            }
        }
    }
}