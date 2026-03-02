const FALLBACK_IMAGE_URL = 'assets/images/plant-placeholder.svg';
const IMAGE_PROXY_BASE_URL = 'https://images.weserv.nl/?url=';
const UNSPLASH_HOSTS = new Set(['images.unsplash.com', 'source.unsplash.com']);
const DEFAULT_IMAGE_WIDTH = 900;
const DEFAULT_IMAGE_QUALITY = '70';

export function resolvePlantImageUrl(imageUrl?: string | null, preferredWidth = DEFAULT_IMAGE_WIDTH): string {
  const trimmed = (imageUrl ?? '').trim();
  if (!trimmed) {
    return FALLBACK_IMAGE_URL;
  }

  if (trimmed.startsWith(IMAGE_PROXY_BASE_URL)) {
    return trimmed;
  }

  if (trimmed.startsWith('data:') || trimmed.startsWith('blob:') || trimmed.startsWith('assets/')) {
    return trimmed;
  }

  try {
    const normalizedUrl = trimmed.startsWith('//') ? `https:${trimmed}` : trimmed;
    const parsedUrl = new URL(normalizedUrl);

    if (UNSPLASH_HOSTS.has(parsedUrl.hostname)) {
      parsedUrl.searchParams.set('auto', 'format');
      parsedUrl.searchParams.set('fit', 'max');
      parsedUrl.searchParams.set('q', parsedUrl.searchParams.get('q') ?? DEFAULT_IMAGE_QUALITY);
      parsedUrl.searchParams.set('w', parsedUrl.searchParams.get('w') ?? preferredWidth.toString());
      parsedUrl.searchParams.set('dpr', parsedUrl.searchParams.get('dpr') ?? '1');
      return parsedUrl.toString();
    }

    return normalizedUrl;
  } catch {
    return trimmed;
  }
}
