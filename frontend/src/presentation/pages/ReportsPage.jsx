import React from "react";
import { formatPercent } from "../../domain/formatters";
import { useSlaReport } from "../../application/hooks/useSla";
export function ReportsPage({ token }) {
  const report = useSlaReport(token);
  const groups = report.data?.groups || [];

  return (
    <section className="panel">
      <div className="section-head">
        <div>
          <p className="eyebrow">Reportes</p>
          <h2>Cumplimiento SLA</h2>
        </div>
        <span className="big-number">
          {formatPercent(report.data?.compliancePercentage)}
        </span>
      </div>
      {report.loading && <p className="muted">Cargando reporte...</p>}
      {report.error && <div className="error-box">{report.error}</div>}
      <div className="report-bars">
        {groups.map((group) => {
          const percent = Number(group.compliancePercentage ?? 0);
          return (
            <div
              className="report-row"
              key={`${group.supportCategoryId}-${group.technicianUserId || "none"}`}
            >
              <div>
                <strong>{group.technicianName}</strong>
                <span>{group.supportCategoryName}</span>
              </div>
              <div className="progress-track">
                <div
                  className="progress-fill"
                  style={{ width: `${Math.min(100, percent)}%` }}
                />
              </div>
              <strong>{formatPercent(group.compliancePercentage)}</strong>
            </div>
          );
        })}
      </div>
    </section>
  );
}

