using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizApp.Models;
using QuizApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace QuizApp.Pages;

public class AddQuestionsModel : PageModel
{
    private readonly QuizDbContext _context;
    public AddQuestionsModel(QuizDbContext context)
    {
        _context = context;
    }
    [BindProperty]
    public required string questionText { get; set; }

    [BindProperty]
    public int MaxMark { get; set; }

    [BindProperty]
    public QuestionType QuestionType { get; set; }

    [BindProperty]
    public string? NewAnswerText { get; set; }

    [BindProperty]
    public bool NewAnswerIsCorrect { get; set; }

    [BindProperty]
    public string? ShortAnswerText { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid QuizId { get; set; }
    public Quiz? Quiz { get; set; }
    public class TempAnswer
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
    public static List<TempAnswer> TempAnswers { get; set; } = new();
    public async Task<IActionResult> OnGetAsync()
    {
        TempAnswers.Clear();
        var quiz = await _context.Quizzes.FindAsync(QuizId);
        if (quiz == null)
        {
            return NotFound();
        }
        Quiz = quiz;
        return Page();
    }
    public async Task<IActionResult> OnPostAddAnswerAsync()
    {
        var quiz = await _context.Quizzes.FindAsync(QuizId);
        if (quiz == null)
        {
            return NotFound();
        }
        Quiz = quiz;
        if (!string.IsNullOrWhiteSpace(NewAnswerText))
        {
            TempAnswers.Add(new TempAnswer
            {
                Text = NewAnswerText.Trim(),
                IsCorrect = NewAnswerIsCorrect
            });
            NewAnswerText = string.Empty;
            NewAnswerIsCorrect = false;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var quiz = await _context.Quizzes.FindAsync(QuizId);
        if (quiz == null)
        {
            return NotFound();
        }
        Quiz = quiz;
        if (string.IsNullOrWhiteSpace(questionText))
        {
            ModelState.AddModelError("questionText", "Question text is required.");
            return Page();
        }
        if (MaxMark <= 0)
        {
            ModelState.AddModelError("MaxMark", "Max mark must be greater than zero.");
            return Page();
        }
        if (TempAnswers.Count == 0 && QuestionType != QuestionType.ShortAnswer)
        {
            ModelState.AddModelError("TempAnswers", "At least one answer is required for this question type.");
            return Page();
        }
        var question = new Question(questionText.Trim(), MaxMark, QuestionType, quiz.Id);
        
        quiz.AddQuestion(question.QuestionText, question.MaxMark, question.QuestionType);
        _context.Questions.Add(question);

        if (QuestionType == QuestionType.ShortAnswer)
        {
            if (!string.IsNullOrWhiteSpace(ShortAnswerText))
            {
                var answer = new Answer(question.Id, ShortAnswerText.Trim(), true);
                _context.Answers.Add(answer);
            }
        }
        else
        {
            foreach (var tempAnswer in TempAnswers)
            {
                var answer = new Answer(question.Id, tempAnswer.Text.Trim(), tempAnswer.IsCorrect);
                _context.Answers.Add(answer);
            }
        }

        await _context.SaveChangesAsync();
        TempAnswers.Clear();
        return RedirectToPage("AddQuestions", new { QuizId = quiz.Id });
    }
}