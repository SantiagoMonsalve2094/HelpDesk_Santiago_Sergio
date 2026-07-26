import { apiRequest } from "../api/apiClient";

export const supportCategoryRepository = {
  list(token, query) {
    return apiRequest("/api/support-categories", { token, query });
  },
  getById(token, id) {
    return apiRequest(`/api/support-categories/${id}`, { token });
  },
  create(token, category) {
    return apiRequest("/api/support-categories", { token, method: "POST", body: category });
  },
  update(token, id, category) {
    return apiRequest(`/api/support-categories/${id}`, { token, method: "PUT", body: category });
  },
  setActive(token, id, isActive) {
    return apiRequest(`/api/support-categories/${id}/active`, { token, method: "PATCH", body: { isActive } });
  },
  updateSla(token, id, priority, responseTimeMinutes) {
    return apiRequest(`/api/support-categories/${id}/sla/${priority}`, {
      token,
      method: "PUT",
      body: { responseTimeMinutes: Number(responseTimeMinutes) }
    });
  }
};
