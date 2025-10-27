/**
 * Decode HTML entities to regular text
 * Handles entities like &ndash;, &mdash;, &nbsp;, &amp;, &lt;, &gt;, &quot;, &#39;, etc.
 */
export function decodeHtmlEntities(text: string): string {
  const element = document.createElement('div')
  element.innerHTML = text
  return element.textContent || element.innerText || ''
}

/**
 * Simple HTML entity decoding using regex
 * For cases where DOM is not available
 */
export function decodeHtmlEntitiesSimple(text: string): string {
  return text
    .replace(/&ndash;/g, '–')
    .replace(/&mdash;/g, '—')
    .replace(/&nbsp;/g, ' ')
    .replace(/&amp;/g, '&')
    .replace(/&lt;/g, '<')
    .replace(/&gt;/g, '>')
    .replace(/&quot;/g, '"')
    .replace(/&#39;/g, "'")
    .replace(/&copy;/g, '©')
    .replace(/&reg;/g, '®')
    .replace(/&trade;/g, '™')
    .replace(/&#(\d+);/g, (match, dec) => String.fromCharCode(dec))
    .replace(/&#x([a-fA-F0-9]+);/g, (match, hex) => String.fromCharCode(parseInt(hex, 16)))
}


