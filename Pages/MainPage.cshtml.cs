using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NoteFlow.Services;

namespace NoteFlow.Pages
{
    public class MainPageModel : PageModel
    {
        [BindProperty]
        public string Text { get; set; }
        public string Message { get; set; }
    
        public void OnGet()
        {
            // Показываем где сохраняются файлы
            Message = StorageService.GetStorageInfo();
        }
        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                Message = "Введите текст перед сохранением!";
                return Page();
            }

            try
            {
                // var filePath = StorageService.SaveNote(Text);
                // System.IO.File.WriteAllText(filePath, Text);
                // Message = $"Текст сохранен в {filePath}! Введено: {Text.Length} символов";
            }
            catch (Exception ex)
            {
                Message = $"Ошибка: {ex.Message}";
            }

            return Page();
        }
    }
}