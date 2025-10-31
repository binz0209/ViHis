using VietHistory.Application.DTOs;
using VietHistory.Application.Services;
using VietHistory.Domain.Common;
using VietHistory.Domain.Entities;
using VietHistory.Infrastructure.Mongo;
using VietHistory.Infrastructure.Mongo.Repositories;
using VietHistory.Infrastructure.Services.Gemini;

namespace VietHistory.Infrastructure.Services;

public class QuizService : IQuizService
{
    private readonly IRepository<Quiz> _quizRepo;
    private readonly IRepository<QuizAttempt> _attemptRepo;
    private readonly QuizGenerationService _quizGenerator;

    public QuizService(MongoContext ctx, QuizGenerationService quizGenerator)
    {
        _quizRepo = new MongoRepository<Quiz>(ctx.Quizzes);
        _attemptRepo = new MongoRepository<QuizAttempt>(ctx.QuizAttempts);
        _quizGenerator = quizGenerator;
    }

    public async Task<QuizDto> CreateQuizAsync(string creatorId, CreateQuizRequest req)
    {
        if (req == null) throw new ArgumentNullException(nameof(req));
        if (string.IsNullOrWhiteSpace(req.Topic)) throw new ArgumentException("Topic is required", nameof(req.Topic));
        if (req.MultipleChoiceCount < 0 || req.EssayCount < 0) throw new ArgumentException("Counts must be >= 0");
        if (req.MultipleChoiceCount + req.EssayCount == 0) throw new ArgumentException("At least one question required");

        // Generate quiz questions using AI (placeholder or Gemini-based)
        var questions = await _quizGenerator.GenerateQuizQuestionsAsync(
            req.Topic,
            req.MultipleChoiceCount,
            req.EssayCount
        );

        var quiz = new Quiz
        {
            CreatorId = creatorId,
            Topic = req.Topic,
            MultipleChoiceCount = req.MultipleChoiceCount,
            EssayCount = req.EssayCount,
            Questions = questions
        };

        await _quizRepo.AddAsync(quiz);
        
        var questionDtos = quiz.Questions.Select(q => new QuizQuestionDto(
            q.Id,
            q.Type,
            q.Question,
            q.Options,
            q.CorrectAnswerIndex
        )).ToList();

        return new QuizDto(
            quiz.Id,
            quiz.CreatorId,
            quiz.Topic,
            quiz.MultipleChoiceCount,
            quiz.EssayCount,
            questionDtos
        );
    }

    public async Task<QuizDto> GetQuizAsync(string quizId)
    {
        var quiz = await _quizRepo.GetByIdAsync(quizId);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz with ID {quizId} not found.");

        var questionDtos = quiz.Questions.Select(q => new QuizQuestionDto(
            q.Id,
            q.Type,
            q.Question,
            q.Options,
            q.CorrectAnswerIndex
        )).ToList();

        return new QuizDto(
            quiz.Id,
            quiz.CreatorId,
            quiz.Topic,
            quiz.MultipleChoiceCount,
            quiz.EssayCount,
            questionDtos
        );
    }

    public async Task<QuizAttemptDto> SubmitQuizAsync(string userId, SubmitQuizRequest req)
    {
        var quiz = await _quizRepo.GetByIdAsync(req.QuizId);
        if (quiz == null)
            throw new KeyNotFoundException($"Quiz with ID {req.QuizId} not found.");

        // Calculate score
        var answerResults = new List<QuizAnswerResult>();
        var score = 0;

        foreach (var question in quiz.Questions)
        {
            var userAnswer = req.Answers.TryGetValue(question.Id, out var answer) ? answer : null;
            
            if (question.Type == "multipleChoice")
            {
                var isCorrect = question.CorrectAnswerIndex.HasValue &&
                    userAnswer == question.CorrectAnswerIndex.Value.ToString();
                
                if (isCorrect)
                    score++;

                var correctAnswer = question.CorrectAnswerIndex.HasValue 
                    ? question.CorrectAnswerIndex.Value.ToString() 
                    : null;

                answerResults.Add(new QuizAnswerResult(
                    question.Id,
                    isCorrect,
                    userAnswer,
                    correctAnswer
                ));
            }
            else if (question.Type == "essay")
            {
                // Essay questions are not scored automatically
                answerResults.Add(new QuizAnswerResult(
                    question.Id,
                    false,
                    userAnswer,
                    null
                ));
            }
        }

        var totalMcq = quiz.Questions.Count(q => q.Type == "multipleChoice");
        var attempt = new QuizAttempt
        {
            QuizId = req.QuizId,
            UserId = userId,
            Answers = req.Answers ?? new Dictionary<string, string>(),
            Score = score,
            TotalQuestions = totalMcq,
            CompletedAt = DateTime.UtcNow
        };

        await _attemptRepo.AddAsync(attempt);

        return new QuizAttemptDto(
            attempt.Id,
            attempt.QuizId,
            attempt.UserId,
            attempt.Score,
            attempt.TotalQuestions,
            attempt.CompletedAt,
            answerResults
        );
    }

    public async Task<IReadOnlyList<QuizDto>> GetUserQuizzesAsync(string userId)
    {
        var quizzes = await _quizRepo.FindAsync(q => q.CreatorId == userId, 0, 100);
        
        return quizzes.Select(quiz => 
        {
            var questionDtos = quiz.Questions.Select(q => new QuizQuestionDto(
                q.Id,
                q.Type,
                q.Question,
                q.Options,
                q.CorrectAnswerIndex
            )).ToList();

            return new QuizDto(
                quiz.Id,
                quiz.CreatorId,
                quiz.Topic,
                quiz.MultipleChoiceCount,
                quiz.EssayCount,
                questionDtos
            );
        }).ToList();
    }

    public async Task<QuizAttemptDto?> GetUserQuizAttemptAsync(string quizId, string userId)
    {
        var attempts = await _attemptRepo.FindAsync(a => a.QuizId == quizId && a.UserId == userId, 0, 1);
        var attempt = attempts.FirstOrDefault();
        
        if (attempt == null)
            return null;

        var quiz = await _quizRepo.GetByIdAsync(quizId);
        if (quiz == null)
            return null;

        // Build answer results
        var answerResults = new List<QuizAnswerResult>();
        foreach (var question in quiz.Questions)
        {
            var userAnswer = attempt.Answers.TryGetValue(question.Id, out var answer) ? answer : null;
            
            if (question.Type == "multipleChoice")
            {
                var isCorrect = question.CorrectAnswerIndex.HasValue &&
                    userAnswer == question.CorrectAnswerIndex.Value.ToString();
                
                var correctAnswer = question.CorrectAnswerIndex.HasValue 
                    ? question.CorrectAnswerIndex.Value.ToString() 
                    : null;

                answerResults.Add(new QuizAnswerResult(
                    question.Id,
                    isCorrect,
                    userAnswer,
                    correctAnswer
                ));
            }
            else
            {
                answerResults.Add(new QuizAnswerResult(
                    question.Id,
                    false,
                    userAnswer,
                    null
                ));
            }
        }

        return new QuizAttemptDto(
            attempt.Id,
            attempt.QuizId,
            attempt.UserId,
            attempt.Score,
            attempt.TotalQuestions,
            attempt.CompletedAt,
            answerResults
        );
    }
}

