import { apiRequest } from "../api/apiClient";

export const authRepository = {
  login(credentials) {
    return apiRequest("/api/auth/login", { method: "POST", body: credentials });
  },
  me(token) {
    return apiRequest("/api/auth/me", { token });
  }
};
