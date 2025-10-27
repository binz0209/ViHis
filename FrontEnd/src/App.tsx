import React, { useState } from 'react'
import { AuthProvider, useAuth } from './context/AuthContext'
import ChatBoxSelector from './components/ChatBoxSelector'
import ChatBox from './components/ChatBox'
import Login from './pages/Login'
import './App.css'

function AppContent() {
  const { isAuthenticated, loading, user, logout } = useAuth()
  const [currentBoxId, setCurrentBoxId] = useState<string>('')
  const [showLogin, setShowLogin] = useState(false)
  const [showMobileMenu, setShowMobileMenu] = useState(false)

  if (loading) {
    return (
      <div className="App">
        <div className="loading-screen">Đang tải...</div>
      </div>
    )
  }

  const handleLoginClick = () => {
    if (isAuthenticated) {
      logout()
    } else {
      setShowLogin(true)
    }
  }

  return (
    <div className="App">
      {showLogin && <Login onClose={() => setShowLogin(false)} />}
      <div className="container">
        <header className="header">
          <div className="header-content">
            <div>
              <h1>ViHis</h1>
              <p className="subtitle">Trợ lý thông minh tìm hiểu Lịch sử Việt Nam</p>
            </div>
            <div className="header-actions">
              {isAuthenticated && user && (
                <span className="user-info">👤 {user.username}</span>
              )}
              <button className="login-btn" onClick={handleLoginClick}>
                {isAuthenticated ? 'Đăng xuất' : '🔐 Đăng nhập'}
              </button>
            </div>
          </div>
        </header>
        
        <div className="main-content">
          <div className={`mobile-menu-overlay ${showMobileMenu ? 'active' : ''}`} 
               onClick={() => setShowMobileMenu(false)}></div>
          <div className={`chat-selector-wrapper ${showMobileMenu ? 'active' : ''}`}>
            <ChatBoxSelector
              currentBoxId={currentBoxId}
              onSelectBox={(id) => { setCurrentBoxId(id); setShowMobileMenu(false); }}
              onCreateBox={() => { setCurrentBoxId('new'); setShowMobileMenu(false); }}
            />
          </div>
          <div className="chat-container">
            <button className="mobile-menu-btn" onClick={() => setShowMobileMenu(true)}>
              📁
            </button>
            <ChatBox boxId={currentBoxId} />
          </div>
        </div>
      </div>
    </div>
  )
}

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  )
}

export default App

