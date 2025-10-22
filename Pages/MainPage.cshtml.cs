using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace NoteFlow.Pages
{
    public class MainPageModel : PageModel
    {
        [BindProperty]
        public string Text { get; set; }
        public string Message { get; set; }

        public void OnGet()
        {
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
                System.IO.File.WriteAllText("C:/Users/Mi/Desktop/Studying/NoteFlow/output.txt", Text);
                Message = $"Текст сохранен в файл! Введено: {Text.Length} символов";
            }
            catch (Exception ex)
            {
                Message = $"Ошибка: {ex.Message}";
            }

            return Page();
        }
    }
}