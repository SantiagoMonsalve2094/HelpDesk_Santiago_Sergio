import React from "react";
import { toLabel } from "../../domain/formatters";

export function Badge({ type, value }) {
  return <span className={`badge badge-${type}-${value || "default"}`}>{toLabel(value)}</span>;
}

export function EmptyState({ title, description, action }) {
  return (
    <div className="empty-state">
      <span className="empty-state-icon" aria-hidden="true" />
      <strong>{title}</strong>
      <p>{description}</p>
      {action}
    </div>
  );
}

export function Metric({ label, value }) {
  return <div className="metric"><span>{label}</span><strong>{value}</strong></div>;
}

export function HistoryBlock({ title, rows, render }) {
  return (
    <div className="history-block">
      <h3>{title}</h3>
      {(rows || []).length === 0 && <p className="muted">Sin registros.</p>}
      {(rows || []).map((row, index) => <p key={row.id || index}>{render(row)}</p>)}
    </div>
  );
}
