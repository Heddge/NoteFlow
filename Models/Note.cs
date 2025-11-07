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
}