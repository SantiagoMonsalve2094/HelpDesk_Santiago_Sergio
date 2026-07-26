import React from "react";
import { isPrivileged } from "../../domain/ticketRules";
import { toLabel } from "../../domain/formatters";

export function Sidebar({ user, view, onView, onLogout }) {
  const items = [
    ["tickets", "Tickets"],
    ...(["supervisor", "superAdmin"].includes(user.role)
      ? [["categories", "Categorías"]]
      : []),
    ...(isPrivileged(user.role) ? [["sla", "SLA"]] : []),
    ...(user.role === "supervisor" ? [["reports", "Reportes"]] : []),
    ...(user.role === "superAdmin" ? [["users", "Usuarios"]] : [])
  ];

  return (
    <aside className="topbar-nav">
      <div>
        <div className="brand">HelpDesk</div>
        <nav>
          {items.map(([id, label]) => (
            <button
              key={id}
              type="button"
              className={view === id ? "nav-item active" : "nav-item"}
              onClick={() => onView(id)}
            >
              {label}
            </button>
          ))}
        </nav>
      </div>
      <button type="button" className="ghost-button" onClick={onLogout}>
        Salir
      </button>
    </aside>
  );
}

export function Header({ user, onRefresh, refreshing }) {
  return (
    <header className="topbar">
      <div>
        <h2>{user.fullName}</h2>
        <p>{user.email} · {toLabel(user.role)}</p>
      </div>
      <button
        type="button"
        className="secondary-button"
        onClick={onRefresh}
        disabled={refreshing}
      >
        {refreshing ? "Actualizando..." : "Actualizar sesión"}
      </button>
    </header>
  );
}
