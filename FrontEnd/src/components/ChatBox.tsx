import React, { useState, useRef, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import './ChatBox.css'
import { sendMessage, loadBoxMessages, saveBoxMessages, getApiClient } from '../services/api'
import { decodeHtmlEntities } from '../utils/htmlDecoder'
import { formatTextWithMarkdown } from '../utils/renderText'
import { useAuth } from '../context/AuthContext'

const api = getApiClient()

interface Message {
  id: string
  text: string
  sender: 'user' | 'assistant'
  timestamp: Date
  responseTime?: number // Time in milliseconds
}

interface ChatBoxProps {
  boxId?: string
}

const ChatBox: React.FC<ChatBoxProps> = ({ boxId }) => {
  const { user } = useAuth()
  const { chatId } = useParams<{ chatId?: string }>()
  const navigate = useNavigate()
  const [currentBoxId, setCurrentBoxId] = useState<string | undefined>(boxId || chatId)
  const [messages, setMessages] = useState<Message[]>([])
  const [input, setInput] = useState('')
  const [loading, setLoading] = useState(false)
  const messagesEndRef = useRef<HTMLDivElement>(null)
  const saveTimeoutRef = useRef<NodeJS.Timeout | null>(null)

  // Load chat history when boxId or chatId changes
  useEffect(() => {
    const effectiveBoxId = chatId || boxId
    if (!effectiveBoxId) {
      setMessages([])
      setCurrentBoxId(undefined)
      return
    }

    const loadHistory = async () => {
      try {
        console.log('📥 Loading messages for box:', effectiveBoxId)
        const savedMessages = await loadBoxMessages(effectiveBoxId)
        console.log('✅ Loaded', savedMessages.length, 'messages')
        setMessages(savedMessages)
        setCurrentBoxId(effectiveBoxId)
      } catch (error) {
        console.error('❌ Failed to load box messages:', error)
      }
    }
    loadHistory()
  }, [chatId, boxId])

  // Save chat history whenever messages change (debounced)
  useEffect(() => {
    // Skip save if no messages or no box ID
    if (messages.length === 0 || !currentBoxId) return
    
    // Skip auto-save for initial load (avoid saving empty when switching boxes)
    const isRecentMessage = messages.length > 0 && 
                           messages[messages.length - 1].timestamp.getTime() > Date.now() - 5000
    
    if (!isRecentMessage) return

    // Clear previous timeout
    if (saveTimeoutRef.current) {
      clearTimeout(saveTimeoutRef.current)
    }

    // Debounce save by 1 second
    saveTimeoutRef.current = setTimeout(async () => {
      try {
        let machineId = localStorage.getItem('viet-history-machine-id')
        if (!machineId) {
          machineId = `machine-${Date.now()}`
          localStorage.setItem('viet-history-machine-id', machineId)
        }
        await saveBoxMessages(currentBoxId, user?.id, machineId, messages)
        console.log('✅ Messages saved to box:', currentBoxId, '- Message count:', messages.length)
      } catch (error) {
        console.error('❌ Failed to save messages:', error)
      }
    }, 1000)

    return () => {
      if (saveTimeoutRef.current) {
        clearTimeout(saveTimeoutRef.current)
      }
    }
  }, [messages, currentBoxId, user])

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }

  useEffect(() => {
    scrollToBottom()
  }, [messages])

  const handleSend = async () => {
    if (!input.trim() || loading) return

    const userMessage: Message = {
      id: Date.now().toString(),
      text: input.trim(),
      sender: 'user',
      timestamp: new Date(),
    }

    setInput('')
    setLoading(true)

    // Track start time for response time calculation
    const startTime = Date.now()

    // Get chat history BEFORE adding new message (current messages are context)
    const chatHistoryForAI = messages.map(m => ({
      id: m.id,
      text: m.text,
      sender: m.sender,
      timestamp: m.timestamp.toISOString()
    }))

    // Add user message to UI
    setMessages(prev => [...prev, userMessage])

    try {
      // If no boxId, create a new box first
      let boxToUse = currentBoxId
      if (!boxToUse) {
        let machineId = localStorage.getItem('viet-history-machine-id')
        if (!machineId) {
          machineId = `machine-${Date.now()}`
          localStorage.setItem('viet-history-machine-id', machineId)
        }
        console.log('📦 Creating new box...')
        boxToUse = await saveBoxMessages('', user?.id, machineId, [userMessage])
        console.log('✅ Created box:', boxToUse)
        setCurrentBoxId(boxToUse)
      }

      // Get AI response with chat history for context
      console.log('🧠 Sending chat history as context (', chatHistoryForAI.length, 'messages)')
      const response = await sendMessage(input.trim(), chatHistoryForAI, currentBoxId)
      
      // Calculate response time
      const responseTime = Date.now() - startTime
      
      // Decode HTML entities in the response
      const decodedAnswer = decodeHtmlEntities(response.answer)
      const assistantMessage: Message = {
        id: (Date.now() + 1).toString(),
        text: decodedAnswer,
        sender: 'assistant',
        timestamp: new Date(),
        responseTime: responseTime,
      }
      setMessages(prev => [...prev, assistantMessage])
    } catch (error) {
      console.error('Error sending message:', error)
      const errorMessage: Message = {
        id: (Date.now() + 1).toString(),
        text: 'Xin lỗi, đã có lỗi xảy ra khi xử lý câu hỏi của bạn. Vui lòng thử lại.',
        sender: 'assistant',
        timestamp: new Date(),
      }
      setMessages(prev => [...prev, errorMessage])
    } finally {
      setLoading(false)
    }
  }

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  const handleClearHistory = async () => {
    if (window.confirm('Bạn có chắc chắn muốn xóa toàn bộ lịch sử chat của box này?')) {
      try {
        await api.delete(`/api/v1/chat/history/${currentBoxId}`)
        setMessages([])
      } catch (error) {
        console.error('Failed to clear history:', error)
      }
    }
  }

  const handleExampleClick = async (question: string) => {
    if (loading) return
    
    const userMessage: Message = {
      id: Date.now().toString(),
      text: question,
      sender: 'user',
      timestamp: new Date(),
    }

    setInput('')
    setLoading(true)

    // Track start time for response time calculation
    const startTime = Date.now()

    // Get chat history BEFORE adding new message (current messages are context)
    const chatHistoryForAI = messages.map(m => ({
      id: m.id,
      text: m.text,
      sender: m.sender,
      timestamp: m.timestamp.toISOString()
    }))

    // Add user message to UI
    setMessages(prev => [...prev, userMessage])

    try {
      // If no boxId, create a new box first
      let boxToUse = currentBoxId
      if (!boxToUse) {
        let machineId = localStorage.getItem('viet-history-machine-id')
        if (!machineId) {
          machineId = `machine-${Date.now()}`
          localStorage.setItem('viet-history-machine-id', machineId)
        }
        console.log('📦 Creating new box...')
        boxToUse = await saveBoxMessages('', user?.id, machineId, [userMessage])
        console.log('✅ Created box:', boxToUse)
        setCurrentBoxId(boxToUse)
      }

      // Get AI response with chat history for context  
      console.log('🧠 Sending chat history as context (', chatHistoryForAI.length, 'messages)')
      const response = await sendMessage(question, chatHistoryForAI, currentBoxId)
      
      // Calculate response time
      const responseTime = Date.now() - startTime
      // Decode HTML entities in the response
      const decodedAnswer = decodeHtmlEntities(response.answer)
      const assistantMessage: Message = {
        id: (Date.now() + 1).toString(),
        text: decodedAnswer,
        sender: 'assistant',
        timestamp: new Date(),
        responseTime: responseTime,
      }
      setMessages(prev => [...prev, assistantMessage])
    } catch (error) {
      console.error('Error sending message:', error)
      const errorMessage: Message = {
        id: (Date.now() + 1).toString(),
        text: 'Xin lỗi, đã có lỗi xảy ra khi xử lý câu hỏi của bạn. Vui lòng thử lại.',
        sender: 'assistant',
        timestamp: new Date(),
      }
      setMessages(prev => [...prev, errorMessage])
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="chat-box-container">
      <div className="chat-box">
        {messages.length > 0 && (
          <div className="chat-header">
              <span className="chat-info">
              💬 {messages.length} tin nhắn {currentBoxId ? `| Box: ${currentBoxId.substring(0, 8)}...` : ''}
            </span>
            <div className="chat-actions">
              <button 
                className="icon-button" 
                onClick={handleClearHistory}
                title="Xóa lịch sử"
              >
                🗑️
              </button>
            </div>
          </div>
        )}
        <div className="messages">
          {messages.length === 0 && (
            <div className="welcome-message">
              <h2>Chào mừng đến với ViHis! 🎓</h2>
              <p>Bạn có thể đặt câu hỏi về lịch sử Việt Nam tại đây.</p>
              <div className="example-questions">
                <p>Ví dụ:</p>
                <ul>
                  <li onClick={() => handleExampleClick("Chiến thắng Điện Biên Phủ diễn ra năm nào?")}>
                    "Chiến thắng Điện Biên Phủ diễn ra năm nào?"
                  </li>
                  <li onClick={() => handleExampleClick("Kể tôi nghe về cuộc khởi nghĩa Hai Bà Trưng")}>
                    "Kể tôi nghe về cuộc khởi nghĩa Hai Bà Trưng"
                  </li>
                  <li onClick={() => handleExampleClick("Ai là vị vua đầu tiên của triều Nguyễn?")}>
                    "Ai là vị vua đầu tiên của triều Nguyễn?"
                  </li>
                </ul>
              </div>
            </div>
          )}
          
          {messages.map(msg => {
            const formattedText = formatTextWithMarkdown(msg.text)
            return (
              <div key={msg.id} className={`message ${msg.sender}`}>
                <div className="message-bubble">
                  <div 
                    className="message-text" 
                    dangerouslySetInnerHTML={{ __html: formattedText }}
                  />
                  <div className="message-time">
                    {msg.timestamp.toLocaleTimeString('vi-VN', { 
                      hour: '2-digit', 
                      minute: '2-digit' 
                    })}
                    {msg.responseTime && (
                      <span className="response-time"> • {(msg.responseTime / 1000).toFixed(1)}s</span>
                    )}
                  </div>
                </div>
              </div>
            )
          })}
          
          {loading && (
            <div className="message assistant">
              <div className="message-bubble">
                <div className="loading-dots">
                  <span></span>
                  <span></span>
                  <span></span>
                </div>
              </div>
            </div>
          )}
          
          <div ref={messagesEndRef} />
        </div>

        <div className="input-area">
          <textarea
            value={input}
            onChange={e => setInput(e.target.value)}
            onKeyPress={handleKeyPress}
            placeholder="Nhập câu hỏi của bạn..."
            rows={1}
            disabled={loading}
          />
          <button 
            onClick={handleSend} 
            disabled={!input.trim() || loading}
            className="send-button"
          >
            <svg 
              xmlns="http://www.w3.org/2000/svg" 
              viewBox="0 0 24 24" 
              fill="currentColor"
              width="24"
              height="24"
            >
              <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/>
            </svg>
          </button>
        </div>
      </div>
    </div>
  )
}

export default ChatBox

