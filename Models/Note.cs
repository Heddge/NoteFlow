using System.Text.RegularExpressions;

namespace NoteFlow.Models;

public class Note
{
    public string NoteTitle = "";
    public string NoteContent = "";
    public readonly DateTime NoteCreated;
    public DateTime NoteEdited;
    public string NotePath;

    public Note(string path)
    {
        NoteTitle = Regex.Match(path, @"(?<=Notes\\{1})(.*)(?=_\d{8}_\d{6}\.md)").ToString();
        NoteContent = File.ReadAllText(path);
        NoteCreated = File.GetCreationTime(path);
        NoteEdited = File.GetLastWriteTime(path);
        NotePath = path;
    }
    // => Path.Combine(_notesPath, $"{noteTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.md");


    public string BeatifulCreationDate()
    {
        if (NoteCreated.Day == DateTime.Now.Day)
            return $"сегодня {NoteCreated:HH:mm}";

        if (NoteCreated.Day == DateTime.Now.AddDays(-1).Day)
            return $"вчера {NoteCreated:HH:mm}";

        if (NoteCreated.Year == DateTime.Now.Year)
            return $"{NoteCreated:dd.MM_HHmm}";

        if (NoteCreated.Year != DateTime.Now.Year)
            return $"{NoteCreated:dd.MM.yy HH:mm}";
        
        return NoteCreated.ToString();
    }
}