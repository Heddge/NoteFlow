using Microsoft.AspNetCore.Mvc.RazorPages;
using NoteFlow.Models;
using NoteFlow.Services;
using System.Text.RegularExpressions;

namespace NoteFlow.Pages
{
    public class MainScreenModel : PageModel
    {
        public List<string> NoteTitles = new List<string>();
        private static string _notesPathReading = StorageService._notesPath;

        public List<Note> Notes = new List<Note>();

        public void OnGet()
        {
            foreach (string path in Directory.GetFiles(StorageService._notesPath, "*.md"))
            {
                Notes.Add(new Note(path));
            }
            Notes = Notes.OrderByDescending(s => s.NoteCreated).ToList();
        }
    }
}