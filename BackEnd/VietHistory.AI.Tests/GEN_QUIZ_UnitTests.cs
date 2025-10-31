using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VietHistory.Application.DTOs;
using VietHistory.Infrastructure.Mongo;
using VietHistory.Infrastructure.Services;
using Xunit;

namespace VietHistory.AI.Tests
{
    [Trait("Feature", "GEN_QUIZ")]
    public class GEN_QUIZ_UnitTests
    {
        private readonly IMongoContext _mongo;
        private readonly QuizService _service;

        public GEN_QUIZ_UnitTests()
        {
            // Reuse the same test database setup as AI_QA tests
            var mongoSettings = new MongoSettings
            {
                ConnectionString = "mongodb+srv://lanserveUser:Binzdapoet.020904@hlinhwfil.eunq7.mongodb.net/?retryWrites=true&w=majority&appName=HlinhWfil",
                Database = "vihis_test"
            };
            _mongo = new MongoContext(mongoSettings);
            _service = new QuizService(_mongo);
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC01_CreateQuiz_ZeroMcq_SomeEssay_Should_Succeed()
        {
            var quiz = await _service.CreateQuizAsync("u1", new CreateQuizRequest("Topic", 0, 2));
            quiz.Questions.Count(q => q.Type == "multipleChoice").Should().Be(0);
            quiz.Questions.Count(q => q.Type == "essay").Should().Be(2);
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC02_Duplicate_Create_Should_Produce_Distinct_QuizIds()
        {
            var q1 = await _service.CreateQuizAsync("u1", new CreateQuizRequest("Topic", 1, 0));
            var q2 = await _service.CreateQuizAsync("u1", new CreateQuizRequest("Topic", 1, 0));
            q1.Id.Should().NotBe(q2.Id);
        }

        [Fact]
        [Trait("Category", "Scoring")]
        [Trait("Priority", "P0")]
        public async Task TC03_Submit_All_Wrong_Should_Score_Zero()
        {
            var quiz = await _service.CreateQuizAsync("u1", new CreateQuizRequest("Topic", 2, 0));
            var wrong = quiz.Questions.ToDictionary(q => q.Id, q => q.Type == "multipleChoice" ? "3" : "");
            var attempt = await _service.SubmitQuizAsync("u1", new SubmitQuizRequest(quiz.Id, wrong));
            attempt.Score.Should().Be(0);
            attempt.TotalQuestions.Should().Be(2);
        }

        [Fact]
        [Trait("Category", "Scoring")]
        [Trait("Priority", "P1")]
        public async Task TC04_Submit_Partial_Correct_Should_Score_Partial()
        {
            var quiz = await _service.CreateQuizAsync("u1", new CreateQuizRequest("Topic", 2, 0));
            var ans = new Dictionary<string, string>();
            ans[quiz.Questions[0].Id] = "0"; // correct first
            ans[quiz.Questions[1].Id] = "2"; // wrong second
            var attempt = await _service.SubmitQuizAsync("u1", new SubmitQuizRequest(quiz.Id, ans));
            attempt.Score.Should().Be(1);
            attempt.TotalQuestions.Should().Be(2);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P2")]
        public async Task TC05_Submit_Answer_Map_With_Nulls_Should_Not_Throw()
        {
            var quiz = await _service.CreateQuizAsync("u1", new CreateQuizRequest("Topic", 1, 1));
            var ans = new Dictionary<string, string?>();
            // MCQ missing becomes default null
            // Essay whitespace
            ans[quiz.Questions.First().Id] = null;
            ans[quiz.Questions.Last().Id] = "   ";
            var nonNull = ans.ToDictionary(kv => kv.Key, kv => kv.Value);
            var attempt = await _service.SubmitQuizAsync("u1", new SubmitQuizRequest(quiz.Id, nonNull!));
            attempt.Score.Should().Be(0);
        }

        [Fact]
        [Trait("Category", "DataIntegrity")]
        [Trait("Priority", "P2")]
        public async Task TC06_Submit_Then_GetQuiz_Should_Not_Change_Content()
        {
            var quiz = await _service.CreateQuizAsync("u1", new CreateQuizRequest("Topic", 1, 1));
            var before = quiz.Questions.Select(q => q.Id).ToList();
            var ans = quiz.Questions.ToDictionary(q => q.Id, q => q.Type == "multipleChoice" ? "0" : "text");
            _ = await _service.SubmitQuizAsync("u1", new SubmitQuizRequest(quiz.Id, ans));
            var fetched = await _service.GetQuizAsync(quiz.Id);
            fetched.Questions.Select(q => q.Id).Should().Equal(before);
        }

        [Fact]
        [Trait("Category", "MyEndpoints")]
        [Trait("Priority", "P1")]
        public async Task TC07_GetUserQuizAttempt_Should_Return_Null_When_None()
        {
            var quiz = await _service.CreateQuizAsync("u1", new CreateQuizRequest("Topic", 1, 0));
            var attempt = await _service.GetUserQuizAttemptAsync(quiz.Id, "u1");
            attempt.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "MyEndpoints")]
        [Trait("Priority", "P1")]
        public async Task TC08_GetUserQuizAttempt_Should_Return_Latest()
        {
            var quiz = await _service.CreateQuizAsync("u1", new CreateQuizRequest("Topic", 1, 0));
            var ans = new Dictionary<string, string>();
            ans[quiz.Questions[0].Id] = "0";
            _ = await _service.SubmitQuizAsync("u1", new SubmitQuizRequest(quiz.Id, ans));
            _ = await _service.SubmitQuizAsync("u1", new SubmitQuizRequest(quiz.Id, ans));
            var latest = await _service.GetUserQuizAttemptAsync(quiz.Id, "u1");
            latest.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Validation")]
        [Trait("Priority", "P0")]
        public async Task TC09_CreateQuiz_Should_Throw_When_Topic_Empty()
        {
            var req = new CreateQuizRequest(" ", 1, 0);
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateQuizAsync("u1", req));
        }

        [Fact]
        [Trait("Category", "Validation")]
        [Trait("Priority", "P0")]
        public async Task TC10_CreateQuiz_Should_Throw_When_Counts_Invalid()
        {
            var req = new CreateQuizRequest("Topic", -1, 0);
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateQuizAsync("u1", req));

            var reqZero = new CreateQuizRequest("Topic", 0, 0);
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateQuizAsync("u1", reqZero));
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P0")]
        public async Task TC11_CreateQuiz_Should_Return_Questions_As_Requested()
        {
            var req = new CreateQuizRequest("Nhà Trần", 2, 1);
            var quiz = await _service.CreateQuizAsync("u1", req);
            quiz.Topic.Should().Be("Nhà Trần");
            quiz.MultipleChoiceCount.Should().Be(2);
            quiz.EssayCount.Should().Be(1);
            quiz.Questions.Count.Should().Be(3);
            quiz.Questions.Count(q => q.Type == "multipleChoice").Should().Be(2);
            quiz.Questions.Count(q => q.Type == "essay").Should().Be(1);
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC12_CreateQuiz_Unicode_Topic_Should_Succeed()
        {
            var quiz = await _service.CreateQuizAsync("u1", new CreateQuizRequest("Lý Thường Kiệt – Ỷ Lan", 1, 1));
            quiz.Topic.Should().Contain("Ỷ Lan");
            quiz.Questions.Count.Should().Be(2);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P1")]
        public async Task TC13_SubmitQuiz_Missing_Answers_Should_Return_Zero_Score()
        {
            var quiz = await _service.CreateQuizAsync("u1", new CreateQuizRequest("Topic", 2, 1));
            var attempt = await _service.SubmitQuizAsync("u1", new SubmitQuizRequest(quiz.Id, new Dictionary<string, string>()));
            attempt.Score.Should().Be(0);
            attempt.TotalQuestions.Should().Be(2);
        }

        [Fact]
        [Trait("Category", "MyEndpoints")]
        [Trait("Priority", "P1")]
        public async Task TC14_GetUserQuizzes_Should_Filter_By_User()
        {
            await _service.CreateQuizAsync("alice", new CreateQuizRequest("T1", 1, 0));
            await _service.CreateQuizAsync("bob", new CreateQuizRequest("T2", 1, 0));
            var list = await _service.GetUserQuizzesAsync("alice");
            list.Should().OnlyContain(q => q.CreatorId == "alice");
        }

        [Fact]
        [Trait("Category", "Scoring")]
        [Trait("Priority", "P0")]
        public async Task TC15_SubmitQuiz_Should_Score_MCQ_Correctly()
        {
            var quiz = await _service.CreateQuizAsync("u1", new CreateQuizRequest("Chủ đề", 2, 0));
            // All correct → since placeholder sets correct index = 0
            var answers = quiz.Questions.ToDictionary(q => q.Id, q => q.Type == "multipleChoice" ? "0" : "");
            var attempt = await _service.SubmitQuizAsync("u1", new SubmitQuizRequest(quiz.Id, answers));
            attempt.Score.Should().Be(2);
            attempt.TotalQuestions.Should().Be(2);
        }

        [Fact]
        [Trait("Category", "DataIntegrity")]
        [Trait("Priority", "P2")]
        public async Task TC16_Question_Order_Should_Remain_Stable()
        {
            var quiz = await _service.CreateQuizAsync("u1", new CreateQuizRequest("Stable", 3, 1));
            var ids = quiz.Questions.Select(q => q.Id).ToList();
            var fetched = await _service.GetQuizAsync(quiz.Id);
            fetched.Questions.Select(q => q.Id).Should().Equal(ids);
        }

        [Fact]
        [Trait("Category", "Performance")]
        [Trait("Priority", "P2")]
        public async Task TC17_Create_Parallel_Five_Should_Succeed()
        {
            var tasks = Enumerable.Range(0,5).Select(i => _service.CreateQuizAsync("u1", new CreateQuizRequest($"P{i}", 1, 0)));
            var quizzes = await Task.WhenAll(tasks);
            quizzes.Length.Should().Be(5);
        }
    }
}


