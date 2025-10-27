import React, { useState, useEffect } from 'react'
import { getChatBoxes, getApiClient } from '../services/api'
import { useAuth } from '../context/AuthContext'
import './ChatBoxSelector.css'

const api = getApiClient()

interface ChatBox {
  id: string
  name: string
  lastMessageAt: string
  messageCount: number
}

interface ChatBoxSelectorProps {
  currentBoxId?: string
  onSelectBox: (boxId: string) => void
  onCreateBox: () => void
}

const ChatBoxSelector: React.FC<ChatBoxSelectorProps> = ({ currentBoxId, onSelectBox, onCreateBox }) => {
  const { user, isAuthenticated } = useAuth()
  const [boxes, setBoxes] = useState<ChatBox[]>([])
  const [loading, setLoading] = useState(true)
  const [showNewBoxForm, setShowNewBoxForm] = useState(false)
  const [newBoxName, setNewBoxName] = useState('')

  useEffect(() => {
    loadBoxes()
  }, [user, isAuthenticated])

  const loadBoxes = async () => {
    try {
      setLoading(true)
      const userId = isAuthenticated && user ? user.id : undefined
      let machineId = localStorage.getItem('viet-history-machine-id')
      if (!machineId) {
        machineId = `machine-${Date.now()}`
        localStorage.setItem('viet-history-machine-id', machineId)
      }
      
      const boxes = await getChatBoxes(userId, machineId)
      setBoxes(boxes)
      
      // Auto-select first box if none selected
      if (boxes.length > 0 && !currentBoxId) {
        onSelectBox(boxes[0].id)
      }
    } catch (error) {
      console.error('Failed to load boxes:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleCreateBox = async () => {
    if (!newBoxName.trim()) return
    
    try {
      // Create new box by sending empty message with new box name
      const machineId = localStorage.getItem('viet-history-machine-id') || `machine-${Date.now()}`
      
      const response = await api.post('/api/v1/chat/history', {
        boxId: '', // Empty to create new
        machineId,
        userId: user?.id,
        boxName: newBoxName.trim(),
        messages: []
      })

      // Select the newly created box
      if (response.data.boxId) {
        onSelectBox(response.data.boxId)
      }

      setNewBoxName('')
      setShowNewBoxForm(false)
      loadBoxes()
    } catch (error) {
      console.error('Failed to create box:', error)
    }
  }

  const handleRenameBox = async (boxId: string, newName: string) => {
    try {
      await api.put(`/api/v1/chat/history/${boxId}/name`, { name: newName })
      loadBoxes()
    } catch (error) {
      console.error('Failed to rename box:', error)
    }
  }

  const handleDeleteBox = async (boxId: string) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa chat box này?')) return
    
    try {
      await api.delete(`/api/v1/chat/history/${boxId}`)
      
      // Clear current box if deleted
      if (boxId === currentBoxId) {
        onSelectBox('')
      }
      
      loadBoxes()
    } catch (error) {
      console.error('Failed to delete box:', error)
    }
  }

  if (loading) {
    return <div className="chat-box-selector loading">Đang tải...</div>
  }

  return (
    <div className="chat-box-selector">
      <div className="selector-header">
        <h3>📁 Chat Boxes</h3>
        <button onClick={() => setShowNewBoxForm(!showNewBoxForm)} className="btn-new">
          ✨
        </button>
      </div>

      {showNewBoxForm && (
        <div className="new-box-form">
          <input
            type="text"
            value={newBoxName}
            onChange={(e) => setNewBoxName(e.target.value)}
            placeholder="Tên chat box..."
            autoFocus
          />
          <div className="form-actions">
            <button onClick={handleCreateBox}>Tạo</button>
            <button onClick={() => setShowNewBoxForm(false)}>Hủy</button>
          </div>
        </div>
      )}

      <div className="box-list">
        {boxes.map((box) => (
          <div
            key={box.id}
            className={`box-item ${box.id === currentBoxId ? 'active' : ''}`}
            onClick={() => onSelectBox(box.id)}
          >
            <div className="box-name">{box.name}</div>
            <div className="box-meta">
              {box.messageCount} tin nhắn • {new Date(box.lastMessageAt).toLocaleDateString()}
            </div>
            <div className="box-actions">
              <button onClick={(e) => { e.stopPropagation(); handleDeleteBox(box.id) }}>🗑️</button>
            </div>
          </div>
        ))}
      </div>

      {boxes.length === 0 && (
        <div className="empty-state">
          Chưa có chat box nào. Nhấn ✨ để tạo mới!
        </div>
      )}
    </div>
  )
}

export default ChatBoxSelector

