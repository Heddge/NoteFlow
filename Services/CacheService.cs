using Microsoft.AspNetCore.Mvc;
using NoteFlow.Models;
using NoteFlow.Pages;

namespace NoteFlow.Services;
public class CacheService
{
    public Dictionary<string, Note> currNotesDict = new Dictionary<string, Note>();
    public List<Reminder> currReminders = new List<Reminder>();
    private readonly string _notesPath;
    private readonly string _remindersPath;

    public CacheService(string notesPath, string remindersPath)
    {
        _notesPath = notesPath;
        _remindersPath = remindersPath;

        // parsing notes from path
        foreach (string path in Directory.GetFiles(_notesPath, "*.md"))
        {
            currNotesDict.Add(path, new Note(path));
        }
        currNotesDict = currNotesDict.OrderByDescending(s => s.Value.NoteEdited).ToDictionary();

        // // parsing remindes from path
        // foreach (string path in Directory.GetFiles(remindersPath, "*.md"))
        // {
        //     currReminders.Add(new Reminder(path));
        // }
        // currNotes = currNotes.OrderByDescending(s => s.NoteEdited).ToList();
        // parsing notes from path
        foreach (string path in Directory.GetFiles(_remindersPath, "*.md"))
        {
            currReminders.Add(new Reminder(path));
        }
        currReminders = currReminders.OrderBy(s => s.ReminderExpires).ToList();
    }

    public void UpdateNoteInCurrentNotes(string title, string content, string newPath, string oldPath)
    {
        if (!currNotesDict.TryGetValue(oldPath, out Note? temp))
            return;

        try
        {
            temp.NoteContent = content;
            temp.NoteTitle = Note.ExtractTitleFromPath(newPath);
            temp.NoteEdited = DateTime.Now;
            temp.NotePath = newPath;

            if (!PathsEqual(oldPath, newPath))
            {
                currNotesDict.Remove(oldPath);
                currNotesDict[newPath] = temp;
            }

            currNotesDict = currNotesDict
                .OrderByDescending(s => s.Value.NoteEdited)
                .ToDictionary();
        }
        catch (NullReferenceException ee)
        {
            Console.WriteLine($"Something went wrong: {ee.Message}");
        }
    }

    public void DeleteNoteFromCurrentNotes(string Path) =>
        currNotesDict.Remove(Path);

    public void AddNoteToCurrentNotes(string Path)
    {
        Console.WriteLine("Entered in Add method");
        if (!currNotesDict.ContainsKey(Path))
            try
            {
                Console.WriteLine("Adding");
                currNotesDict.Add(Path, new Note(Path));
                currNotesDict[Path].NoteEdited = DateTime.Now;
                currNotesDict = currNotesDict.OrderByDescending(s => s.Value.NoteEdited).ToDictionary();
            }
            catch (Exception eee)
            {
                Console.WriteLine($"Something went wrong: {eee.Message}");
            }
    }

    private string[] getNotesPaths() =>
        currNotesDict.Keys.ToArray();
    public void AddReminderToCurrentReminders(string path)
    {
        if (currReminders.Any(x => x.ReminderPath == path))
            return;

        try
        {
            currReminders.Add(new Reminder(path));
            currReminders = currReminders.OrderBy(x => x.ReminderExpires).ToList();
        }
        catch (IOException e)
        {
            Console.WriteLine($"Something went wrong: {e.Message}");
        }
    }

    public void DeleteReminderFromCurrentReminders(string path) =>
        currReminders = currReminders
            .Where(x => !PathsEqual(x.ReminderPath, path))
            .OrderBy(x => x.ReminderExpires)
            .ToList();

    public void UpdateReminderInCurrentReminders(string newPath, string oldPath)
    {
        currReminders = currReminders
            .Where(x => !PathsEqual(x.ReminderPath, oldPath) && !PathsEqual(x.ReminderPath, newPath))
            .ToList();

        if (File.Exists(newPath))
        {
            currReminders.Add(new Reminder(newPath));
        }

        currReminders = currReminders
            .OrderBy(x => x.ReminderExpires)
            .ToList();
    }

    public void UpdateMissedNotesInDirToCurrentNotes(bool flag)
    {
        if (flag)
        {
            string[] addedPaths = Directory.GetFiles(_notesPath, "*.md").Where(x => !currNotesDict.ContainsKey(x)).ToArray();
            foreach (string path in addedPaths)
                AddNoteToCurrentNotes(path);
            return;
        }

        string[] deletedPaths = getNotesPaths().Where(x => !Directory.GetFiles(_notesPath, "*.md").Contains(x)).ToArray();
        
        foreach (string path in deletedPaths)
            DeleteNoteFromCurrentNotes(path);
    }

    private static bool PathsEqual(string left, string right)
    {
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return string.Equals(left, right, comparison);
    }
}
