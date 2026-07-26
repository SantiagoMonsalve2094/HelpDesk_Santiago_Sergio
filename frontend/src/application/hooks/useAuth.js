import { useState } from "react";
import { authRepository } from "../../infrastructure/repositories/authRepository";
import {
  clearStoredSession,
  readStoredSession,
  saveStoredSession,
  saveStoredUser
} from "../../infrastructure/storage/sessionStorage";

export function useAuth() {
  const stored = readStoredSession();
  const [token, setToken] = useState(stored.token);
  const [user, setUser] = useState(stored.user);

  function saveSession(nextToken, nextUser) {
    saveStoredSession(nextToken, nextUser);
    setToken(nextToken);
    setUser(nextUser);
  }

  function logout() {
    clearStoredSession();
    setToken(null);
    setUser(null);
  }

  async function login(credentials) {
    const response = await authRepository.login(credentials);
    saveSession(response.accessToken, response.user);
    return response;
  }

  async function refreshCurrentUser() {
    const me = await authRepository.me(token);
    saveStoredUser(me);
    setUser(me);
    return me;
  }

  return { token, user, login, logout, refreshCurrentUser };
}
