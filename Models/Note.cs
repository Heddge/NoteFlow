using System.Text.RegularExpressions;

namespace NoteFlow.Models;

public class Note
{
    public string NoteTitle = "";
    public string NoteContent = "";
    public string NotePath;
    public readonly DateTime NoteCreated;
    public DateTime NoteEdited;

    public Note(string path)
    {
        NoteTitle = Regex.Match(path, @"(?<=Notes\\{1})(.*)(?=_\d{8}_\d{6}\.md)").ToString();
        NoteContent = File.ReadAllText(path);
        NoteCreated = File.GetCreationTime(path);
        NoteEdited = DateTime.Now;
        NotePath = path;
    }
}