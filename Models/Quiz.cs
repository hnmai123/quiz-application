namespace QuizApp.Models
{
    public class Quiz
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public List<Question> Questions { get; private set; } = new();
        public double? Grade { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public int TotalMarks => Questions.Sum(q => q.MaxMark);
        public Quiz(string title)
        {
            Id = Guid.NewGuid();
            Title = title;
        }
        public void SetGrade(int grade)
        {
            Grade = grade;
        }
        public void AddQuestion(string questionText, int maxMark, QuestionType questionType)
        {
            Questions.Add(new Question(questionText, maxMark, questionType, Id));
        }
        public bool IsAttempted => Grade.HasValue;
        public string GradeRange => $"0-{TotalMarks}";
        public double? GradePercentage => Grade.HasValue ? Math.Round(Grade.Value / TotalMarks * 100, 2) : null;
        public string GradeDisplay => GradePercentage.HasValue ? $"{GradePercentage.Value}%" : "-";
    }
}