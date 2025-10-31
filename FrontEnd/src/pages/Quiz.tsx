import React, { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { createQuiz, getQuiz, submitQuiz, getMyQuizzes, QuizDto, QuizAttemptDto } from '../services/api'
import './Quiz.css'

const Quiz: React.FC = () => {
  const { quizId } = useParams<{ quizId?: string }>()
  const navigate = useNavigate()
  
  const [stage, setStage] = useState<'create' | 'taking' | 'submitted'>('create')
  const [topic, setTopic] = useState('')
  const [mcCount, setMcCount] = useState(5)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [quiz, setQuiz] = useState<QuizDto | null>(null)
  const [answers, setAnswers] = useState<Record<string, string>>({})
  const [attempt, setAttempt] = useState<QuizAttemptDto | null>(null)
  const [myQuizzes, setMyQuizzes] = useState<QuizDto[]>([])

  // Load quiz by ID from URL
  useEffect(() => {
    if (quizId && quizId !== 'new') {
      loadQuiz(quizId)
    }
  }, [quizId])

  // Load my quizzes list when in create stage
  useEffect(() => {
    if (stage === 'create' && !quizId) {
      loadMyQuizzes()
    }
  }, [stage, quizId])

  const loadQuiz = async (id: string) => {
    setLoading(true)
    setError('')
    try {
      const loadedQuiz = await getQuiz(id)
      setQuiz(loadedQuiz)
      setStage('taking')
    } catch (err: any) {
      setError(err.response?.data?.error || 'Không tìm thấy quiz')
    } finally {
      setLoading(false)
    }
  }

  const loadMyQuizzes = async () => {
    try {
      console.log('📋 Loading my quizzes...')
      const quizzes = await getMyQuizzes()
      console.log('✅ Loaded', quizzes.length, 'quizzes')
      setMyQuizzes(quizzes)
    } catch (err) {
      console.error('❌ Failed to load quizzes:', err)
      // Ignore error, might be unauth
      setMyQuizzes([])
    }
  }

  const handleCreateQuiz = async () => {
    if (!topic.trim()) {
      setError('Vui lòng nhập chủ đề')
      return
    }

    setLoading(true)
    setError('')

    try {
      const createdQuiz = await createQuiz({
        topic: topic.trim(),
        multipleChoiceCount: mcCount,
        essayCount: 0
      })
      setQuiz(createdQuiz)
      setStage('taking')
      // Update URL
      navigate(`/quiz/${createdQuiz.id}`)
    } catch (err: any) {
      setError(err.response?.data?.error || 'Có lỗi xảy ra khi tạo quiz')
    } finally {
      setLoading(false)
    }
  }

  const handleSubmitQuiz = async () => {
    if (!quiz) return

    setLoading(true)
    setError('')

    try {
      const result = await submitQuiz({
        quizId: quiz.id,
        answers
      })
      setAttempt(result)
      setStage('submitted')
    } catch (err: any) {
      console.error('❌ Submit error:', err)
      setError(err.response?.data?.error || 'Có lỗi xảy ra khi nộp bài')
    } finally {
      setLoading(false)
    }
  }

  const resetQuiz = () => {
    setStage('create')
    setQuiz(null)
    setAnswers({})
    setAttempt(null)
    navigate('/quiz/new')
  }

  const handleRetakeQuiz = (quizId: string) => {
    navigate(`/quiz/${quizId}`)
  }

  const copyQuizLink = (quizId: string) => {
    const link = `${window.location.origin}/quiz/${quizId}`
    navigator.clipboard.writeText(link).then(() => {
      // Show toast notification
      const toast = document.createElement('div')
      toast.textContent = '✅ Đã copy link vào clipboard!'
      toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: #4caf50;
        color: white;
        padding: 12px 24px;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.2);
        z-index: 10000;
        font-size: 14px;
        animation: slideIn 0.3s ease;
      `
      document.body.appendChild(toast)
      
      setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease'
        setTimeout(() => toast.remove(), 300)
      }, 2000)
    }).catch(err => {
      console.error('Failed to copy:', err)
      alert('Không thể copy link. Link là: ' + link)
    })
  }

  if (stage === 'create') {
    return (
      <div className="quiz-container">
        <div className="quiz-box">
          <h2>🎯 Tạo Quiz Mới</h2>
          
          {error && <div className="error-message">{error}</div>}

          <div className="form-group">
            <label>Chủ đề</label>
            <input
              type="text"
              value={topic}
              onChange={(e) => setTopic(e.target.value)}
              placeholder="Ví dụ: Lịch sử Việt Nam thời kỳ độc lập"
            />
          </div>

          <div className="form-group">
            <label>Số câu hỏi trắc nghiệm: {mcCount}</label>
            <input
              type="range"
              min="1"
              max="20"
              value={mcCount}
              onChange={(e) => setMcCount(parseInt(e.target.value))}
            />
          </div>

          <button 
            onClick={handleCreateQuiz} 
            disabled={loading}
            className="submit-button"
          >
            {loading ? 'Đang tạo...' : 'Tạo Quiz'}
          </button>

          {/* My Quizzes List - always show when in create stage */}
          <div className="my-quizzes-section">
            <h3 className="section-title">📝 Quiz Đã Làm</h3>
            {myQuizzes.length > 0 ? (
              <div className="my-quizzes-list">
                {myQuizzes.map((q) => (
                  <div key={q.id} className="quiz-item" onClick={() => copyQuizLink(q.id)} style={{cursor: 'pointer'}}>
                    <div className="quiz-item-info">
                      <h4>{q.topic}</h4>
                      <p>{q.questions.length} câu hỏi</p>
                    </div>
                    <div className="quiz-item-actions">
                      <button 
                        type="button"
                        onClick={(e) => {
                          e.stopPropagation();
                          handleRetakeQuiz(q.id);
                        }}
                        className="retake-btn"
                        title="Làm lại quiz này"
                        style={{padding: '10px 20px', border: 'none', borderRadius: '5px', background: '#4caf50', color: 'white', cursor: 'pointer'}}
                      >
                        🔄 Làm lại
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="empty-state">Chưa có quiz nào. Tạo quiz mới để bắt đầu!</p>
            )}
          </div>
        </div>
      </div>
    )
  }

  if (stage === 'submitted' && attempt && quiz) {
    const score = attempt.score || 0
    const total = attempt.totalQuestions
    const percentage = total > 0 ? Math.round((score / total) * 100) : 0
    const resultsMap = attempt.answerResults?.reduce((acc, r) => {
      acc[r.questionId] = r
      return acc
    }, {} as Record<string, typeof attempt.answerResults[0]>) || {}

    return (
      <div className="quiz-container">
        <div className="quiz-box">
          <div className="quiz-header">
            <h2>📊 Kết quả: {quiz.topic}</h2>
            <button onClick={resetQuiz} className="reset-btn">⟲ Tạo Quiz mới</button>
          </div>

          <div className="score-display">
            <div className="score-circle">{score}/{total}</div>
            <div className="percentage">{percentage}%</div>
          </div>
          
          <p className="result-message">
            {percentage >= 80 ? '🎉 Xuất sắc!' :
             percentage >= 60 ? '👍 Tốt!' :
             percentage >= 40 ? '💪 Cần cố gắng thêm!' :
             '📚 Học thêm nhé!'}
          </p>

          <div className="questions-list">
            {quiz.questions.map((question, index) => {
              const result = resultsMap[question.id]
              const isCorrect = result?.isCorrect
              
              return (
                <div 
                  key={question.id} 
                  className={`question-card ${isCorrect ? 'correct-answer' : 'wrong-answer'}`}
                >
                  <div className="result-badge">
                    {isCorrect ? '✓ Đúng' : '✗ Sai'}
                  </div>
                  <h3 className="question-number">Câu {index + 1}: Trắc nghiệm</h3>
                  <p className="question-text">{question.question}</p>
                  
                  {question.type === 'multipleChoice' && question.options ? (
                    <div className="options-list">
                      {question.options.map((option, optIndex) => {
                        const isSelected = String(optIndex) === answers[question.id]
                        const isCorrectOption = optIndex === question.correctAnswerIndex
                        
                        return (
                          <label 
                            key={optIndex} 
                            className={`option-item ${isSelected && isCorrect ? 'selected-correct' : ''} ${isCorrectOption ? 'correct-option' : ''}`}
                          >
                            <input
                              type="radio"
                              checked={isSelected}
                              disabled
                            />
                            <span>{option}</span>
                            {isCorrectOption && <span className="check-mark">✓</span>}
                          </label>
                        )
                      })}
                    </div>
                  ) : null}
                </div>
              )
            })}
          </div>

          <button onClick={resetQuiz} className="submit-button">
            Tạo Quiz mới
          </button>
        </div>
      </div>
    )
  }

  if (!quiz && stage !== 'create') {
    if (loading) {
      return <div className="quiz-container"><div className="quiz-box">Đang tải...</div></div>
    }
    // If we have quizId but no quiz loaded, show error
    if (quizId && quizId !== 'new') {
      return <div className="quiz-container"><div className="quiz-box">
        <p>{error || 'Không tìm thấy quiz'}</p>
        <button onClick={resetQuiz} className="reset-btn">Quay lại</button>
      </div></div>
    }
    // If no quiz and no quizId, show create form
    return <div className="quiz-container"><div className="quiz-box">Đang tải form...</div></div>
  }
  
  // If still no quiz at this point, stay in create stage
  if (!quiz) {
    return null // This will trigger the create stage rendering above
  }

  return (
    <div className="quiz-container">
      <div className="quiz-box">
        <div className="quiz-header">
          <h2>📝 Quiz: {quiz.topic}</h2>
          <button onClick={resetQuiz} className="reset-btn">⟲ Quay lại</button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <div className="questions-list">
          {quiz.questions.map((question, index) => (
            <div key={question.id} className="question-card">
              <h3 className="question-number">Câu {index + 1}: Trắc nghiệm</h3>
              <p className="question-text">{question.question}</p>
              
              {question.type === 'multipleChoice' && question.options ? (
                <div className="options-list">
                  {question.options.map((option, optIndex) => (
                    <label key={optIndex} className="option-item">
                      <input
                        type="radio"
                        name={question.id}
                        value={optIndex}
                        checked={answers[question.id] === String(optIndex)}
                        onChange={(e) => setAnswers({ ...answers, [question.id]: e.target.value })}
                      />
                      <span>{option}</span>
                    </label>
                  ))}
                </div>
              ) : null}
            </div>
          ))}
        </div>

        <button
          onClick={handleSubmitQuiz}
          disabled={loading}
          className="submit-button"
        >
          {loading ? 'Đang chấm...' : 'Nộp bài'}
        </button>
      </div>
    </div>
  )
}

export default Quiz
