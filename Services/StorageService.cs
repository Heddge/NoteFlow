// Services/StorageService.cs
using System;
using System.IO;
using NoteFlow.Models;

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

        private static string GetPath(string noteTitle)
            => Path.Combine(_notesPath, $"{noteTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.md");
            
        
        public static void SaveNewNote(string title, string content)
        {
            // creating new filepath
            var filePath = GetPath(title);

            // put content into the current note
            System.IO.File.WriteAllText(filePath, content);
        }

        public static string SaveEditedNote(string title, string content, string path)
        {
            // new path with new (if user changed it) name
            string newFilePath = GetPath(title);

            // put content into the current note and renaming file
            System.IO.File.WriteAllText(path, content);
            System.IO.File.Move(path, newFilePath);

            return newFilePath;
        }

    }
}