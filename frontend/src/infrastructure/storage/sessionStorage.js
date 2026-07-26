const TOKEN_KEY = "helpdesk.accessToken";
const USER_KEY = "helpdesk.user";

export function readStoredSession() {
  const token = localStorage.getItem(TOKEN_KEY);
  const rawUser = localStorage.getItem(USER_KEY);
  if (!token || !rawUser) return { token: null, user: null };
  try {
    return { token, user: JSON.parse(rawUser) };
  } catch {
    clearStoredSession();
    return { token: null, user: null };
  }
}

export function saveStoredSession(token, user) {
  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

export function saveStoredUser(user) {
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

export function clearStoredSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}
