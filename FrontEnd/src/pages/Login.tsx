import React, { useState } from 'react'
import { loginUser, registerUser } from '../services/auth'
import { useAuth } from '../context/AuthContext'
import './Login.css'

interface LoginProps {
  onClose: () => void
}

const Login: React.FC<LoginProps> = ({ onClose }) => {
  const { login, register } = useAuth()
  const [isLogin, setIsLogin] = useState(true)
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      if (isLogin) {
        await login(username, password)
      } else {
        await register(username, email, password)
      }
      
      onClose() // Close modal after successful login
    } catch (err: any) {
      setError(err.response?.data?.error || 'Đã có lỗi xảy ra')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="login-container">
      <div className="login-box">
            <div className="login-header">
              <button className="close-btn" onClick={onClose}>✕</button>
              <h1>🇻🇳 VietHistory</h1>
              <p>{isLogin ? 'Đăng nhập' : 'Đăng ký'}</p>
            </div>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Username</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              placeholder="Nhập username"
            />
          </div>

          {!isLogin && (
            <div className="form-group">
              <label>Email</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                placeholder="Nhập email"
              />
            </div>
          )}

          <div className="form-group">
            <label>Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              placeholder="Nhập password"
            />
          </div>

          <button type="submit" disabled={loading} className="submit-button">
            {loading ? 'Đang xử lý...' : (isLogin ? 'Đăng nhập' : 'Đăng ký')}
          </button>
        </form>

        <div className="switch-form">
          {isLogin ? (
            <p>
              Chưa có tài khoản?{' '}
              <a href="#" onClick={() => setIsLogin(false)}>
                Đăng ký ngay
              </a>
            </p>
          ) : (
            <p>
              Đã có tài khoản?{' '}
              <a href="#" onClick={() => setIsLogin(true)}>
                Đăng nhập
              </a>
            </p>
          )}
        </div>
      </div>
    </div>
  )
}

export default Login

