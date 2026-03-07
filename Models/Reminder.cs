using System.Globalization;
using System.Text.Json;

namespace NoteFlow.Models
{
    public class Reminder
    {
        public string ReminderTitle { get; set; } = "";
        public string ReminderDescription { get; set; } = "";
        public DateTime ReminderExpires { get; set; } = DateTime.Now.AddHours(1);
        public DateTime ReminderCreated { get; set; } = DateTime.Now;
        public bool IsRepeating { get; set; }
        public string ReminderPath { get; set; } = "";

        public Reminder(string path)
        {
            ReminderPath = path;

            if (!File.Exists(path))
                return;

            try
            {
                var payload = JsonSerializer.Deserialize<ReminderStoragePayload>(File.ReadAllText(path));

                if (payload is null)
                    return;

                ReminderTitle = payload.Title ?? "";
                ReminderDescription = payload.Description ?? "";
                ReminderExpires = payload.ExpiresAt;
                ReminderCreated = payload.CreatedAt == default ? File.GetCreationTime(path) : payload.CreatedAt;
                IsRepeating = payload.IsRepeating;
            }
            catch (JsonException)
            {
                // Fallback for legacy text-based reminders.
                ReminderTitle = Path.GetFileNameWithoutExtension(path);
                ReminderDescription = File.ReadAllText(path);
                ReminderCreated = File.GetCreationTime(path);
                ReminderExpires = File.GetLastWriteTime(path);
            }
        }

        public Reminder(string title, string description, DateTime expiresAt, bool isRepeating)
        {
            ReminderTitle = title;
            ReminderDescription = description;
            ReminderExpires = expiresAt;
            ReminderCreated = DateTime.Now;
            IsRepeating = isRepeating;
        }

        public string ToStorageJson()
        {
            var payload = new ReminderStoragePayload
            {
                Title = ReminderTitle,
                Description = ReminderDescription,
                ExpiresAt = ReminderExpires,
                CreatedAt = ReminderCreated,
                IsRepeating = IsRepeating
            };

            return JsonSerializer.Serialize(payload);
        }

        public string BeatifulCreationDate()
        {
            if (ReminderCreated.Date == DateTime.Now.Date)
                return $"сегодня {ReminderCreated:HH:mm}";

            if (ReminderCreated.Date == DateTime.Now.Date.AddDays(-1))
                return $"вчера {ReminderCreated:HH:mm}";

            if (ReminderCreated.Year == DateTime.Now.Year)
                return $"{ReminderCreated:dd.MM HH:mm}";

            return $"{ReminderCreated:dd.MM.yy HH:mm}";
        }

        public string BeatifulExpiringDate()
        {
            if (ReminderExpires.Date == DateTime.Now.Date)
                return $"сегодня {ReminderExpires:HH:mm}";

            if (ReminderExpires.Date == DateTime.Now.Date.AddDays(1))
                return $"завтра {ReminderExpires:HH:mm}";

            if (ReminderExpires.Year == DateTime.Now.Year)
                return $"{ReminderExpires:dd.MM HH:mm}";

            return $"{ReminderExpires:dd.MM.yy HH:mm}";
        }

        public string GetExpiringDayLabel()
        {
            if (ReminderExpires.Date == DateTime.Now.Date)
                return "Сегодня";

            if (ReminderExpires.Date == DateTime.Now.Date.AddDays(1))
                return "Завтра";

            return ReminderExpires.ToString("dd MMM", new CultureInfo("ru-RU"));
        }

        public string GetExpiringTimeLabel() => ReminderExpires.ToString("HH:mm");
    }

    public class ReminderStoragePayload
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRepeating { get; set; }
    }
}
