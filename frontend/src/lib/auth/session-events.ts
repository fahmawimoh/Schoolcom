type SessionExpiredCallback = (translationKey?: string) => void;

let onSessionExpired: SessionExpiredCallback | null = null;
let sessionExpiryInProgress = false;

export function registerSessionExpiredHandler(
  handler: SessionExpiredCallback
): () => void {
  onSessionExpired = handler;
  return () => {
    if (onSessionExpired === handler) onSessionExpired = null;
  };
}

export function notifySessionExpired(translationKey?: string): void {
  if (sessionExpiryInProgress) return;
  sessionExpiryInProgress = true;
  onSessionExpired?.(translationKey);
}

export function isSessionExpiryInProgress(): boolean {
  return sessionExpiryInProgress;
}

export function resetSessionExpiryInProgress(): void {
  sessionExpiryInProgress = false;
}
