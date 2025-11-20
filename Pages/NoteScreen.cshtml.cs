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

        public string _path;

        private Note _currNote;

        public void OnGet(string notePath)
        {
            this._path = notePath;
            if (System.IO.File.Exists(_path))
            {
                _currNote = new Note(_path);
                NoteTitle = _currNote.NoteTitle;
                NoteContent = _currNote.NoteContent;
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
            if ((string.IsNullOrWhiteSpace(NoteTitle) || string.IsNullOrEmpty(NoteTitle))
            && (string.IsNullOrWhiteSpace(NoteContent) || string.IsNullOrEmpty(NoteContent)))
                return Page();

            if (string.IsNullOrWhiteSpace(NoteTitle) || string.IsNullOrEmpty(NoteTitle))
            {
                NoteTitle = "Новая заметка";
            }

            // if user needs to edit note
            if (System.IO.File.Exists(_path))
                // updating path from old to new (with new filename)
                _path = StorageService.SaveEditedNote(NoteTitle, NoteContent, _path);

            // if user doesn`t need to edit note => create new note \/
            StorageService.SaveNewNote(NoteTitle, NoteContent);

            return Page();

        }

        public IActionResult OnPostDeleteNote(string path)
        {
            CurrentStorage.DeleteNoteFromCurrentNote(path);
            System.IO.File.Delete(path);
            return RedirectToPage("/MainScreen");
        }
    }
}