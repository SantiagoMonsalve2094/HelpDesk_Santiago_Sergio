import { apiRequest } from "../api/apiClient";

export const userRepository = {
  list(token, query) {
    return apiRequest("/api/users", { token, query });
  },
  getById(token, id) {
    return apiRequest(`/api/users/${id}`, { token });
  },
  create(token, user) {
    return apiRequest("/api/users", { token, method: "POST", body: user });
  },
  updateIdentity(token, id, identity) {
    return apiRequest(`/api/users/${id}/identity`, { token, method: "PUT", body: identity });
  },
  resetPassword(token, id, password) {
    return apiRequest(`/api/users/${id}/password`, { token, method: "PUT", body: { password } });
  },
  setActive(token, id, isActive) {
    return apiRequest(`/api/users/${id}/active`, { token, method: "PATCH", body: { isActive } });
  },
  updateTechnicianProfile(token, id, profile) {
    return apiRequest(`/api/users/${id}/technician-profile`, {
      token,
      method: "PUT",
      body: { ...profile, maxActiveTickets: Number(profile.maxActiveTickets) }
    });
  },
  updateSupervisorCategory(token, id, supportCategoryId) {
    return apiRequest(`/api/users/${id}/supervisor-category`, { token, method: "PUT", body: { supportCategoryId } });
  }
};
