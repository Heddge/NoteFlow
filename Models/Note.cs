using System.Text.RegularExpressions;

namespace NoteFlow.Models;

public class Note
{
    public readonly string NoteTitle = "";
    public readonly string NoteContent = "";
    public readonly DateTime NoteCreated;
    public readonly DateTime NoteEdited;

    public Note(string path)
    {
        NoteTitle = Regex.Match(path, @"(?<=Notes\\{1})(.*)(?=_\d{8}_\d{6}\.md)").ToString();
        NoteContent = File.ReadAllText(path);
        NoteCreated = File.GetCreationTime(path);
    }
}