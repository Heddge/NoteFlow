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


    public string BeatifulEditedDate()
    {
        if (NoteEdited.Date == DateTime.Now.Date)
            return $"сегодня {NoteEdited:HH:mm}";

        if (NoteEdited.Date == DateTime.Now.AddDays(-1).Date)
            return $"вчера {NoteEdited:HH:mm}";

        if (NoteEdited.Year == DateTime.Now.Year)
            return $"{NoteEdited:dd.MM HH:mm}";

        if (NoteEdited.Year != DateTime.Now.Year)
            return $"{NoteEdited:dd.MM.yy HH:mm}";
        
        return NoteEdited.ToString();
    }

    public string GetId()
    {
        NoteId = Guid.NewGuid();
        return NoteId.ToString();
    }
}
