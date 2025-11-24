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
        public static readonly string _remindersPath;
        public static readonly char[] _bannedChars = { '?', '<', '>', ':', '"', '|', '*', '\\', '/' };

        static StorageService()
        {
            _myDocumentsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "NoteFlow");

            _notesPath = Path.Combine(_myDocumentsPath, "Notes");
            _remindersPath = Path.Combine(_myDocumentsPath, "Reminders");

            // Создаем папки при первом использовании
            if (!Directory.Exists(_notesPath))
                Directory.CreateDirectory(_notesPath);
            if (!Directory.Exists(_remindersPath))
                Directory.CreateDirectory(_remindersPath);

            Console.WriteLine($"Storage initialized: {_notesPath}");
            Console.WriteLine($"Storage initialized: {_remindersPath}");
            // Console.WriteLine($"Count notes in Directory: {CountFilesInDirectory()}");
        }

        private static string GetNotePath(string noteTitle)
            => Path.Combine(_notesPath, $"{noteTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.md");
            
        
        public static void SaveNewNote(string title, string content)
        {
            // creating new filepath
            var filePath = GetNotePath(ToSafetyNoteName(title));

            // put content into the current note
            System.IO.File.WriteAllText(filePath, content);
            CacheService.AddNoteToCurrentNotes(filePath);

        }

        public static string SaveEditedNote(string title, string content, string path)
        {
            // new path with new (if user changed it) name
            string newFilePath = GetNotePath(ToSafetyNoteName(title));

            try
            {
                // put content into the current note and renaming file
                System.IO.File.WriteAllText(path, content);

                CacheService.UpdateNoteInCurrentNotes(title, content, newFilePath, path);

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

        private static string ToSafetyNoteName(string oldName)
        {
            if (string.IsNullOrWhiteSpace(oldName))
                return "Новая заметка";

            string safetyName = new string(oldName.Select(x => _bannedChars.Contains(x) ? ' ' : x).ToArray());

            return string.IsNullOrWhiteSpace(safetyName) ? "Новая заметка" : safetyName;
        }

        public static int CountNotesInDirectory()
            => Directory.GetFiles(_notesPath, "*.md").Count();

        public static void GenerateNotesDB()
        {
            string[] temp = CacheService.currNotes.Select(
                x => new string(x.GetId() + '?' + x.NoteTitle + '?' + x.NotePath + '?' + x.NoteCreated + '?' + x.NoteEdited + '?' + x.NoteContent.Substring(0,30))).ToArray();
            File.WriteAllLines(Path.Combine(_notesPath, "notesdb.txt"), temp);
        }
    }
}