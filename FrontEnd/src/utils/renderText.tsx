import React from 'react'

/**
 * Simple markdown renderer for basic formatting
 * Converts markdown to HTML elements
 */
export function renderMarkdownText(text: string): React.ReactNode {
  const lines = text.split('\n')
  const elements: React.ReactNode[] = []

  lines.forEach((line, index) => {
    line = line.trim()
    
    // Empty line
    if (!line) {
      elements.push(<br key={index} />)
      return
    }

    // Bold text **text** or *text*
    if (line.startsWith('* ') || line.match(/^\*\s+/)) {
      const content = line.replace(/^\*\s+/, '').trim()
      elements.push(
        <ul key={index}>
          <li>{renderInlineMarkdown(content)}</li>
        </ul>
      )
    } else {
      elements.push(
        <p key={index}>{renderInlineMarkdown(line)}</p>
      )
    }
  })

  return <div>{elements}</div>
}

/**
 * Render inline markdown (bold, italic, code)
 */
function renderInlineMarkdown(text: string): React.ReactNode[] {
  // Simple regex to handle bold (**text**) and italic (*text*)
  const parts: React.ReactNode[] = []
  let currentIndex = 0
  
  // Handle **bold**
  const boldRegex = /\*\*(.+?)\*\*/g
  let match: RegExpExecArray | null
  const boldMatches: Array<{ start: number, end: number, text: string }> = []
  
  while ((match = boldRegex.exec(text)) !== null) {
    boldMatches.push({
      start: match.index,
      end: match.index + match[0].length,
      text: match[1]
    })
  }

  const sortedMatches = [...boldMatches].sort((a, b) => a.start - b.start)
  
  sortedMatches.forEach((boldMatch, i) => {
    if (boldMatch.start > currentIndex) {
      const part = text.substring(currentIndex, boldMatch.start)
      parts.push(part)
    }
    parts.push(<strong key={i}>{boldMatch.text}</strong>)
    currentIndex = boldMatch.end
  })
  
  if (currentIndex < text.length) {
    parts.push(text.substring(currentIndex))
  }
  
  return parts.length > 0 ? parts : [text]
}

/**
 * Simple text formatter for bullet points and bold/italic
 */
export function formatTextWithMarkdown(text: string): string {
  // Replace markdown bold with <strong> tags
  text = text.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
  
  // Replace markdown italic with <em> tags (but not if it's inside bold)
  text = text.replace(/(?<!\*)\*(?!\*)(.+?)\*(?!\*)/g, '<em>$1</em>')
  
  // Replace bullet points with proper HTML list
  const lines = text.split('\n')
  
  // Remove excessive blank lines (more than 1 consecutive blank line)
  const cleanedLines: string[] = []
  let prevWasBlank = false
  
  lines.forEach(line => {
    const isBlank = line.trim() === ''
    
    if (!isBlank) {
      cleanedLines.push(line)
      prevWasBlank = false
    } else if (!prevWasBlank) {
      // Keep only single blank line
      cleanedLines.push('')
      prevWasBlank = true
    }
    // Skip additional blank lines
  })
  
  const formattedLines: string[] = []
  let inList = false
  let currentParagraph: string[] = []
  
  cleanedLines.forEach((line, index) => {
    line = line.trim()
    
    // Check if line is a bullet point
    if (/^[\*\-•]\s+/.test(line)) {
      // Close current paragraph if exists
      if (currentParagraph.length > 0) {
        if (inList) {
          formattedLines.push('</ul>')
          inList = false
        }
        formattedLines.push(`<p>${currentParagraph.join(' ')}</p>`)
        currentParagraph = []
      }
      
      const content = line.replace(/^[\*\-•]\s+/, '').trim()
      if (!inList) {
        formattedLines.push('<ul>')
        inList = true
      }
      formattedLines.push(`<li>${content}</li>`)
    } else if (line === '') {
      // Empty line - close list if open, flush paragraph
      if (inList) {
        formattedLines.push('</ul>')
        inList = false
      }
      if (currentParagraph.length > 0) {
        formattedLines.push(`<p>${currentParagraph.join(' ')}</p>`)
        currentParagraph = []
      }
    } else {
      // Regular text - add to current paragraph
      if (inList) {
        formattedLines.push('</ul>')
        inList = false
      }
      currentParagraph.push(line)
    }
  })
  
  // Flush remaining paragraph
  if (currentParagraph.length > 0) {
    formattedLines.push(`<p>${currentParagraph.join(' ')}</p>`)
  }
  
  if (inList) {
    formattedLines.push('</ul>')
  }
  
  text = formattedLines.join('')
  
  return text
}

