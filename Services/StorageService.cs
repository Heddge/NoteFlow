// Services/StorageService.cs
using System;
using System.IO;
using System.Text;
using NoteFlow.Models;
using NoteFlow.Pages;

namespace NoteFlow.Services
{
    public class StorageService
    {
        private static readonly UTF8Encoding Utf8WithoutBom = new UTF8Encoding(false);
        public readonly string _storageRootPath;
        public readonly string _notesPath;
        public readonly string _remindersPath;
        public readonly char[] _bannedChars = { '?', '<', '>', ':', '"', '|', '*', '\\', '/' };

        public StorageService()
        {
            _storageRootPath = ResolveStorageRootPath();

            _notesPath = Path.Combine(_storageRootPath, "Notes");
            _remindersPath = Path.Combine(_storageRootPath, "Reminders");

            if (!Directory.Exists(_storageRootPath))
                Directory.CreateDirectory(_storageRootPath);

            // Создаем папки при первом использовании
            if (!Directory.Exists(_notesPath))
                Directory.CreateDirectory(_notesPath);
            if (!Directory.Exists(_remindersPath))
                Directory.CreateDirectory(_remindersPath);

            MigrateLegacyMacStorage();

            Console.WriteLine($"Storage root: {_storageRootPath}");
            Console.WriteLine($"Notes path: {_notesPath}");
            Console.WriteLine($"Reminders path: {_remindersPath}");
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
            System.IO.File.WriteAllText(filePath, content ?? string.Empty, Utf8WithoutBom);

            return filePath;
        }

        public string SaveEditedNote(string title, string content, string path)
        {
            string safeTitle = ToSafetyNoteName(title);
            string currentTitle = ToSafetyNoteName(Note.ExtractTitleFromPath(path));
            string newFilePath = string.Equals(currentTitle, safeTitle, StringComparison.Ordinal)
                ? path
                : GetNotePath(safeTitle);

            try
            {
                // write content first to the target path; this is safer across platforms than write+move
                System.IO.File.WriteAllText(newFilePath, content ?? string.Empty, Utf8WithoutBom);

                if (!PathsEqual(path, newFilePath) && File.Exists(path))
                    File.Delete(path);
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

            string safetyName = new string(oldName
                .Normalize(NormalizationForm.FormC)
                .Select(x => _bannedChars.Contains(x) ? ' ' : x)
                .ToArray());

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
            File.WriteAllText(reminderPath, reminder.ToStorageJson(), Utf8WithoutBom);

            return reminderPath;
        }

        public string SaveEditedReminder(Reminder reminder, string path)
        {
            string safeTitle = ToSafetyReminderName(reminder.ReminderTitle);
            string currentTitle = ToSafetyReminderName(Note.ExtractTitleFromPath(path));
            string newFilePath = string.Equals(currentTitle, safeTitle, StringComparison.Ordinal)
                ? path
                : GetReminderPath(safeTitle);

            try
            {
                File.WriteAllText(newFilePath, reminder.ToStorageJson(), Utf8WithoutBom);

                if (!PathsEqual(path, newFilePath) && File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Reminder save failed: {e.Message}");
            }

            return newFilePath;
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

            var safetyName = new string(oldName
                .Normalize(NormalizationForm.FormC)
                .Select(x => _bannedChars.Contains(x) ? ' ' : x)
                .ToArray());

            return string.IsNullOrWhiteSpace(safetyName) ? "Новое напоминание" : safetyName;
        }

        private static bool PathsEqual(string left, string right)
        {
            StringComparison comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            return string.Equals(left, right, comparison);
        }

        private static string ResolveStorageRootPath()
        {
            if (OperatingSystem.IsMacOS())
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Library",
                    "Application Support",
                    "NoteFlow");
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "NoteFlow");
        }

        private void MigrateLegacyMacStorage()
        {
            if (!OperatingSystem.IsMacOS())
                return;

            string legacyRootPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "NoteFlow");

            if (!Directory.Exists(legacyRootPath) || PathsEqual(legacyRootPath, _storageRootPath))
                return;

            try
            {
                MergeStorageDirectory(Path.Combine(legacyRootPath, "Notes"), _notesPath);
                MergeStorageDirectory(Path.Combine(legacyRootPath, "Reminders"), _remindersPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Legacy macOS storage migration skipped: {e.Message}");
            }
        }

        private static void MergeStorageDirectory(string sourceDirectory, string targetDirectory)
        {
            if (!Directory.Exists(sourceDirectory))
                return;

            Directory.CreateDirectory(targetDirectory);

            foreach (string sourcePath in Directory.GetFiles(sourceDirectory, "*.md"))
            {
                string targetPath = Path.Combine(targetDirectory, Path.GetFileName(sourcePath));
                if (!File.Exists(targetPath))
                {
                    File.Copy(sourcePath, targetPath);
                }
            }
        }
    }
}
