type HtmlCharMap = {
  [key: string]: string;
};

const abc: HtmlCharMap = {
  '&': '&amp;',
  '<': '&lt;',
  '>': '&gt;',
  "'": '&#39;',
  '"': '&quot;',
};

export function escapeHtml(str: string) {
  return str.replace(/[&<>'"]/g, (tag: string) => {
    return abc[tag] || '';
  });
}
