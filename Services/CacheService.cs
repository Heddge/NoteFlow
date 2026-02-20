using Microsoft.AspNetCore.Mvc;
using NoteFlow.Models;
using NoteFlow.Pages;

namespace NoteFlow.Services;
public class CacheService
{
    public static Dictionary<string, Note> currNotesDict = new Dictionary<string, Note>();
    public static List<Reminder> currReminders = new List<Reminder>();
    private static string notesPath = StorageService._notesPath;
    // private static string remindersPath = StorageService._remindersPath;
    static CacheService()
    {
        // parsing notes from path
        foreach (string path in Directory.GetFiles(notesPath, "*.md"))
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
    }

    public static void UpdateNoteInCurrentNotes(string title, string content, string newPath, string Path)
    {
        if (currNotesDict.ContainsKey(Path))
            try
            {
                Note temp = currNotesDict[Path];
                temp.NoteContent = content;
                temp.NoteTitle = title;
                temp.NoteEdited = DateTime.Now;
                temp.NotePath = newPath;
                currNotesDict = currNotesDict.OrderByDescending(s => s.Value.NoteEdited).ToDictionary();
            }
            catch (NullReferenceException ee)
            {
                Console.WriteLine($"Something went wrong: {ee.Message}");
            }
    }

    public static void DeleteNoteFromCurrentNotes(string Path) =>
        currNotesDict.Remove(Path);

    public static void AddNoteToCurrentNotes(string Path)
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
            catch (IOException eee)
            {
                Console.WriteLine($"Something went wrong: {eee.Message}");
            }
    }

    private static string[] getNotesPaths() =>
        currNotesDict.Keys.ToArray();

    public void UpdateMissedNotesInDirToCurrentNotes(bool flag)
    {
        if (flag)
        {
            string[] addedPaths = Directory.GetFiles(notesPath, "*.md").Where(x => !currNotesDict.ContainsKey(x)).ToArray();
            foreach (string path in addedPaths)
                AddNoteToCurrentNotes(path);
            return;
        }

        string[] deletedPaths = getNotesPaths().Where(x => !Directory.GetFiles(notesPath, "*.md").Contains(x)).ToArray();
        
        foreach (string path in deletedPaths)
            DeleteNoteFromCurrentNotes(path);
    }
}