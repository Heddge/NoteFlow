using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using NoteFlow.Models;
using NoteFlow.Services;

namespace NoteFlow.Pages
{
    public class MainScreenModel : PageModel
    {
        // list of all user`s notes
        public List<Note> Notes = new List<Note>();
        public List<Reminder> Reminders = new List<Reminder>();
        public CacheService currentCache = new CacheService();

        public void OnGet()
        {
            Notes = CacheService.currNotes;
            Reminders = CacheService.currReminders.OrderBy(x => x.ReminderExpires).ToList();
            
            if (Notes.Count() != StorageService.CountNotesInDirectory())
            {
                currentCache.UpdateMissedNotesInDirToCurrentNotes(Notes.Count() < StorageService.CountNotesInDirectory());
                Notes = CacheService.currNotes;
            }

            if (Reminders.Count() != StorageService.CountRemindersInDirectory())
            {
                CacheService.currReminders = Directory
                    .GetFiles(StorageService._remindersPath, "*.md")
                    .Select(path => new Reminder(path))
                    .OrderBy(x => x.ReminderExpires)
                    .ToList();

                Reminders = CacheService.currReminders;
            }
        }

        public IActionResult OnPostCreateReminder(string Title, string Description, int Day, int Month, int Year, string Time, bool IsRepeating)
        {
            if (string.IsNullOrWhiteSpace(Title))
                return RedirectToPage();

            if (!TimeSpan.TryParse(Time, out TimeSpan parsedTime))
                parsedTime = new TimeSpan(12, 0, 0);

            try
            {
                int safeDay = Math.Min(Day, DateTime.DaysInMonth(Year, Month));
                DateTime reminderDate = new DateTime(Year, Month, safeDay, parsedTime.Hours, parsedTime.Minutes, 0);

                var reminder = new Reminder(
                    Title.Trim(),
                    Description?.Trim() ?? "",
                    reminderDate,
                    IsRepeating
                );

                string savedReminderPath = StorageService.SaveReminder(reminder);
                CacheService.AddReminderToCurrentReminders(savedReminderPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Reminder creation failed: {e.Message}");
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDeleteReminder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return RedirectToPage();

            StorageService.DeleteReminder(path);
            CacheService.DeleteReminderFromCurrentReminders(path);

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
