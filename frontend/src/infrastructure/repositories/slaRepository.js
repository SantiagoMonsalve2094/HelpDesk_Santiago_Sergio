import { apiRequest } from "../api/apiClient";

export const slaRepository = {
  alerts(token, query) {
    return apiRequest("/api/sla/alerts", { token, query });
  },
  report(token, query) {
    return apiRequest("/api/sla/report", { token, query });
  }
};
