/**
 * Chat History Manager
 * Manages chat history storage per machine using Backend API with localStorage fallback
 */

import { saveChatHistory, loadChatHistory, deleteChatHistory, ChatMessage } from '../services/api'

interface Message {
  id: string
  text: string
  sender: 'user' | 'assistant'
  timestamp: Date
}

const MACHINE_ID_KEY = 'viet-history-machine-id'
const CHAT_HISTORY_KEY = 'viet-history-chat'

export class ChatHistoryManager {
  private machineId: string
  private storageKey: string

  constructor() {
    this.machineId = this.getOrCreateMachineId()
    this.storageKey = `${CHAT_HISTORY_KEY}-${this.machineId}`
  }

  /**
   * Get or create a unique machine ID
   */
  private getOrCreateMachineId(): string {
    let machineId = localStorage.getItem(MACHINE_ID_KEY)
    
    if (!machineId) {
      // Generate a unique ID using timestamp + random
      machineId = `machine-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`
      localStorage.setItem(MACHINE_ID_KEY, machineId)
    }
    
    return machineId
  }

  /**
   * Get the current machine ID
   */
  getMachineId(): string {
    return this.machineId
  }

  /**
   * Save messages to backend API with localStorage fallback
   */
  async saveMessages(messages: Message[]): Promise<void> {
    try {
      // Try to save to backend first
      await saveChatHistory(this.machineId, messages as ChatMessage[])
      
      // Also save to localStorage as backup
      const serialized = messages.map(msg => ({
        ...msg,
        timestamp: msg.timestamp.toISOString()
      }))
      localStorage.setItem(this.storageKey, JSON.stringify(serialized))
    } catch (error) {
      console.warn('Failed to save to backend, using localStorage only:', error)
      
      // Fallback to localStorage only
      const serialized = messages.map(msg => ({
        ...msg,
        timestamp: msg.timestamp.toISOString()
      }))
      localStorage.setItem(this.storageKey, JSON.stringify(serialized))
    }
  }

  /**
   * Load messages from backend API with localStorage fallback
   */
  async loadMessages(): Promise<Message[]> {
    try {
      // Try to load from backend first
      const messages = await loadChatHistory(this.machineId)
      
      // Also update localStorage
      const serialized = messages.map(msg => ({
        ...msg,
        timestamp: msg.timestamp.toISOString()
      }))
      localStorage.setItem(this.storageKey, JSON.stringify(serialized))
      
      return messages
    } catch (error) {
      console.warn('Failed to load from backend, using localStorage:', error)
      
      // Fallback to localStorage
      const stored = localStorage.getItem(this.storageKey)
      if (!stored) return []

      const parsed = JSON.parse(stored)
      return parsed.map((msg: any) => ({
        ...msg,
        timestamp: new Date(msg.timestamp)
      }))
    }
  }

  /**
   * Clear chat history for current machine
   */
  async clearHistory(): Promise<void> {
    try {
      await deleteChatHistory(this.machineId)
    } catch (error) {
      console.warn('Failed to delete from backend:', error)
    } finally {
      localStorage.removeItem(this.storageKey)
    }
  }

  /**
   * Get all machine IDs that have chat history
   */
  static getAllMachines(): string[] {
    const machines: string[] = []
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i)
      if (key && key.startsWith(CHAT_HISTORY_KEY)) {
        const machineId = key.replace(`${CHAT_HISTORY_KEY}-`, '')
        machines.push(machineId)
      }
    }
    return machines
  }

  /**
   * Check if there's any chat history for current machine
   */
  hasHistory(): boolean {
    return localStorage.getItem(this.storageKey) !== null
  }

  /**
   * Get number of messages for current machine
   */
  getHistoryLength(): number {
    const messages = this.loadMessages()
    return messages.length
  }

  /**
   * Get last message timestamp
   */
  getLastMessageTime(): Date | null {
    const messages = this.loadMessages()
    if (messages.length === 0) return null
    return messages[messages.length - 1].timestamp
  }

  /**
   * Export chat history as JSON
   */
  exportHistory(): string {
    const messages = this.loadMessages()
    return JSON.stringify({
      machineId: this.machineId,
      exportedAt: new Date().toISOString(),
      messageCount: messages.length,
      messages
    }, null, 2)
  }

  /**
   * Import chat history from JSON
   */
  importHistory(json: string): boolean {
    try {
      const data = JSON.parse(json)
      if (data.messages && Array.isArray(data.messages)) {
        const messages = data.messages.map((msg: any) => ({
          ...msg,
          timestamp: new Date(msg.timestamp)
        }))
        this.saveMessages(messages)
        return true
      }
      return false
    } catch (error) {
      console.error('Error importing chat history:', error)
      return false
    }
  }
}

// Singleton instance
let chatHistoryManager: ChatHistoryManager | null = null

export function getChatHistoryManager(): ChatHistoryManager {
  if (!chatHistoryManager) {
    chatHistoryManager = new ChatHistoryManager()
  }
  return chatHistoryManager
}

