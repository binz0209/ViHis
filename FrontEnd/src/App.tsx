import React, { useState, useEffect } from 'react'
import { Routes, Route, Link, useNavigate, useParams, useLocation } from 'react-router-dom'
import { AuthProvider, useAuth } from './context/AuthContext'
import ChatBoxSelector from './components/ChatBoxSelector'
import ChatBox from './components/ChatBox'
import Login from './pages/Login'
import Quiz from './pages/Quiz'
import './App.css'

function AppContent() {
  const { isAuthenticated, loading, user, logout } = useAuth()
  const location = useLocation()
  const [currentBoxId, setCurrentBoxId] = useState<string>('')
  const [showLogin, setShowLogin] = useState(false)
  const [showMobileMenu, setShowMobileMenu] = useState(false)
  
  // Determine active tab from URL
  const [activeTab, setActiveTab] = useState<'chat' | 'quiz'>(
    location.pathname.startsWith('/quiz') ? 'quiz' : 'chat'
  )

  // Update active tab when route changes
  useEffect(() => {
    if (location.pathname.startsWith('/quiz')) {
      setActiveTab('quiz')
    } else if (location.pathname.startsWith('/chat')) {
      setActiveTab('chat')
    }
  }, [location])

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
              <h1>ViHis <img src="/vietnam-flag.gif" alt="🇻🇳" className="flag-animation" /></h1>
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

        {/* Tabs */}
        <div className="tabs">
              <Link 
            to="/chat"
            className={`tab ${activeTab === 'chat' ? 'active' : ''}`}
          >
            💬 Chat
          </Link>
          <Link 
            to="/quiz"
            className={`tab ${activeTab === 'quiz' ? 'active' : ''}`}
          >
            🎯 Quiz
          </Link>
        </div>
        
        <div className="main-content">
          <Routes>
            <Route path="/chat/:chatId" element={
              <>
                <div className={`mobile-menu-overlay ${showMobileMenu ? 'active' : ''}`} 
                     onClick={() => setShowMobileMenu(false)}></div>
                <div className={`chat-selector-wrapper ${showMobileMenu ? 'active' : ''}`}>
                  <ChatBoxSelector
                    currentBoxId={currentBoxId}
                    onSelectBox={(id) => { setShowMobileMenu(false); }}
                    onCreateBox={() => { setShowMobileMenu(false); }}
                  />
                </div>
                <div className="chat-container">
                  <button className="mobile-menu-btn" onClick={() => setShowMobileMenu(true)}>
                    ☰
                  </button>
                  <ChatBox boxId={undefined} />
                </div>
              </>
            } />
            <Route path="/chat" element={
              <>
                <div className={`mobile-menu-overlay ${showMobileMenu ? 'active' : ''}`} 
                     onClick={() => setShowMobileMenu(false)}></div>
                <div className={`chat-selector-wrapper ${showMobileMenu ? 'active' : ''}`}>
                  <ChatBoxSelector
                    currentBoxId={currentBoxId}
                    onSelectBox={(id) => { setShowMobileMenu(false); }}
                    onCreateBox={() => { setShowMobileMenu(false); }}
                  />
                </div>
                <div className="chat-container">
                  <button className="mobile-menu-btn" onClick={() => setShowMobileMenu(true)}>
                    ☰
                  </button>
                  <ChatBox boxId={currentBoxId} />
                </div>
              </>
            } />
            <Route path="/" element={
              <>
                <div className={`mobile-menu-overlay ${showMobileMenu ? 'active' : ''}`} 
                     onClick={() => setShowMobileMenu(false)}></div>
                <div className={`chat-selector-wrapper ${showMobileMenu ? 'active' : ''}`}>
                  <ChatBoxSelector
                    currentBoxId={currentBoxId}
                    onSelectBox={(id) => { setShowMobileMenu(false); }}
                    onCreateBox={() => { setShowMobileMenu(false); }}
                  />
                </div>
                <div className="chat-container">
                  <button className="mobile-menu-btn" onClick={() => setShowMobileMenu(true)}>
                    ☰
                  </button>
                  <ChatBox boxId={currentBoxId} />
                </div>
              </>
            } />
            <Route path="/quiz/:quizId" element={<Quiz />} />
            <Route path="/quiz/new" element={<Quiz />} />
            <Route path="/quiz" element={<Quiz />} />
          </Routes>
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

