using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoteFlow.Models
{
    public class Reminder
    {
        public DateTime ReminderExpires;
        public DateTime ReminderCreated;
        public string ReminderContent = "";

        public Reminder(string path)
        {
            ReminderExpires = DateTime.Now;
            ReminderCreated = DateTime.Now;
            ReminderContent = "";
        }
        public string BeatifulCreationDate()
        {
            if (ReminderCreated.Day == DateTime.Now.Day)
                return $"сегодня {ReminderCreated:HH:mm}";

            if (ReminderCreated.Day == DateTime.Now.AddDays(-1).Day)
                return $"вчера {ReminderCreated:HH:mm}";

            if (ReminderCreated.Year == DateTime.Now.Year)
                return $"{ReminderCreated:dd.MM HH:mm}";

            if (ReminderCreated.Year != DateTime.Now.Year)
                return $"{ReminderCreated:dd.MM.yy HH:mm}";
            
            return ReminderCreated.ToString();
        }

        public string BeatifulExpiringDate()
        {
            if (ReminderExpires.Day == DateTime.Now.Day)
                return $"сегодня {ReminderExpires:HH:mm}";

            if (ReminderExpires.Day == DateTime.Now.AddDays(-1).Day)
                return $"вчера {ReminderExpires:HH:mm}";

            if (ReminderExpires.Year == DateTime.Now.Year)
                return $"{ReminderExpires:dd.MM HH:mm}";

            if (ReminderExpires.Year != DateTime.Now.Year)
                return $"{ReminderExpires:dd.MM.yy HH:mm}";
            
            return ReminderExpires.ToString();
        }
    }
}