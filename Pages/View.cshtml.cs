using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;

namespace QuizApp.Pages;

public class ViewModel : PageModel
{
    private readonly QuizDbContext _context;
    public ViewModel(QuizDbContext context)
    {
        _context = context;
    }
    [BindProperty(SupportsGet = true)]
    public Guid QuizId { get; set; }
    public Quiz? Quiz { get; set; }
    public QuizAttempt? QuizAttempt { get; set; }
    public async Task<IActionResult> OnGetAsync()
    {
        Quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == QuizId);

        if (Quiz == null)
        {
            return NotFound();
        }

        QuizAttempt = await _context.QuizAttempts
            .Include(qa => qa.UserAnswers)
            .ThenInclude(ua => ua.Question)
            .ThenInclude(q => q.Answers)
            .Where(qa => qa.QuizId == QuizId)
            .OrderByDescending(qa => qa.AttemptedAt)
            .FirstOrDefaultAsync();
        return Page();
    }
}