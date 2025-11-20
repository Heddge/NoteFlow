using NoteFlow.Models;

namespace NoteFlow.Services;

public class CurrentStorage
{
    public static List<Note> currNotes = new List<Note>();
    private static string path = StorageService._notesPath;

    static CurrentStorage()
    {
        Console.WriteLine("PARSING PARSING PARSING PARSING PARSING");
        // parsing notes from path
        foreach (string path in Directory.GetFiles(path, "*.md"))
        {
            currNotes.Add(new Note(path));
        }
        currNotes = currNotes.OrderByDescending(s => s.NoteEdited).ToList();
    }

    public static void UpdateNoteInCurrentNotes(string title, string content, string newPath, string Path)
    {
        if (currNotes.FindIndex(x => x.NotePath == Path) != -1)
            try
            {
                currNotes.Find(x => x.NotePath == Path).NoteContent = content;
                currNotes.Find(x => x.NotePath == Path).NoteTitle = title;
                currNotes.Find(x => x.NotePath == Path).NoteEdited = DateTime.Now;
                currNotes.Find(x => x.NotePath == Path).NotePath = newPath;
                currNotes = currNotes.OrderByDescending(s => s.NoteEdited).ToList();
                return;
            }
            catch (NullReferenceException ee)
            {
                Console.WriteLine("Something went wrong.");
            }
    }

    public static void DeleteNoteFromCurrentNotes(string Path) =>
        currNotes = currNotes.Where(x => x.NotePath != Path).OrderByDescending(x => x.NoteEdited).ToList();

    public static void AddNoteToCurrentNotes(string Path)
    {
        if (currNotes.FindIndex(x => x.NotePath == Path) == -1)
        try
        {
            currNotes.Add(new Note(Path));
            currNotes[currNotes.Count()-1].NoteEdited = DateTime.Now;
            currNotes = currNotes.OrderByDescending(s => s.NoteEdited).ToList();
        }
        catch (IOException eee)
        {
            Console.WriteLine("Something went wrong.");
        }
    }
}