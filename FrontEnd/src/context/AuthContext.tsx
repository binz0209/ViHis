import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { loginUser, registerUser, logoutUser, isAuthenticated as checkAuth, getStoredUser } from '../services/auth'
import { User } from '../services/auth'

interface AuthContextType {
  user: User | null
  isAuthenticated: boolean
  loading: boolean
  login: (username: string, password: string) => Promise<void>
  register: (username: string, email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isAuthenticated, setIsAuthenticated] = useState(false)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    // Check if user is logged in
    const checkAuthStatus = () => {
      const authenticated = checkAuth()
      const storedUser = getStoredUser()
      
      setIsAuthenticated(authenticated)
      setUser(storedUser)
      setLoading(false)
    }

    checkAuthStatus()
  }, [])

  const login = async (username: string, password: string) => {
    const response = await loginUser(username, password)
    setUser(response.user)
    setIsAuthenticated(true)
  }

  const register = async (username: string, email: string, password: string) => {
    const response = await registerUser(username, email, password)
    setUser(response.user)
    setIsAuthenticated(true)
  }

  const logout = () => {
    logoutUser()
    setUser(null)
    setIsAuthenticated(false)
  }

  return (
    <AuthContext.Provider value={{ user, isAuthenticated, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return context
}


