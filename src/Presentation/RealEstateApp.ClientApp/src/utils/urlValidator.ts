const ALLOWED_EMBED_HOSTS = [
  'youtube.com',
  'youtu.be',
  'vimeo.com',
  'matterport.com',
  'kuula.co',
  'roundme.com',
];

export function isSafeEmbedUrl(url: string): boolean {
  if (typeof url !== 'string') {
    return false;
  }

  const trimmed = url.trim();
  if (trimmed.length === 0) {
    return false;
  }

  try {
    const parsed = new URL(trimmed);
    if (parsed.protocol !== 'https:') {
      return false;
    }

    const hostname = parsed.hostname.toLowerCase();
    return ALLOWED_EMBED_HOSTS.some(
      (allowedHost) => hostname === allowedHost || hostname.endsWith(`.${allowedHost}`)
    );
  } catch {
    return false;
  }
}
