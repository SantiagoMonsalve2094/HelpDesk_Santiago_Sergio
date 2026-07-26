export const priorities = ["low", "medium", "high", "critical"];
export const statuses = ["open", "assigned", "inProgress", "resolved", "closed", "reopened", "overdue"];
export const forceableStatuses = statuses.filter((status) => status !== "overdue");
export const roles = ["user", "technician", "supervisor", "superAdmin"];
export const REOPEN_WINDOW_HOURS = 48;

export const labels = {
  low: "Baja",
  medium: "Media",
  high: "Alta",
  critical: "Crítica",
  open: "Abierto",
  assigned: "Asignado",
  inProgress: "En proceso",
  resolved: "Resuelto",
  closed: "Cerrado",
  reopened: "Reabierto",
  overdue: "Vencido",
  user: "Cliente",
  technician: "Técnico",
  supervisor: "Supervisor",
  superAdmin: "SuperAdmin"
};
