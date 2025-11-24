using NoteFlow.Models;
using NoteFlow.Pages;

namespace NoteFlow.Services;
public class CacheService
{
    public static List<Note> currNotes = new List<Note>();
    private static string path = StorageService._notesPath;
    static CacheService()
    {
        // parsing notes from path
        foreach (string path in Directory.GetFiles(path, "*.md"))
        {
            currNotes.Add(new Note(path));
        }
        currNotes = currNotes.OrderByDescending(s => s.NoteEdited).ToList();
    }

    public static void UpdateNoteInCurrentNotes(string title, string content, string newPath, string Path)
    {
        int neededNote = currNotes.FindIndex(x => x.NotePath == Path);

        if (neededNote != -1)
            try
            {
                Note temp = currNotes[neededNote];
                temp.NoteContent = content;
                temp.NoteTitle = title;
                temp.NoteEdited = DateTime.Now;
                temp.NotePath = newPath;
                currNotes[neededNote] = temp;
                currNotes = currNotes.OrderByDescending(s => s.NoteEdited).ToList();
            }
            catch (NullReferenceException ee)
            {
                Console.WriteLine($"Something went wrong: {ee.Message}");
            }
    }

    public static void DeleteNoteFromCurrentNotes(string Path) =>
        currNotes = currNotes.Where(x => x.NotePath != Path).OrderByDescending(x => x.NoteEdited).ToList();

    public static void AddNoteToCurrentNotes(string Path)
    {
        Console.WriteLine("Entered in Add method");
        if (currNotes.FindIndex(x => x.NotePath == Path) == -1)
            try
            {
                Console.WriteLine("Adding");
                currNotes.Add(new Note(Path));
                currNotes[currNotes.Count()-1].NoteEdited = DateTime.Now;
                currNotes = currNotes.OrderByDescending(s => s.NoteEdited).ToList();
            }
            catch (IOException eee)
            {
                Console.WriteLine($"Something went wrong: {eee.Message}");
            }
    }

    private static string[] getNotesTitles() =>
        currNotes.Select(x => x.NoteTitle).ToArray();

        
    private static string[] getNotesPaths() =>
        currNotes.Select(x => x.NotePath).ToArray();

    public static void UpdateMissedNotesInDirToCurrentNotes(bool flag)
    {
        string[] paths = Directory.GetFiles(path, "*.md").Where(x => !getNotesPaths().Contains(x)).ToArray();

        foreach (string path in paths)
            AddNoteToCurrentNotes(path);
    }
}