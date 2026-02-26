const FALLBACK_IMAGE_URL = 'assets/images/plant-placeholder.svg';
const PLANTNET_HOST = 'bs.plantnet.org';
const IMAGE_PROXY_BASE_URL = 'https://images.weserv.nl/?url=';

export function resolvePlantImageUrl(imageUrl?: string | null): string {
  const trimmed = (imageUrl ?? '').trim();
  if (!trimmed) {
    return FALLBACK_IMAGE_URL;
  }

  if (trimmed.startsWith(IMAGE_PROXY_BASE_URL)) {
    return trimmed;
  }

  if (trimmed.toLowerCase().includes(PLANTNET_HOST)) {
    return `${IMAGE_PROXY_BASE_URL}${encodeURIComponent(trimmed)}`;
  }

  return trimmed;
}
