import axios from 'axios'

export interface AiAskRequest {
  question: string
  language?: string
  maxContext?: number
}

export interface AiAnswer {
  answer: string
  model: string
  costUsd?: number | null
}

const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Add machine ID to all requests
api.interceptors.request.use(config => {
  let machineId = localStorage.getItem('viet-history-machine-id')
  if (!machineId) {
    machineId = `machine-${Date.now()}`
    localStorage.setItem('viet-history-machine-id', machineId)
  }
  
  if (config.headers) {
    config.headers['X-Machine-Id'] = machineId
  }
  
  return config
})

export const sendMessage = async (
  question: string, 
  chatHistory?: ChatMessage[], 
  boxId?: string
): Promise<AiAnswer> => {
  try {
    const response = await api.post<AiAnswer>('/api/v1/ai/ask', {
      question,
      language: 'vi',
      maxContext: 5,
      boxId: boxId,
      chatHistory: chatHistory?.map(msg => ({
        id: msg.id,
        text: msg.text,
        sender: msg.sender,
        timestamp: msg.timestamp
      }))
    } as any)
    
    return response.data
  } catch (error) {
    console.error('API Error:', error)
    throw error
  }
}

// Chat History APIs
export interface ChatMessage {
  id: string
  text: string
  sender: 'user' | 'assistant'
  timestamp: Date
}

export const saveChatHistory = async (machineId: string, messages: ChatMessage[]): Promise<void> => {
  try {
    const response = await api.post('/api/v1/chat/history', {
      machineId,
      messages: messages.map(m => ({
        id: m.id,
        text: m.text,
        sender: m.sender,
        timestamp: m.timestamp.toISOString()
      }))
    })
    return response.data
  } catch (error) {
    console.error('Error saving chat history:', error)
    throw error
  }
}

export const loadChatHistory = async (machineId: string): Promise<ChatMessage[]> => {
  try {
    const response = await api.get<{ messages: ChatMessage[] }>(`/api/v1/chat/history/${machineId}`)
    return response.data.messages.map(m => ({
      ...m,
      timestamp: new Date(m.timestamp)
    }))
  } catch (error) {
    console.error('Error loading chat history:', error)
    throw error
  }
}

export const loadBoxMessages = async (boxId: string): Promise<ChatMessage[]> => {
  try {
    const response = await api.get<{ messages: ChatMessage[], boxId: string, name: string }>(`/api/v1/chat/history/${boxId}`)
    return response.data.messages.map(m => ({
      ...m,
      timestamp: new Date(m.timestamp)
    }))
  } catch (error) {
    console.error('Error loading box messages:', error)
    throw error
  }
}

export const deleteChatHistory = async (machineId: string): Promise<void> => {
  try {
    await api.delete(`/api/v1/chat/history/${machineId}`)
  } catch (error) {
    console.error('Error deleting chat history:', error)
    throw error
  }
}

export const saveBoxMessages = async (boxId: string, userId: string | undefined, machineId: string, messages: ChatMessage[]): Promise<string> => {
  try {
    const response = await api.post<{ success: boolean, boxId: string }>('/api/v1/chat/history', {
      boxId,
      userId,
      machineId,
      messages: messages.map(m => ({
        id: m.id,
        text: m.text,
        sender: m.sender,
        timestamp: m.timestamp.toISOString()
      }))
    })
    return response.data.boxId
  } catch (error) {
    console.error('Error saving box messages:', error)
    throw error
  }
}

export const getChatBoxes = async (userId?: string, machineId?: string): Promise<any[]> => {
  try {
    const params: any = {}
    if (userId) params.userId = userId
    if (machineId) params.machineId = machineId
    
    const response = await api.get('/api/v1/chat/boxes', { params })
    return response.data.boxes || []
  } catch (error) {
    console.error('Error loading chat boxes:', error)
    throw error
  }
}

export const getApiClient = () => api

// Quiz APIs
export interface CreateQuizRequest {
  topic: string
  multipleChoiceCount: number
  essayCount: number
}

export interface QuizQuestionDto {
  id: string
  type: string
  question: string
  options?: string[] | null
  correctAnswerIndex?: number | null
}

export interface QuizDto {
  id: string
  creatorId: string
  topic: string
  multipleChoiceCount: number
  essayCount: number
  questions: QuizQuestionDto[]
}

export interface SubmitQuizRequest {
  quizId: string
  answers: Record<string, string>
}

export interface QuizAnswerResult {
  questionId: string
  isCorrect: boolean
  userAnswer: string | null
  correctAnswer: string | null
}

export interface QuizAttemptDto {
  id: string
  quizId: string
  userId: string
  score?: number | null
  totalQuestions: number
  completedAt?: Date | null
  answerResults?: QuizAnswerResult[]
}

export const createQuiz = async (data: CreateQuizRequest): Promise<QuizDto> => {
  try {
    const response = await api.post<QuizDto>('/api/v1/quiz/create', data)
    return response.data
  } catch (error) {
    console.error('Create quiz error:', error)
    throw error
  }
}

export const getQuiz = async (quizId: string): Promise<QuizDto> => {
  try {
    const response = await api.get<QuizDto>(`/api/v1/quiz/${quizId}`)
    return response.data
  } catch (error) {
    console.error('Get quiz error:', error)
    throw error
  }
}

export const submitQuiz = async (data: SubmitQuizRequest): Promise<QuizAttemptDto> => {
  try {
    console.log('📡 API submit data:', data)
    const response = await api.post<QuizAttemptDto>('/api/v1/quiz/submit', data)
    console.log('📥 API response:', response.data)
    return response.data
  } catch (error) {
    console.error('❌ Submit quiz error:', error)
    throw error
  }
}

export const getMyQuizzes = async (): Promise<QuizDto[]> => {
  try {
    const response = await api.get<QuizDto[]>('/api/v1/quiz/my-quizzes')
    return response.data
  } catch (error) {
    console.error('Get my quizzes error:', error)
    throw error
  }
}

export const getMyAttempt = async (quizId: string): Promise<QuizAttemptDto | null> => {
  try {
    const response = await api.get<QuizAttemptDto>(`/api/v1/quiz/${quizId}/my-attempt`)
    return response.data
  } catch (error: any) {
    if (error.response?.status === 404) return null
    console.error('Get my attempt error:', error)
    throw error
  }
}

export default api

