using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;

namespace QuizApp.Pages;

public class AttemptModel : PageModel
{
    private readonly QuizDbContext _context;
    public AttemptModel(QuizDbContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public Guid QuizId { get; set; }
    public Quiz? Quiz { get; set; }

    [BindProperty]
    public Dictionary<Guid, string> Answers { get; set; } = new();

    [BindProperty]
    public Dictionary<Guid, List<Guid>> MultiChoiceAnswers { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == QuizId);

        if (Quiz == null) return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync()
    {
        Quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == QuizId);

        if (Quiz == null) return NotFound();

        int totalScore = 0;
        var attempt = new QuizAttempt
        {
            Id = Guid.NewGuid(),
            QuizId = Quiz.Id,
        };

        foreach (var question in Quiz.Questions)
        {
            var userAnswer = new UserAnswer
            {
                Id = Guid.NewGuid(),
                QuizAttemptId = attempt.Id,
                QuestionId = question.Id,
                Question = question
            };

            if (question.QuestionType == QuestionType.SingleChoice)
            {
                if (Answers.TryGetValue(question.Id, out string? selectedAnswerId)
                    && Guid.TryParse(selectedAnswerId, out Guid answerId))
                {
                    var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == answerId);
                    userAnswer.SelectedAnswerIds.Add(answerId);
                    if (selectedAnswer?.IsCorrect == true)
                    {
                        totalScore += question.MaxMark;
                        userAnswer.IsCorrect = true;
                    }
                }
            }
            else if (question.QuestionType == QuestionType.MultiChoice)
            {
                if (MultiChoiceAnswers.TryGetValue(question.Id, out List<Guid>? selectedAnswerIds))
                {
                    userAnswer.SelectedAnswerIds = selectedAnswerIds;
                    var correctAnswers = question.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToHashSet();
                    var selectedAnswers = selectedAnswerIds.ToHashSet();

                    // Check if all selected answers are correct and no incorrect answers are selected
                    if (selectedAnswers.SetEquals(correctAnswers))
                    {
                        totalScore += question.MaxMark;
                        userAnswer.IsCorrect = true;
                    }
                }
            }
            else if (question.QuestionType == QuestionType.ShortAnswer)
            {
                if (Answers.TryGetValue(question.Id, out string? textAnswer))
                {
                    // Short answer questions are manually graded by marker - no automatic scoring
                    userAnswer.TextAnswer = textAnswer;
                }
            }
            attempt.UserAnswers.Add(userAnswer);
        }

        // Update the quiz to mark it as attempted and store the score
        attempt.Score = totalScore;
        Quiz.SetGrade(totalScore);

        // Save the attempt
        _context.QuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        // return RedirectToPage("/View", new { QuizId = Quiz.Id });
        return RedirectToPage("/View", new { QuizId = Quiz.Id, AttemptId = attempt.Id });
    }

}