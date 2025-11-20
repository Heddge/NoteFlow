// Services/StorageService.cs
using System;
using System.IO;
using NoteFlow.Models;
using NoteFlow.Pages;

namespace NoteFlow.Services
{
    public class StorageService
    {
        private static readonly string _myDocumentsPath;
        public static readonly string _notesPath;
        public static readonly char[] _bannedChars = { '?', '<', '>', ':', '"', '|', '*', '\\', '/' };

        static StorageService()
        {
            _myDocumentsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "NoteFlow");

            _notesPath = Path.Combine(_myDocumentsPath, "Notes");

            // Создаем папки при первом использовании
            if (!Directory.Exists(_notesPath))
                Directory.CreateDirectory(_notesPath);

            Console.WriteLine($"Storage initialized: {_notesPath}");
        }

        private static string GetPath(string noteTitle)
            => Path.Combine(_notesPath, $"{noteTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.md");
            
        
        public static void SaveNewNote(string title, string content)
        {
            // creating new filepath
            var filePath = GetPath(ToSafetyName(title));

            // put content into the current note
            System.IO.File.WriteAllText(filePath, content);
            CurrentStorage.AddNoteToCurrentNotes(filePath);

        }

        public static string SaveEditedNote(string title, string content, string path)
        {
            // new path with new (if user changed it) name
            string newFilePath = GetPath(ToSafetyName(title));

            try
            {
                // put content into the current note and renaming file
                System.IO.File.WriteAllText(path, content);

                CurrentStorage.UpdateNoteInCurrentNotes(title, content,newFilePath, path);

                System.IO.File.Move(path, newFilePath);


            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (NotSupportedException e)
            {
                Console.WriteLine(e.Message);
            }

            return newFilePath;
            }

        private static string ToSafetyName(string oldName)
        {
            if (string.IsNullOrEmpty(oldName) || string.IsNullOrWhiteSpace(oldName))
                return "Новая заметка";

            string safetyName = new string(oldName.Select(x => _bannedChars.Contains(x) ? ' ' : x).ToArray());

            return string.IsNullOrEmpty(safetyName) || string.IsNullOrWhiteSpace(safetyName) ? "Новая заметка" : safetyName;
        }
    }
}