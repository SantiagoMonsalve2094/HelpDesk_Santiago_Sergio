import { apiRequest } from "../api/apiClient";

export const ticketRepository = {
  list(token, query) {
    return apiRequest("/api/tickets", { token, query });
  },
  getById(token, id) {
    return apiRequest(`/api/tickets/${id}`, { token });
  },
  create(token, ticket) {
    return apiRequest("/api/tickets", { token, method: "POST", body: ticket });
  },
  update(token, id, ticket) {
    return apiRequest(`/api/tickets/${id}`, { token, method: "PUT", body: ticket });
  },
  delete(token, id) {
    return apiRequest(`/api/tickets/${id}`, { token, method: "DELETE" });
  },
  addComment(token, id, text) {
    return apiRequest(`/api/tickets/${id}/comments`, { token, method: "POST", body: { text } });
  },
  assign(token, id, technicianUserId) {
    return apiRequest(`/api/tickets/${id}/assign`, { token, method: "POST", body: { technicianUserId } });
  },
  reassign(token, id, technicianUserId, reason) {
    return apiRequest(`/api/tickets/${id}/reassign`, { token, method: "POST", body: { technicianUserId, reason } });
  },
  start(token, id) {
    return apiRequest(`/api/tickets/${id}/start`, { token, method: "POST" });
  },
  resolve(token, id, resolutionComment) {
    return apiRequest(`/api/tickets/${id}/resolve`, { token, method: "POST", body: { resolutionComment } });
  },
  close(token, id) {
    return apiRequest(`/api/tickets/${id}/close`, { token, method: "POST" });
  },
  reopen(token, id) {
    return apiRequest(`/api/tickets/${id}/reopen`, { token, method: "POST" });
  },
  assignableTechnicians(token, id) {
    return apiRequest(`/api/tickets/${id}/assignable-technicians`, { token });
  }
};
