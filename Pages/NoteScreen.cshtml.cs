using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NoteFlow.Services;

namespace NoteFlow.Pages
{
    public class NoteScreenModel : PageModel
    {

        [BindProperty]
        public string NoteContent { get; set; }
        [BindProperty]
        public string NoteTitle { get; set; }

        public void OnGet()
        {
            
        }
        
        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(NoteTitle) || string.IsNullOrEmpty(NoteTitle))
            {
                NoteTitle = "Новая заметка";
            }

            var filePath = StorageService.SaveNote(NoteContent, $"{NoteTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.md");
            System.IO.File.WriteAllText(filePath, NoteContent);
            NoteTitle = $"Текст сохранен в {filePath}! Введено: {NoteContent.Length} символов";

            return Page();
        }
    }
}