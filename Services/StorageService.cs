// Services/StorageService.cs
using System;
using System.IO;
using NoteFlow.Models;
using NoteFlow.Pages;

namespace NoteFlow.Services
{
    public class StorageService
    {
        public readonly string _myDocumentsPath;
        public readonly string _notesPath;
        public readonly string _remindersPath;
        public readonly char[] _bannedChars = { '?', '<', '>', ':', '"', '|', '*', '\\', '/' };

        public StorageService()
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
        public string GetNotePath(string noteTitle)
            => Path.Combine(_notesPath, $"{noteTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.md");

        private string GetReminderPath(string reminderTitle)
            => Path.Combine(_remindersPath, $"{reminderTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.md");
            
        
        public string SaveNewNote(string title, string content)
        {
            // creating new filepath
            var filePath = GetNotePath(ToSafetyNoteName(title));

            // put content into the current note
            System.IO.File.WriteAllText(filePath, content);

            return filePath;
        }

        public string SaveEditedNote(string title, string content, string path)
        {
            // new path with new (if user changed it) name
            string newFilePath = GetNotePath(ToSafetyNoteName(title));

            try
            {
                // put content into the current note and renaming file
                System.IO.File.WriteAllText(path, content);

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

        private string ToSafetyNoteName(string oldName)
        {
            if (string.IsNullOrWhiteSpace(oldName))
                return "Новая заметка";

            string safetyName = new string(oldName.Select(x => _bannedChars.Contains(x) ? ' ' : x).ToArray());

            return string.IsNullOrWhiteSpace(safetyName) ? "Новая заметка" : safetyName;
        }

        public int CountNotesInDirectory()
            => Directory.GetFiles(_notesPath, "*.md").Count();

        // public static void GenerateNotesDB()
        // {
        //     string[] temp = CacheService.currNotesDict.Select(
        //         x => new string(x.GetId() + '?' + x.NoteTitle + '?' + x.NotePath + '?' + x.NoteCreated + '?' + x.NoteEdited + '?' + x.NoteContent.Substring(0,30))).ToArray();
        //     File.WriteAllLines(Path.Combine(_notesPath, "notesdb.txt"), temp);
        // }
        public int CountRemindersInDirectory()
            => Directory.GetFiles(_remindersPath, "*.md").Count();

        public string SaveReminder(Reminder reminder)
        {
            var reminderPath = GetReminderPath(ToSafetyReminderName(reminder.ReminderTitle));
            File.WriteAllText(reminderPath, reminder.ToStorageJson());

            return reminderPath;
        }

        public void DeleteReminder(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Reminder delete failed: {e.Message}");
            }
        }

        // public void GenerateNotesDB()
        // {
        //     string[] temp = cache.currNotes.Select(
        //         x => new string(x.GetId() + '?' + x.NoteTitle + '?' + x.NotePath + '?' + x.NoteCreated + '?' + x.NoteEdited + '?' + x.NoteContent.Substring(0,30))).ToArray();
        //     File.WriteAllLines(Path.Combine(_notesPath, "notesdb.txt"), temp);
        // }

        private string ToSafetyReminderName(string oldName)
        {
            if (string.IsNullOrWhiteSpace(oldName))
                return "Новое напоминание";

            var safetyName = new string(oldName.Select(x => _bannedChars.Contains(x) ? ' ' : x).ToArray());

            return string.IsNullOrWhiteSpace(safetyName) ? "Новое напоминание" : safetyName;
        }
    }
}
