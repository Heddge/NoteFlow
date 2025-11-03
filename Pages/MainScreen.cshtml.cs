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

            
            /*
            List<string> temp = Directory.GetFiles(_notesPathReading, "*.md").ToList();

            foreach (string s in temp)
            {
                NoteTitles.Add(Regex.Match(s, @"(?<=Notes\\{1})(.*)(?=_\d{8}_\d{6}\.md)").ToString());
            }
            */
        }
    }
}