import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Add token to requests if available
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export interface User {
  id: string
  username: string
  email: string
}

export interface AuthResponse {
  token: string
  user: User
}

export const registerUser = async (username: string, email: string, password: string): Promise<AuthResponse> => {
  try {
    const response = await api.post<AuthResponse>('/api/v1/auth/register', {
      username,
      email,
      password,
    })
    
    localStorage.setItem('authToken', response.data.token)
    localStorage.setItem('user', JSON.stringify(response.data.user))
    
    return response.data
  } catch (error) {
    console.error('Register error:', error)
    throw error
  }
}

export const loginUser = async (username: string, password: string): Promise<AuthResponse> => {
  try {
    const response = await api.post<AuthResponse>('/api/v1/auth/login', {
      username,
      password,
    })
    
    localStorage.setItem('authToken', response.data.token)
    localStorage.setItem('user', JSON.stringify(response.data.user))
    
    return response.data
  } catch (error) {
    console.error('Login error:', error)
    throw error
  }
}

export const logoutUser = (): void => {
  localStorage.removeItem('authToken')
  localStorage.removeItem('user')
}

export const getCurrentUser = async (): Promise<User | null> => {
  try {
    const response = await api.get<User>('/api/v1/auth/me')
    return response.data
  } catch (error) {
    console.error('Get current user error:', error)
    return null
  }
}

export const isAuthenticated = (): boolean => {
  return localStorage.getItem('authToken') !== null
}

export const getStoredUser = (): User | null => {
  const userStr = localStorage.getItem('user')
  if (!userStr) return null
  try {
    return JSON.parse(userStr)
  } catch {
    return null
  }
}

export default api


