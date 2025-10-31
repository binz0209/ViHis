# GEN_QUIZ – Phase 1: Analyze (Result – theo Prompt Phase 1 mới)

Date: 2025-10-31
Scope: Phân tích feature GEN_QUIZ theo template Phase 1 hợp nhất; liệt kê hàm/endpoint cần test, nhánh logic, edge cases, và ưu tiên.

### Functions to Test (Ranked)
1. POST `api/v1/quiz/submit` – SubmitQuiz (High)
   - Main purpose: Nhận đáp án, chấm điểm MCQ, lưu `QuizAttempt` và trả `QuizAttemptDto`.
   - Inputs: `SubmitQuizRequest(quizId:string, answers:Dictionary<string,string>)`; userId từ token hoặc `guest_{X-Machine-Id}`.
   - Returns: `QuizAttemptDto { id, quizId, userId, score, totalQuestions, completedAt, answerResults[] }`.
   - Dependencies: Mongo `Quizzes`, `QuizAttempts`; `IQuizService.SubmitQuizAsync`.
   - Logic branches: quiz không tồn tại; MCQ đúng/sai; essay (không tính điểm); answers rỗng/thiếu; key lạ; index không hợp lệ.
   - Edge cases: `answers=null`; `answers` thiếu/extra; tất cả sai; chỉ essay; nộp lặp lại; quiz lớn.
   - Test type: Integration (Real) + Unit (service); Traits: Feature=GEN_QUIZ, Category=Scoring|EdgeCase|ErrorHandling, Priority=P0/P1.
   - Suggested Test Names (TCxx): TC01_Submit_All_Correct_Score_Full; TC02_Submit_All_Wrong_Score_Zero; TC03_Submit_Partial; TC04_Submit_Missing_Answers; TC05_Submit_Unknown_Keys; TC06_Submit_Invalid_Index; TC07_Submit_Essay_Only; TC08_Submit_Repeat_NewAttempt.

2. POST `api/v1/quiz/create` – CreateQuiz (High)
   - Main purpose: Tạo quiz theo topic với số MCQ/essay; lưu DB; trả `QuizDto`.
   - Inputs: `CreateQuizRequest(topic, multipleChoiceCount, essayCount)`; userId từ token hoặc `guest_{X-Machine-Id}`.
   - Returns: `QuizDto { id, creatorId, topic, multipleChoiceCount, essayCount, questions[] }`.
   - Dependencies: Mongo `Quizzes`; `IQuizService.CreateQuizAsync`.
   - Logic branches: topic rỗng/whitespace; counts âm; tổng = 0; unicode topic; count lớn.
   - Edge cases: thiếu `X-Machine-Id` (guest mặc định); tạo trùng liên tiếp; giới hạn số câu.
   - Test type: Integration + Unit; Traits: HappyPath|Validation|EdgeCase; Priority=P0/P1.
   - Suggested (TCxx): TC09_Create_Valid_Returns_Quiz; TC10_Create_ZeroMcq_SomeEssay; TC11_Create_Invalid_Counts_Throws; TC12_Create_Empty_Topic_Throws; TC13_Create_Unicode_Topic; TC14_Create_Large_Counts_CurrentBehavior; TC15_Duplicate_Create_Returns_Distinct_Ids.

3. GET `api/v1/quiz/{quizId}` – GetQuiz (Medium)
   - Main purpose: Lấy đầy đủ quiz theo id.
   - Inputs: `quizId:string`.
   - Returns: `QuizDto` với danh sách `questions`.
   - Dependencies: Mongo `Quizzes`; `IQuizService.GetQuizAsync`.
   - Logic branches: tồn tại/không tồn tại; (malformed id hiện tại được Mongo ném FormatException trước controller – ghi nhận).
   - Edge cases: unknown id → 404; ổn định thứ tự câu hỏi.
   - Test type: Integration + Unit; Traits: ErrorHandling|DataIntegrity; Priority=P1/P2.
   - Suggested: TC16_GetQuiz_UnknownId_404; TC17_Question_Order_Stable.

4. GET `api/v1/quiz/my-quizzes` – GetMyQuizzes (Medium)
   - Main purpose: Liệt kê quiz do user hiện tại tạo.
   - Inputs: userId (token hoặc `guest_{X-Machine-Id}`); header `X-Machine-Id` khi guest.
   - Returns: `IReadOnlyList<QuizDto>`.
   - Dependencies: Mongo `Quizzes`; `IQuizService.GetUserQuizzesAsync`.
   - Logic branches: guest mới (rỗng), có dữ liệu.
   - Edge cases: `X-Machine-Id` khác nhau → tách người dùng.
   - Test type: Integration; Traits: MyEndpoints; Priority=P1/P2.
   - Suggested: TC18_MyQuizzes_RecentlyCreated_Included; TC19_MyQuizzes_Empty_For_New_Guest.

5. GET `api/v1/quiz/{quizId}/my-attempt` – GetMyAttempt [Authorize] (Medium)
   - Main purpose: Lấy attempt gần nhất của user cho quiz.
   - Inputs: `quizId`, token hợp lệ.
   - Returns: `QuizAttemptDto` hoặc `404`.
   - Dependencies: Mongo `QuizAttempts`, `Quizzes`; `IQuizService.GetUserQuizAttemptAsync`.
   - Logic branches: có/không có attempt; xác thực; quyền sở hữu.
   - Edge cases: thiếu token → 401; người khác → 404/403 (theo policy); malformed id (ghi nhận hành vi). 
   - Test type: Integration; Traits: Security|MyEndpoints; Priority=P1.
   - Suggested: TC20_MyAttempt_Without_Auth_401; TC21_MyAttempt_Returns_Latest.

### Prioritization
- High: SubmitQuiz, CreateQuiz – tác động trực tiếp scoring và tạo dữ liệu.
- Medium: GetQuiz, MyQuizzes, MyAttempt – điều hướng luồng người dùng, quyền truy cập.
- Low: Không có riêng; phần tối ưu/hiệu năng sẽ thêm ở Phase 5 (parallel creates/submits, big quiz size).

### Acceptance (Phase 1)
- Danh sách hàm đã xếp hạng với inputs/returns/dependencies/branches/edge cases.
- Mục tiêu coverage: `QuizController` ≥ 85% (hướng 100%), `QuizService` ≥ 100% logic chính.
- Ràng buộc môi trường: Integration Real (Mongo thực), không seed; cho phép Unit test ở service.

### Handoff sang Phase 2
- Sinh Test Case Matrix theo TCxx ở trên (Happy/Edge/Error/Scoring/Security/Performance) với Given–When–Then, Traits, và tiêu chí assert định lượng.
