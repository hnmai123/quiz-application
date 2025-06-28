using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizApp.Data;
using QuizApp.Models;
namespace QuizApp.Pages;
using Microsoft.EntityFrameworkCore;

public class IndexModel : PageModel
{
    private readonly QuizDbContext _context;
    public IndexModel(QuizDbContext context)
    {
        _context = context;
    }
    public List<Quiz> Quizzes { get; set; } = new();
    public async Task OnGetAsync()
    {
        Quizzes = await _context.Quizzes
            .Include(q => q.Questions)
            .ToListAsync();
    }
}
