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
        public Dictionary<string, Note> NotesDict = new Dictionary<string, Note>();
        public List<Reminder> Reminders = new List<Reminder>();
        public CacheService currentCache = new CacheService();
        public StorageService currentStorage = new StorageService();
        public void OnGet()
        {
            NotesDict = currentCache.currNotesDict;
            
            if (NotesDict.Count() != currentStorage.CountNotesInDirectory())
            {
                currentCache.UpdateMissedNotesInDirToCurrentNotes(NotesDict.Count() < currentStorage.CountNotesInDirectory());
                NotesDict = currentCache.currNotesDict;
            }
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