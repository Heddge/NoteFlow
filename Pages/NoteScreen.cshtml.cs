using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using NoteFlow.Models;
using NoteFlow.Services;

namespace NoteFlow.Pages
{
    public class NoteScreenModel : PageModel
    {

        [BindProperty]
        public string NoteContent { get; set; } = "";
        [BindProperty]
        public string NoteTitle { get; set; } = "";
        public string _path = "";
        private readonly CacheService cache;
        private readonly StorageService storage;

        public NoteScreenModel()
        {
            storage = new StorageService();
            cache = new CacheService(storage._notesPath, storage._remindersPath);
        }
        public void OnGet(string notePath)
        {
            this._path = notePath;

            if (System.IO.File.Exists(_path))
            {
                Note tempNote = new Note(_path);
                NoteTitle = tempNote.NoteTitle;
                NoteContent = tempNote.NoteContent;
            }
        }

        /// <summary>
        /// Function for saving new or editing old note.
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostCreateOrEditNote(string path)
        {
            _path = path;

            // if user tried to save full empty note
            if (string.IsNullOrWhiteSpace(NoteTitle) && string.IsNullOrWhiteSpace(NoteContent))
                return Page();

            if (string.IsNullOrWhiteSpace(NoteTitle))
            {
                NoteTitle = "Новая заметка";
            }

            // if user needs to edit note
            if (System.IO.File.Exists(_path))
            {
                string oldPath = _path;
                // updating path from old to new (with new filename)
                _path = storage.SaveEditedNote(NoteTitle, NoteContent, _path);
                cache.UpdateNoteInCurrentNotes(NoteTitle, NoteContent, _path, oldPath);
                return RedirectToPage("NoteScreen", new { notePath = _path });
            }

            // if user doesn`t need to edit note => create new note \/
            string newNotePath = storage.SaveNewNote(NoteTitle, NoteContent);
            cache.AddNoteToCurrentNotes(newNotePath);

            return RedirectToPage("NoteScreen", new { notePath = newNotePath });

        }

        public IActionResult OnPostDeleteNote(string path)
        {
            cache.DeleteNoteFromCurrentNotes(path);
            System.IO.File.Delete(path);
            return RedirectToPage("/MainScreen");
        }

        public Dictionary<string, Note> GetNotesDict() =>
            cache.currNotesDict;

        public IActionResult OnPostRedirectToNote(string path)
        {
                return RedirectToPage("NoteScreen", new { notePath = path });

        }

        public IActionResult OnPostDeleteNoteFromNoteScreen(string path)
        {
            cache.DeleteNoteFromCurrentNotes(path);
            System.IO.File.Delete(path);
            return RedirectToPage("/NoteScreen");
        }
    }
}
