import DOMPurify from "dompurify";
import { marked } from "marked";

// Sanitize after Markdown parsing so both legacy HTML and Markdown use one boundary.
export function safeMarkup(content: string): string {
  return DOMPurify.sanitize(marked.parse(content, { async: false }), {
    ALLOWED_TAGS: ["p", "br", "hr", "strong", "em", "del", "s", "u", "sub", "sup",
      "h1", "h2", "h3", "h4", "h5", "h6", "ul", "ol", "li", "blockquote",
      "pre", "code", "a", "img", "table", "thead", "tbody", "tr", "th", "td", "div", "span"],
    ALLOWED_ATTR: ["href", "src", "alt", "title", "colspan", "rowspan"],
    ALLOW_DATA_ATTR: false,
    ALLOW_ARIA_ATTR: false,
  });
}
