using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NoteFlow.Pages
{
    public class NoteScreenModel : PageModel
    {
        public string Title{ get; set; }
        public string Content { get; set; }

        
    }
}