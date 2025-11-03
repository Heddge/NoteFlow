using System.Text.RegularExpressions;

namespace NoteFlow.Models;

public class Note
{
    public readonly string NoteTitle = "";
    public readonly string NoteContent = "";
    public readonly DateTime NoteCreated = DateTime.Now;

    public Note(string path)
    {
        // C:\Users\Mi\Documents\NoteFlow\Notes\ddd_20251031_215330.md

        NoteTitle = Regex.Match(path, @"(?<=Notes\\{1})(.*)(?=_\d{8}_\d{6}\.md)").ToString();

        NoteContent = File.ReadAllText(path);
    }
}