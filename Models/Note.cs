using System.Text.RegularExpressions;
using System.Text;

namespace NoteFlow.Models;

public class Note
{
    public string NoteTitle = "";
    public string NoteContent = "";
    public readonly DateTime NoteCreated;
    public DateTime NoteEdited;
    public string NotePath;
    public Guid NoteId;

    public Note(string path)
    {
        NoteTitle = ExtractTitleFromPath(path);
        NoteContent = File.ReadAllText(path);
        NoteCreated = File.GetCreationTime(path);
        NoteEdited = File.GetLastWriteTime(path);
        NotePath = path;
    }

    public static string ExtractTitleFromPath(string path)
    {
        string fileName = Path.GetFileNameWithoutExtension(path);
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        Match match = Regex.Match(fileName, @"^(?<title>.+?)_\d{8}_\d{6}$");
        string extractedTitle = match.Success ? match.Groups["title"].Value : fileName;

        return extractedTitle.Normalize(NormalizationForm.FormC);
    }


    public string BeatifulCreationDate()
    {
        if (NoteCreated.Day == DateTime.Now.Day)
            return $"сегодня {NoteCreated:HH:mm}";

        if (NoteCreated.Day == DateTime.Now.AddDays(-1).Day)
            return $"вчера {NoteCreated:HH:mm}";

        if (NoteCreated.Year == DateTime.Now.Year)
            return $"{NoteCreated:dd.MM HH:mm}";

        if (NoteCreated.Year != DateTime.Now.Year)
            return $"{NoteCreated:dd.MM.yy HH:mm}";
        
        return NoteCreated.ToString();
    }

    public string GetId()
    {
        NoteId = Guid.NewGuid();
        return NoteId.ToString();
    }
}
