import { labels } from "./constants";

export function toLabel(value) {
  return labels[value] || value || "Sin dato";
}

export function formatDate(value) {
  if (!value) return "Sin fecha";
  return new Intl.DateTimeFormat("es-CO", {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(new Date(value));
}

export function formatPercent(value) {
  return value === null || value === undefined ? "Sin evaluados" : `${Number(value).toFixed(2)}%`;
}

export function formatDuration(value) {
  if (!value) return "Sin SLA";
  const parts = String(value).split(":").map(Number);
  if (parts.length < 3 || parts.some(Number.isNaN)) return String(value);
  const [hours, minutes] = parts;
  if (hours >= 24 && minutes === 0) return `${Math.round(hours / 24)} días`;
  if (hours > 0 && minutes > 0) return `${hours} h ${minutes} min`;
  if (hours > 0) return `${hours} h`;
  return `${minutes} min`;
}
