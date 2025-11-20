using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using NoteFlow.Models;
using NoteFlow.Services;
using System.Text.RegularExpressions;

namespace NoteFlow.Pages
{
    public class MainScreenModel : PageModel
    {
        // list of all user`s notes
        public List<Note> Notes = new List<Note>();

        // path for saving and parsing notes
        private string _notesPathReading = StorageService._notesPath;

        public void OnGet()
        {
            Notes = CurrentStorage.currNotes;
        }

        // method for redirecting to NoteScreen to create or edit note
        public IActionResult OnPostToEditorForEditOrCreate(string path)
        {
            // if user needs to CREATE note
            if (string.IsNullOrEmpty(path))
                return RedirectToPage("NoteScreen", new { notePath = "a" });

            // if user needs to EDIT note
            return RedirectToPage("NoteScreen", new { notePath = path });
        }
    }
}