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

export default api

