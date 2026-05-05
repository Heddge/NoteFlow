using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using NoteFlow.Models;
using NoteFlow.Services;

namespace NoteFlow.Pages
{
    public class MainScreenModel : PageModel
    {
        // list of all user`s notes
        public Dictionary<string, Note> NotesDict = new Dictionary<string, Note>();
        public List<Reminder> Reminders = new List<Reminder>();
        public CacheService currentCache;
        public StorageService currentStorage;

        public MainScreenModel()
        {
            currentStorage = new StorageService();
            currentCache = new CacheService(currentStorage._notesPath, currentStorage._remindersPath);
        }
        public void OnGet()
        {
            NotesDict = currentCache.currNotesDict;
            // Notes = CacheService.currNotes;
            Reminders = currentCache.currReminders.OrderBy(x => x.ReminderExpires).ToList();
            
            if (NotesDict.Count() != currentStorage.CountNotesInDirectory())
            {
                currentCache.UpdateMissedNotesInDirToCurrentNotes(NotesDict.Count() < currentStorage.CountNotesInDirectory());
                NotesDict = currentCache.currNotesDict;
            }

            if (Reminders.Count() != currentStorage.CountRemindersInDirectory())
            {
                Reminders = Directory
                    .GetFiles(currentStorage._remindersPath, "*.md")
                    .Select(path => new Reminder(path))
                    .OrderBy(x => x.ReminderExpires)
                    .ToList();

                Reminders = currentCache.currReminders;
            }
        }

        public IActionResult OnPostCreateReminder(string Title, string Description, int Day, int Month, int Year, string Time, bool IsRepeating)
            => SaveReminderInternal(null, Title, Description, Day, Month, Year, Time, IsRepeating);

        public IActionResult OnPostSaveReminder(string OriginalPath, string Title, string Description, int Day, int Month, int Year, string Time, bool IsRepeating)
            => SaveReminderInternal(OriginalPath, Title, Description, Day, Month, Year, Time, IsRepeating);

        private IActionResult SaveReminderInternal(string? originalPath, string Title, string Description, int Day, int Month, int Year, string Time, bool IsRepeating)
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                Title = "Новое напоминание";
            }

            if (!TimeSpan.TryParse(Time, out TimeSpan parsedTime))
                parsedTime = new TimeSpan(12, 0, 0);

            try
            {
                int safeDay = Math.Min(Day, DateTime.DaysInMonth(Year, Month));
                DateTime reminderDate = new DateTime(Year, Month, safeDay, parsedTime.Hours, parsedTime.Minutes, 0);

                bool isEditing = !string.IsNullOrWhiteSpace(originalPath) && System.IO.File.Exists(originalPath);

                if (isEditing)
                {
                    var reminder = new Reminder(originalPath!);
                    reminder.ReminderTitle = Title.Trim();
                    reminder.ReminderDescription = Description?.Trim() ?? "";
                    reminder.ReminderExpires = reminderDate;
                    reminder.IsRepeating = IsRepeating;

                    string savedReminderPath = currentStorage.SaveEditedReminder(reminder, originalPath!);
                    currentCache.UpdateReminderInCurrentReminders(savedReminderPath, originalPath!);
                }
                else
                {
                    var reminder = new Reminder(
                        Title.Trim(),
                        Description?.Trim() ?? "",
                        reminderDate,
                        IsRepeating
                    );

                    string savedReminderPath = currentStorage.SaveReminder(reminder);
                    currentCache.AddReminderToCurrentReminders(savedReminderPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Reminder save failed: {e.Message}");
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDeleteReminder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return RedirectToPage();

            currentStorage.DeleteReminder(path);
            currentCache.DeleteReminderFromCurrentReminders(path);

            return RedirectToPage();
        }

        // method for redirecting to NoteScreen to create or edit note
        public IActionResult OnPostToEditorForEditOrCreate(string path)
        {
            // if user needs to CREATE note
            if (string.IsNullOrEmpty(path))
                return RedirectToPage("NoteScreen", new { notePath = "a" });

            // if user needs to EDIT note
            return RedirectToPage("NoteScreen", new { notePath = path });
        }
    }
}
