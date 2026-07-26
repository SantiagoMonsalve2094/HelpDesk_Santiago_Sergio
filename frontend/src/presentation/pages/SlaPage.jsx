import React, { useState } from "react";
import { formatDate, formatPercent, toLabel } from "../../domain/formatters";
import { useSlaAlerts, useSlaReport } from "../../application/hooks/useSla";
import { Metric } from "../components/Feedback";
export function SlaPage({ token }) {
  const [filters, setFilters] = useState({
    supportCategoryId: "",
    technicianUserId: "",
  });
  const report = useSlaReport(token, filters);
  const alerts = useSlaAlerts(token, { pageSize: 20 });

  return (
    <section className="stack">
      <div className="panel">
        <div className="section-head">
          <div>
            <p className="eyebrow">Cumplimiento</p>
            <h2>Reporte SLA</h2>
          </div>
          <span className="big-number">
            {formatPercent(report.data?.compliancePercentage)}
          </span>
        </div>
        <div className="metric-row">
          <Metric label="Cumplidos" value={report.data?.totalMetCycles ?? 0} />
          <Metric
            label="Vencidos"
            value={report.data?.totalBreachedCycles ?? 0}
          />
          <Metric
            label="Pendientes"
            value={report.data?.totalPendingCycles ?? 0}
          />
          <Metric
            label="Evaluados"
            value={report.data?.totalEvaluatedCycles ?? 0}
          />
        </div>
        <div className="table">
          <div className="table-row header">
            <span>Categoría</span>
            <span>Técnico</span>
            <span>Cumplimiento</span>
            <span>Ciclos</span>
          </div>
          {(report.data?.groups || []).map((group) => (
            <div
              className="table-row"
              key={`${group.supportCategoryId}-${group.technicianUserId || "none"}`}
            >
              <span>{group.supportCategoryName}</span>
              <span>{group.technicianName}</span>
              <span>{formatPercent(group.compliancePercentage)}</span>
              <span>
                {group.metCycles}/{group.evaluatedCycles}
              </span>
            </div>
          ))}
        </div>
      </div>
      <div className="panel">
        <div className="section-head">
          <h2>Alertas de vencimiento</h2>
        </div>
        <div className="ticket-list">
          {(alerts.data?.items || []).map((alert) => (
            <div key={alert.ticketId} className="alert-card">
              <strong>
                {alert.ticketNumber} · {alert.subject}
              </strong>
              <span>
                {toLabel(alert.priority)} · {toLabel(alert.status)}
              </span>
              <small>
                {alert.isBreached ? "Vencido" : "Vence"}{" "}
                {formatDate(alert.deadlineAtUtc)}
              </small>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

export function SupervisorAlerts({ token }) {
  const alerts = useSlaAlerts(token, { pageSize: 5 });
  const items = alerts.data?.items || [];
  if (alerts.loading || items.length === 0) return null;

  return (
    <div className="panel compact-alerts">
      <div className="section-head compact">
        <div>
          <p className="eyebrow">SLA</p>
          <h3>Alertas</h3>
        </div>
      </div>
      <div className="ticket-list">
        {items.map((alert) => (
          <div
            key={alert.ticketId}
            className={
              alert.isBreached ? "alert-card overdue-card" : "alert-card"
            }
          >
            <strong>{alert.ticketNumber}</strong>
            <span>{alert.subject}</span>
            <small>
              {alert.isBreached ? "Vencido" : "Vence"}{" "}
              {formatDate(alert.deadlineAtUtc)}
            </small>
          </div>
        ))}
      </div>
    </div>
  );
}

