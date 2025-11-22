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

        public void OnGet()
        {
            if (System.IO.File.ReadAllLines(Path.Combine(StorageService._notesPath, "notesdb.txt")).Count() != StorageService.CountFilesInDirectory() - 1)
                CacheService.UpdateMissedNotesInDirToCurrentNotes();
            
            Notes = CacheService.currNotes;

            // foreach (string s in System.IO.File.ReadAllLines(Path.Combine(StorageService._notesPath, "notesdb.txt")).ToArray())
            // {
            //     Notes.Add(new Note());
            //     Notes[Notes.Count()-1].NoteContent = 
            // }
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