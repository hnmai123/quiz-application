using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizApp.Models;
using QuizApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    [BindProperty]
    public int SelectedCorrectIndex { get; set; } = -1;

    [BindProperty]
    public bool IsLocked { get; set; } = false;

    public async Task<IActionResult> OnPostLockFields()
    {
        // Reload the quiz so it's available in the view
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == QuizId);

        if (quiz == null)
        {
            return NotFound();
        }

        Quiz = quiz;
        IsLocked = true;
        Console.WriteLine($"Fields locked for quiz ID {QuizId}");
        return Page();
    }


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
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == QuizId);

        if (quiz == null)
        {
            Console.WriteLine($"Quiz with ID {QuizId} not found.");
            return NotFound();
        }
        Quiz = quiz;
        Console.WriteLine($"Loaded quiz ID {Quiz.Id}");
        return Page();
    }
    public async Task<IActionResult> OnPostAddAnswerAsync()
    {

        if (!IsLocked)
        {
            Console.WriteLine("Cannot add answers: fields are not locked.");
            ModelState.AddModelError("IsLocked", "Questions must be saved before adding answers.");
            return Page();
        }
        var quiz = await _context.Quizzes
        .Include(q => q.Questions)
        .ThenInclude(q => q.Answers)
        .FirstOrDefaultAsync(q => q.Id == QuizId);

        if (quiz == null)
        {
            Console.WriteLine($"Quiz with ID {QuizId} not found.");
            return NotFound();
        }
        Quiz = quiz;
        if (!string.IsNullOrWhiteSpace(NewAnswerText))
        {
            TempAnswers.Add(new TempAnswer
            {
                Text = NewAnswerText.Trim(),
                IsCorrect = QuestionType == QuestionType.MultiChoice ? NewAnswerIsCorrect : false
            });
            Console.WriteLine($"Added answer: {NewAnswerText.Trim()}, Total answers: {TempAnswers.Count}");
            NewAnswerText = string.Empty;
            NewAnswerIsCorrect = false;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAddQuestionAsync()
    {
        var quiz = await _context.Quizzes.FindAsync(QuizId);
        if (quiz == null)
        {
            Console.WriteLine($"Quiz with ID {QuizId} not found.");
            return NotFound();
        }
        Quiz = quiz;
        if (string.IsNullOrWhiteSpace(questionText))
        {
            Console.WriteLine("Question text is required.");
            ModelState.AddModelError("questionText", "Question text is required.");
            return Page();
        }
        if (TempAnswers.Count == 0 && QuestionType != QuestionType.ShortAnswer)
        {
            Console.WriteLine("At least one answer is required for this question type.");
            ModelState.AddModelError("TempAnswers", "At least one answer is required for this question type.");
            return Page();
        }
        var question = new Question(questionText.Trim(), MaxMark, QuestionType, quiz.Id);

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
            for (int i = 0; i < TempAnswers.Count; i++)
            {
                bool isCorrect;
                if (QuestionType == QuestionType.MultiChoice)
                {
                    // For MultiChoice, use the IsCorrect value from TempAnswers
                    isCorrect = TempAnswers[i].IsCorrect;
                }
                else
                {
                    // For SingleChoice, use SelectedCorrectIndex
                    isCorrect = (SelectedCorrectIndex == i);
                }

                Console.WriteLine($"Creating answer: {TempAnswers[i].Text}, IsCorrect: {isCorrect}, QuestionType: {QuestionType}");
                var answer = new Answer(question.Id, TempAnswers[i].Text.Trim(), isCorrect);
                _context.Answers.Add(answer);
            }
        }

        await _context.SaveChangesAsync();

        TempAnswers.Clear();
        questionText = string.Empty;
        MaxMark = 0;
        QuestionType = QuestionType.SingleChoice;
        ShortAnswerText = null;
        NewAnswerText = string.Empty;
        NewAnswerIsCorrect = false;
        SelectedCorrectIndex = -1;
        IsLocked = false;
        Console.WriteLine("Question and answers added successfully.");
        return RedirectToPage("AddQuestions", new { QuizId = quiz.Id });
    }

    public async Task<IActionResult> OnPostChangeTypeAsync()
    {
        var quiz = await _context.Quizzes.FindAsync(QuizId);
        if (quiz == null)
        {
            return NotFound();
        }
        Quiz = quiz;
        TempAnswers.Clear();
        if (QuestionType == QuestionType.ShortAnswer)
        {
            NewAnswerText = null;
            NewAnswerIsCorrect = false;
        }
        else
        {
            NewAnswerText = string.Empty;
            NewAnswerIsCorrect = false;
        }
        Console.WriteLine($"Changed question type to {QuestionType}. Answers cleared.");
        return Page();
    }

    public async Task<IActionResult> OnPostSubmitQuizAsync()
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == QuizId);

        if (quiz == null)
        {
            return NotFound();
        }

        if (!quiz.Questions.Any())
        {
            ModelState.AddModelError("", "Cannot submit a quiz with no questions.");
            return Page();
        }
        Console.WriteLine($"Quiz '{quiz.Title}' has been finalized with {quiz.Questions.Count} questions.");
        return RedirectToPage("/Index"); 
    }
}