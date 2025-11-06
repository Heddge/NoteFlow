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
        public string NoteContent { get; set; }
        [BindProperty]
        public string NoteTitle { get; set; }

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
            else
            {
                NoteTitle = "";
                NoteContent = "";
            }
        }

        public IActionResult OnPost()
        {
            return Page();
        }

        /// <summary>
        /// Function for saving new or editing old note.
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostCreateOrEditNote(string path)
        {
            _path = path;

            // if user needs to edit note
            if (System.IO.File.Exists(_path))
            {
                if (string.IsNullOrWhiteSpace(NoteTitle) || string.IsNullOrEmpty(NoteTitle))
                {
                    NoteTitle = "Новая заметка";
                }

                // new path with new (if user changed it) name
                string newFilePath = Path.Combine(StorageService._notesPath, $"{NoteTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.md");

                // put content into the current note
                System.IO.File.WriteAllText(_path, NoteContent);

                // renaming file
                System.IO.File.Move(_path, newFilePath);

                // updating path from old to new (with new filename)
                _path = newFilePath;

                return Page();
            }

            // if user doesn`t need to edit note => create new note
            if (string.IsNullOrWhiteSpace(NoteTitle) || string.IsNullOrEmpty(NoteTitle))
            {
                NoteTitle = "Новая заметка";
            }

            // creating new filepath
            var filePath = StorageService.SaveNote(NoteContent, $"{NoteTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.md");

            // put content into the current note
            System.IO.File.WriteAllText(filePath, NoteContent);
            return Page();

        }
    }
}