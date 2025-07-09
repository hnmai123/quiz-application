using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizApp.Pages;
using QuizApp.Models;
using QuizApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Admin")]
public class CreateQuizModel : PageModel
{
    private readonly QuizDbContext _context;
    public CreateQuizModel(QuizDbContext context)
    {
        _context = context;
    }
    [BindProperty]
    public required string Title { get; set; }
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            ModelState.AddModelError("Title", "Quiz title is required.");
            return Page();
        }
        var quiz = new Quiz(Title);
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();
        return RedirectToPage("AddQuestions", new { QuizId = quiz.Id });
    }
}