import React, { useEffect, useState } from "react";
import { createRoot } from "react-dom/client";
import "./styles.css";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:8080";
const TOKEN_KEY = "helpdesk.accessToken";
const USER_KEY = "helpdesk.user";

const priorities = ["low", "medium", "high", "critical"];
const statuses = ["open", "assigned", "inProgress", "resolved", "closed", "reopened", "overdue"];
const roles = ["user", "technician", "supervisor", "superAdmin"];
const REOPEN_WINDOW_HOURS = 48;

const labels = {
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

function toLabel(value) {
  return labels[value] || value || "Sin dato";
}

function isPrivileged(role) {
  return role === "supervisor" || role === "superAdmin";
}

function isTechLike(role) {
  return role === "technician" || isPrivileged(role);
}

function formatDate(value) {
  if (!value) return "Sin fecha";
  return new Intl.DateTimeFormat("es-CO", {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(new Date(value));
}

function formatPercent(value) {
  return value === null || value === undefined ? "Sin evaluados" : `${Number(value).toFixed(2)}%`;
}

function getTicketDeadline(ticket) {
  if (ticket?.slaDeadlineAtUtc) return ticket.slaDeadlineAtUtc;
  const pendingCycle = ticket?.slaCycles?.find((cycle) => cycle.outcome === "pending");
  return pendingCycle?.deadlineAtUtc || ticket?.slaCycles?.[0]?.deadlineAtUtc;
}

function readStoredSession() {
  const token = localStorage.getItem(TOKEN_KEY);
  const rawUser = localStorage.getItem(USER_KEY);
  if (!token || !rawUser) return { token: null, user: null };
  try {
    return { token, user: JSON.parse(rawUser) };
  } catch {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    return { token: null, user: null };
  }
}

async function apiRequest(path, { token, method = "GET", body, query } = {}) {
  const url = new URL(`${API_BASE_URL}${path}`);
  Object.entries(query || {}).forEach(([key, value]) => {
    if (value !== "" && value !== null && value !== undefined) {
      url.searchParams.set(key, value);
    }
  });

  let response;
  try {
    response = await fetch(url, {
      method,
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {})
      },
      body: body ? JSON.stringify(body) : undefined
    });
  } catch {
    throw new Error("No se pudo conectar con la API. Verifica que el backend esté en ejecución.");
  }

  if (response.status === 204) return null;
  const text = await response.text();
  const data = text ? JSON.parse(text) : null;
  if (!response.ok) {
    const message = data?.errors?.[0]?.message || data?.data || data?.title || "La API rechazó la solicitud.";
    throw new Error(normalizeErrorMessage(message));
  }
  return data;
}

function normalizeErrorMessage(message) {
  const fallback = "No se pudo completar la operación.";
  if (!message) return fallback;

  const translations = {
    "Failed to fetch": "No se pudo conectar con la API. Verifica que el backend esté en ejecución.",
    "NetworkError when attempting to fetch resource.": "No se pudo conectar con la API. Verifica que el backend esté en ejecución.",
    "Load failed": "No se pudo conectar con la API. Verifica que el backend esté en ejecución.",
    "The user name or password is incorrect.": "El correo o la contraseña son incorrectos.",
    "Invalid credentials.": "El correo o la contraseña son incorrectos.",
    Unauthorized: "No tienes una sesión válida. Inicia sesión nuevamente.",
    Forbidden: "No tienes permisos para realizar esta acción.",
    "Not Found": "No se encontró el recurso solicitado."
  };

  return translations[message] || message || fallback;
}

function hasBlankFields(...values) {
  return values.some((value) => {
    if (Array.isArray(value)) return value.length === 0;
    return value === null || value === undefined || String(value).trim() === "";
  });
}

function useNow(intervalMs = 30000) {
  const [now, setNow] = useState(() => new Date());
  useEffect(() => {
    const timer = window.setInterval(() => setNow(new Date()), intervalMs);
    return () => window.clearInterval(timer);
  }, [intervalMs]);
  return now;
}

function routeToView(pathname) {
  if (pathname === "/reports") return "reports";
  return "tickets";
}

function formatDuration(value) {
  if (!value) return "Sin SLA";
  const parts = String(value).split(":").map(Number);
  if (parts.length < 3 || parts.some(Number.isNaN)) return String(value);
  const [hours, minutes] = parts;
  if (hours >= 24 && minutes === 0) return `${Math.round(hours / 24)} días`;
  if (hours > 0 && minutes > 0) return `${hours} h ${minutes} min`;
  if (hours > 0) return `${hours} h`;
  return `${minutes} min`;
}

function getCurrentSlaCycle(ticket) {
  return ticket?.slaCycles?.find((cycle) => cycle.outcome === "pending") || ticket?.slaCycles?.[0] || null;
}

function isTicketOverdue(ticket, now = new Date()) {
  if (!ticket || ticket.status === "resolved" || ticket.status === "closed") return false;
  const deadline = getTicketDeadline(ticket);
  return Boolean(ticket.isOverdue || (deadline && new Date(deadline) < now));
}

function getEffectiveStatus(ticket, now = new Date()) {
  return isTicketOverdue(ticket, now) ? "overdue" : ticket?.status;
}

function getVisibleTickets(tickets, user) {
  if (!user) return [];
  if (user.role === "user") return tickets.filter((ticket) => ticket.creatorUserId === user.id || ticket.ownerId === user.id);
  if (user.role === "technician") {
    return tickets.filter((ticket) => ticket.currentTechnicianUserId === user.id || ticket.assigneeId === user.id);
  }
  return tickets;
}

function canAssign(ticket, user) {
  return isPrivileged(user.role) && ["open", "reopened"].includes(ticket.status);
}

function canReassign(ticket, user) {
  return isPrivileged(user.role) && ["assigned", "inProgress", "reopened"].includes(ticket.status) && Boolean(ticket.currentTechnicianUserId);
}

function canStart(ticket, user) {
  return isTechLike(user.role) && ["assigned", "reopened"].includes(ticket.status);
}

function canResolve(ticket, user) {
  return isTechLike(user.role) && ["inProgress", "reopened"].includes(ticket.status);
}

function canClose(ticket, user) {
  return ticket.status === "resolved" && ticket.creatorUserId === user.id;
}

function canReopen(ticket, user, now = new Date()) {
  if (user.role !== "user" || ticket.creatorUserId !== user.id || ticket.status !== "resolved" || !ticket.resolvedAtUtc) {
    return false;
  }
  const reopenUntil = new Date(ticket.resolvedAtUtc);
  reopenUntil.setHours(reopenUntil.getHours() + REOPEN_WINDOW_HOURS);
  return now <= reopenUntil;
}

function technicianSupportsTicket(technician, ticket) {
  const specialties = technician.supportCategoryIds || technician.specialties || technician.specialityIds;
  if (!Array.isArray(specialties) || specialties.length === 0) return true;
  return specialties.some((value) => String(value).toLowerCase() === String(ticket.supportCategoryId).toLowerCase());
}

function hasAvailableCapacity(technician) {
  if (typeof technician.availableCapacity === "number") return technician.availableCapacity > 0;
  if (typeof technician.activeTicketCount === "number" && typeof technician.maxActiveTickets === "number") {
    return technician.activeTicketCount < technician.maxActiveTickets;
  }
  return true;
}

function useAsyncData(loader, deps) {
  const [state, setState] = useState({ loading: true, error: "", data: null });
  useEffect(() => {
    let alive = true;
    setState((current) => ({ ...current, loading: true, error: "" }));
    loader()
      .then((data) => alive && setState({ loading: false, error: "", data }))
      .catch((error) => alive && setState({ loading: false, error: error.message, data: null }));
    return () => {
      alive = false;
    };
  }, deps);
  return state;
}

function App() {
  const stored = readStoredSession();
  const [token, setToken] = useState(stored.token);
  const [user, setUser] = useState(stored.user);
  const [view, setView] = useState(() => routeToView(window.location.pathname));
  const [toast, setToast] = useState("");

  useEffect(() => {
    const onPopState = () => setView(routeToView(window.location.pathname));
    window.addEventListener("popstate", onPopState);
    return () => window.removeEventListener("popstate", onPopState);
  }, []);

  function navigate(nextView) {
    const path = nextView === "reports" ? "/reports" : "/";
    window.history.pushState({}, "", path);
    setView(nextView);
  }

  function saveSession(nextToken, nextUser) {
    localStorage.setItem(TOKEN_KEY, nextToken);
    localStorage.setItem(USER_KEY, JSON.stringify(nextUser));
    setToken(nextToken);
    setUser(nextUser);
  }

  function logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    setToken(null);
    setUser(null);
    setView("tickets");
  }

  async function refreshCurrentUser() {
    const me = await apiRequest("/api/auth/me", { token });
    localStorage.setItem(USER_KEY, JSON.stringify(me));
    setUser(me);
    return me;
  }

  if (!token || !user) {
    return <LoginScreen onLogin={saveSession} />;
  }

  return (
    <div className="app-shell">
      <Sidebar user={user} view={view} onView={navigate} onLogout={logout} />
      <main className="workspace">
        {toast && <div className="toast">{toast}</div>}
        <Header user={user} onRefresh={refreshCurrentUser} />
        {view === "tickets" && <TicketsView token={token} user={user} notify={setToast} />}
        {view === "categories" && <CategoriesView token={token} user={user} notify={setToast} />}
        {view === "sla" && isPrivileged(user.role) && <SlaView token={token} />}
        {view === "reports" && user.role === "supervisor" && <ReportsView token={token} />}
        {view === "users" && user.role === "superAdmin" && <UsersView token={token} notify={setToast} />}
      </main>
    </div>
  );
}

function LoginScreen({ onLogin }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function submit(event) {
    event.preventDefault();
    if (hasBlankFields(email, password)) {
      setError("Ingresa el correo electrónico y la contraseña.");
      return;
    }
    setLoading(true);
    setError("");
    try {
      const response = await apiRequest("/api/auth/login", {
        method: "POST",
        body: { email, password }
      });
      onLogin(response.accessToken, response.user);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="login-page">
      <section className="login-panel">
        <div>
          <p className="eyebrow">Mesa de ayuda</p>
          <h1>HelpDesk</h1>
          <p className="muted">Gestión de tickets, SLA, técnicos y categorías.</p>
        </div>
        <form onSubmit={submit} className="stack" noValidate>
          <label>
            Correo electrónico
            <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
          </label>
          <label>
            Contraseña
            <input value={password} onChange={(event) => setPassword(event.target.value)} type="password" required />
          </label>
          {error && <div className="error-box">{error}</div>}
          <button className="primary-button" disabled={loading}>{loading ? "Ingresando..." : "Ingresar"}</button>
        </form>
      </section>
    </main>
  );
}

function Sidebar({ user, view, onView, onLogout }) {
  const items = [
    ["tickets", "Tickets"],
    ["categories", "Categorías"],
    ...(isPrivileged(user.role) ? [["sla", "SLA"]] : []),
    ...(user.role === "supervisor" ? [["reports", "Reportes"]] : []),
    ...(user.role === "superAdmin" ? [["users", "Usuarios"]] : [])
  ];
  return (
    <aside className="sidebar">
      <div>
        <div className="brand">HelpDesk</div>
        <nav>
          {items.map(([id, label]) => (
            <button key={id} className={view === id ? "nav-item active" : "nav-item"} onClick={() => onView(id)}>
              {label}
            </button>
          ))}
        </nav>
      </div>
      <button className="ghost-button" onClick={onLogout}>Salir</button>
    </aside>
  );
}

function Header({ user, onRefresh }) {
  return (
    <header className="topbar">
      <div>
        <h2>{user.fullName}</h2>
        <p>{user.email} · {toLabel(user.role)}</p>
      </div>
      <button className="secondary-button" onClick={onRefresh}>Actualizar sesión</button>
    </header>
  );
}

function TicketsView({ token, user, notify }) {
  const [filters, setFilters] = useState({ status: "", priority: "", isOverdue: "" });
  const [selectedId, setSelectedId] = useState("");
  const [refreshKey, setRefreshKey] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const now = useNow();
  const tickets = useAsyncData(
    () => apiRequest("/api/tickets", { token, query: { ...filters, pageSize: 50 } }),
    [token, refreshKey, filters.status, filters.priority, filters.isOverdue]
  );

  function refresh(message) {
    if (message) notify(message);
    setRefreshKey((value) => value + 1);
  }

  const items = getVisibleTickets(tickets.data?.items || [], user);
  const selected = selectedId || items[0]?.id || "";
  const canCreateTicket = user.role !== "technician";

  return (
    <section className="grid-page">
      <div className="stack">
      {user.role === "supervisor" && <SupervisorAlerts token={token} />}
      <div className="panel list-panel">
        <div className="section-head">
          <div>
            <p className="eyebrow">Operación</p>
            <h2>Tickets</h2>
          </div>
          {canCreateTicket && <button className="primary-button" onClick={() => setShowCreate((value) => !value)}>Nuevo</button>}
        </div>
        <div className="filters">
          <select value={filters.status} onChange={(event) => setFilters({ ...filters, status: event.target.value })}>
            <option value="">Estado</option>
            {statuses.map((status) => <option key={status} value={status}>{toLabel(status)}</option>)}
          </select>
          <select value={filters.priority} onChange={(event) => setFilters({ ...filters, priority: event.target.value })}>
            <option value="">Prioridad</option>
            {priorities.map((priority) => <option key={priority} value={priority}>{toLabel(priority)}</option>)}
          </select>
          <select value={filters.isOverdue} onChange={(event) => setFilters({ ...filters, isOverdue: event.target.value })}>
            <option value="">SLA</option>
            <option value="true">Vencidos</option>
            <option value="false">En plazo</option>
          </select>
        </div>
        {showCreate && <CreateTicketForm token={token} onDone={() => { setShowCreate(false); refresh("Ticket creado."); }} />}
        {tickets.loading && <p className="muted">Cargando tickets...</p>}
        {tickets.error && <div className="error-box">{tickets.error}</div>}
        <div className="ticket-list">
          {items.map((ticket) => (
            <button
              key={ticket.id}
              className={[
                "ticket-card",
                selected === ticket.id ? "selected" : "",
                isTicketOverdue(ticket, now) ? "overdue-card" : ""
              ].filter(Boolean).join(" ")}
              onClick={() => setSelectedId(ticket.id)}
            >
              <span className="ticket-card-main">
                <span className="ticket-number">{ticket.ticketNumber}</span>
                <strong className="ticket-title" title={ticket.subject}>{ticket.subject}</strong>
              </span>
              <span className="ticket-card-badges">
                <Badge type="status" value={getEffectiveStatus(ticket, now)} />
                <Badge type="priority" value={ticket.priority} />
              </span>
              <small className="ticket-date">Vence {formatDate(ticket.slaDeadlineAtUtc)}</small>
            </button>
          ))}
          {!tickets.loading && items.length === 0 && (
            <EmptyState
              title="No hay tickets para mostrar"
              description="Cuando existan solicitudes disponibles para tu rol aparecerán en esta lista."
              action={canCreateTicket ? <button className="secondary-button" onClick={() => setShowCreate(true)}>Crear ticket</button> : null}
            />
          )}
        </div>
      </div>
      </div>
      <TicketDetail token={token} user={user} ticketId={selected} onChanged={refresh} />
    </section>
  );
}

function CreateTicketForm({ token, onDone }) {
  const [form, setForm] = useState({ subject: "", description: "", priority: "", supportCategoryId: "" });
  const categories = useAsyncData(() => apiRequest("/api/support-categories", { token, query: { pageSize: 100 } }), [token]);
  const [error, setError] = useState("");

  async function submit(event) {
    event.preventDefault();
    setError("");
    if (hasBlankFields(form.subject, form.description, form.supportCategoryId, form.priority)) {
      setError("Completa el asunto, la descripción y la categoría.");
      return;
    }
    try {
      await apiRequest("/api/tickets", { token, method: "POST", body: form });
      onDone();
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <form className="inline-form" onSubmit={submit} noValidate>
      <input placeholder="Asunto" value={form.subject} onChange={(e) => setForm({ ...form, subject: e.target.value })} required />
      <textarea placeholder="Descripción" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} required />
      <select value={form.supportCategoryId} onChange={(e) => setForm({ ...form, supportCategoryId: e.target.value })} required>
        <option value="">Categoría</option>
        {(categories.data?.items || []).map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
      </select>
      <select value={form.priority} onChange={(e) => setForm({ ...form, priority: e.target.value })}>
        <option value="">Prioridad</option>
        {priorities.map((priority) => <option key={priority} value={priority}>{toLabel(priority)}</option>)}
      </select>
      {error && <div className="error-box">{error}</div>}
      <button className="primary-button">Crear ticket</button>
    </form>
  );
}

function TicketDetail({ token, user, ticketId, onChanged }) {
  const [refreshKey, setRefreshKey] = useState(0);
  const detail = useAsyncData(
    () => ticketId ? apiRequest(`/api/tickets/${ticketId}`, { token }) : Promise.resolve(null),
    [token, ticketId, refreshKey]
  );
  const now = useNow();

  if (!ticketId) return <div className="panel detail-panel"><p className="muted">No hay tickets para mostrar.</p></div>;
  if (detail.loading) return <div className="panel detail-panel"><p className="muted">Cargando detalle...</p></div>;
  if (detail.error) return <div className="panel detail-panel"><div className="error-box">{detail.error}</div></div>;
  if (!detail.data) return <div className="panel detail-panel"><p className="muted">Selecciona un ticket para ver el detalle.</p></div>;

  const ticket = detail.data;
  const currentSla = getCurrentSlaCycle(ticket);
  const effectiveStatus = getEffectiveStatus(ticket, now);
  const refresh = (message) => {
    setRefreshKey((value) => value + 1);
    onChanged(message);
  };

  return (
    <div className="panel detail-panel">
      <div className="section-head">
        <div>
          <p className="eyebrow">{ticket.ticketNumber}</p>
          <h2>{ticket.subject}</h2>
        </div>
        <Badge type="status" value={effectiveStatus} />
      </div>
      <p className="detail-description">{ticket.description}</p>
      <div className="metric-row">
        <Metric label="Prioridad" value={toLabel(ticket.priority)} />
        <Metric label="Estado" value={toLabel(effectiveStatus)} />
        <Metric label="Fecha SLA" value={formatDate(getTicketDeadline(ticket))} />
        <Metric label="Tiempo máximo" value={formatDuration(currentSla?.duration)} />
      </div>
      <SlaCountdown ticket={ticket} now={now} />
      <TicketActions token={token} user={user} ticket={ticket} onDone={refresh} />
      <div className="two-columns">
        <HistoryBlock title="Comentarios" rows={ticket.comments} render={(row) => `${formatDate(row.createdAtUtc)} - ${row.authorUserId}: ${row.body}`} />
        <HistoryBlock title="Estados" rows={ticket.statusHistory} render={(row) => `${formatDate(row.changedAtUtc)} - ${toLabel(row.newStatus)}`} />
      </div>
    </div>
  );
}

function SlaCountdown({ ticket, now }) {
  const deadline = getTicketDeadline(ticket);
  if (!deadline) return null;
  const remainingMs = new Date(deadline).getTime() - now.getTime();
  const isOverdue = isTicketOverdue(ticket, now);
  const absMinutes = Math.max(0, Math.floor(Math.abs(remainingMs) / 60000));
  const hours = Math.floor(absMinutes / 60);
  const minutes = absMinutes % 60;
  const text = isOverdue
    ? `SLA superado hace ${hours} h ${minutes} min`
    : `Tiempo restante: ${hours} h ${minutes} min`;

  return (
    <div className={isOverdue ? "sla-banner danger" : "sla-banner"}>
      <strong>{isOverdue ? "Alerta de vencimiento" : "SLA en curso"}</strong>
      <span>{text}</span>
      <small>Fecha límite: {formatDate(deadline)}</small>
    </div>
  );
}

function TicketActions({ token, user, ticket, onDone }) {
  const [comment, setComment] = useState("");
  const [resolution, setResolution] = useState("");
  const [closeComment, setCloseComment] = useState("");
  const [assignTo, setAssignTo] = useState("");
  const [reason, setReason] = useState("");
  const [error, setError] = useState("");
  const now = useNow();
  const technicians = useAsyncData(
    () => isPrivileged(user.role) ? apiRequest(`/api/tickets/${ticket.id}/assignable-technicians`, { token }) : Promise.resolve([]),
    [token, ticket.id, user.role]
  );
  const availableTechnicians = (technicians.data || []).filter((technician) => technicianSupportsTicket(technician, ticket));

  async function post(path, body, message) {
    setError("");
    if (body && hasBlankFields(...Object.values(body))) {
      setError("Completa los campos obligatorios.");
      return false;
    }
    try {
      await apiRequest(path, { token, method: "POST", body });
      setComment("");
      setResolution("");
      setCloseComment("");
      setReason("");
      onDone(message);
      return true;
    } catch (err) {
      setError(err.message);
      return false;
    }
  }

  async function closeTicket(event) {
    event.preventDefault();
    const commentSaved = await post(`/api/tickets/${ticket.id}/comments`, { text: closeComment }, "Comentario de cierre registrado.");
    if (commentSaved) {
      await post(`/api/tickets/${ticket.id}/close`, null, "Ticket cerrado.");
    }
  }

  const shouldShowAssignment = canAssign(ticket, user) || canReassign(ticket, user);
  const isReassignment = canReassign(ticket, user);

  return (
    <div className="actions">
      {error && <div className="error-box">{error}</div>}
      <div className="action-grid">
        <form noValidate onSubmit={(e) => { e.preventDefault(); post(`/api/tickets/${ticket.id}/comments`, { text: comment }, "Comentario registrado."); }}>
          <input value={comment} onChange={(e) => setComment(e.target.value)} placeholder="Comentario" required />
          <button className="secondary-button">Comentar</button>
        </form>
        {canResolve(ticket, user) && (
          <form noValidate onSubmit={(e) => { e.preventDefault(); post(`/api/tickets/${ticket.id}/resolve`, { resolutionComment: resolution }, "Ticket resuelto."); }}>
            <input value={resolution} onChange={(e) => setResolution(e.target.value)} placeholder="Comentario de resolución" required />
            <button className="secondary-button">Resolver</button>
          </form>
        )}
        {shouldShowAssignment && (
          <form noValidate onSubmit={(e) => { e.preventDefault(); post(`/api/tickets/${ticket.id}/${isReassignment ? "reassign" : "assign"}`, isReassignment ? { technicianUserId: assignTo, reason } : { technicianUserId: assignTo }, "Asignación actualizada."); }}>
            <select value={assignTo} onChange={(e) => setAssignTo(e.target.value)} required>
              <option value="">Técnico</option>
              {availableTechnicians.map((tech) => (
                <option key={tech.technicianUserId} value={tech.technicianUserId} disabled={!hasAvailableCapacity(tech)}>
                  {tech.fullName} ({tech.activeTicketCount}/{tech.maxActiveTickets}) {!hasAvailableCapacity(tech) ? " - Capacidad Máxima" : ""}
                </option>
              ))}
            </select>
            {isReassignment && <input value={reason} onChange={(e) => setReason(e.target.value)} placeholder="Motivo" required />}
            <button className="secondary-button">{isReassignment ? "Reasignar" : "Asignar"}</button>
          </form>
        )}
      </div>
      <div className="button-row">
        {canStart(ticket, user) && <button className="ghost-button" onClick={() => post(`/api/tickets/${ticket.id}/start`, null, "Ticket en proceso.")}>Iniciar</button>}
        {canReopen(ticket, user, now) && <button className="ghost-button" onClick={() => post(`/api/tickets/${ticket.id}/reopen`, null, "Ticket reabierto.")}>Reabrir</button>}
      </div>
      {canClose(ticket, user) && (
        <form className="close-form" noValidate onSubmit={closeTicket}>
          <textarea value={closeComment} onChange={(e) => setCloseComment(e.target.value)} placeholder="Comentario de cierre" />
          <button className="primary-button" disabled={closeComment.trim().length === 0}>Cerrar Ticket</button>
        </form>
      )}
    </div>
  );
}
function CategoriesView({ token, user, notify }) {
  const [refreshKey, setRefreshKey] = useState(0);
  const categories = useAsyncData(() => apiRequest("/api/support-categories", { token, query: { includeInactive: true, pageSize: 100 } }), [token, refreshKey]);
  const [form, setForm] = useState({ name: "", description: "", lowSlaMinutes: 1440, mediumSlaMinutes: 480, highSlaMinutes: 240, criticalSlaMinutes: 120 });
  const [error, setError] = useState("");

  async function create(event) {
    event.preventDefault();
    setError("");
    if (hasBlankFields(form.name, form.description)) {
      setError("Completa el nombre y la descripción de la categoría.");
      return;
    }
    try {
      await apiRequest("/api/support-categories", { token, method: "POST", body: form });
      setForm({ name: "", description: "", lowSlaMinutes: 1440, mediumSlaMinutes: 480, highSlaMinutes: 240, criticalSlaMinutes: 120 });
      setRefreshKey((value) => value + 1);
      notify("Categoría creada.");
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <section className="panel">
      <div className="section-head">
        <div>
          <p className="eyebrow">Catálogo</p>
          <h2>Categorías y SLA</h2>
        </div>
      </div>
      {user.role === "superAdmin" && (
        <form className="category-form" onSubmit={create} noValidate>
          <input placeholder="Nombre" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
          <input placeholder="Descripción" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} required />
          {priorities.map((priority) => (
            <label key={priority}>
              SLA {toLabel(priority)} (min)
              <input type="number" min="1" value={form[`${priority}SlaMinutes`]} onChange={(e) => setForm({ ...form, [`${priority}SlaMinutes`]: Number(e.target.value) })} />
            </label>
          ))}
          {error && <div className="error-box">{error}</div>}
          <button className="primary-button">Crear categoría</button>
        </form>
      )}
      {categories.loading && <p className="muted">Cargando categorías...</p>}
      <div className="cards-grid">
        {(categories.data?.items || []).map((category) => (
          <CategoryCard key={category.id} token={token} user={user} category={category} onChanged={() => setRefreshKey((value) => value + 1)} />
        ))}
      </div>
    </section>
  );
}

function CategoryCard({ token, user, category, onChanged }) {
  const detail = useAsyncData(() => apiRequest(`/api/support-categories/${category.id}`, { token }), [token, category.id]);
  const [sla, setSla] = useState({ priority: "critical", responseTimeMinutes: 120 });
  const [error, setError] = useState("");

  async function updateSla(event) {
    event.preventDefault();
    setError("");
    try {
      await apiRequest(`/api/support-categories/${category.id}/sla/${sla.priority}`, { token, method: "PUT", body: { responseTimeMinutes: Number(sla.responseTimeMinutes) } });
      onChanged();
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <article className="data-card">
      <div className="section-head compact">
        <div>
          <h3>{category.name}</h3>
          <p>{category.description}</p>
        </div>
        <span className="status">{category.isActive ? "Activa" : "Inactiva"}</span>
      </div>
      <div className="sla-list">
        {(detail.data?.slaPolicies || []).map((policy) => (
          <span key={policy.id}>{toLabel(policy.priority)}: {policy.responseTime}</span>
        ))}
      </div>
      {isPrivileged(user.role) && (
        <form className="mini-form" onSubmit={updateSla} noValidate>
          <select value={sla.priority} onChange={(e) => setSla({ ...sla, priority: e.target.value })}>
            {priorities.map((priority) => <option key={priority} value={priority}>{toLabel(priority)}</option>)}
          </select>
          <input type="number" min="1" value={sla.responseTimeMinutes} onChange={(e) => setSla({ ...sla, responseTimeMinutes: e.target.value })} />
          <button className="secondary-button">Actualizar SLA</button>
        </form>
      )}
      {error && <div className="error-box">{error}</div>}
    </article>
  );
}

function SlaView({ token }) {
  const [filters, setFilters] = useState({ supportCategoryId: "", technicianUserId: "" });
  const report = useAsyncData(() => apiRequest("/api/sla/report", { token, query: filters }), [token, filters.supportCategoryId, filters.technicianUserId]);
  const alerts = useAsyncData(() => apiRequest("/api/sla/alerts", { token, query: { pageSize: 20 } }), [token]);

  return (
    <section className="stack">
      <div className="panel">
        <div className="section-head">
          <div>
            <p className="eyebrow">Cumplimiento</p>
            <h2>Reporte SLA</h2>
          </div>
          <span className="big-number">{formatPercent(report.data?.compliancePercentage)}</span>
        </div>
        <div className="metric-row">
          <Metric label="Cumplidos" value={report.data?.totalMetCycles ?? 0} />
          <Metric label="Vencidos" value={report.data?.totalBreachedCycles ?? 0} />
          <Metric label="Pendientes" value={report.data?.totalPendingCycles ?? 0} />
          <Metric label="Evaluados" value={report.data?.totalEvaluatedCycles ?? 0} />
        </div>
        <div className="table">
          <div className="table-row header"><span>Categoría</span><span>Técnico</span><span>Cumplimiento</span><span>Ciclos</span></div>
          {(report.data?.groups || []).map((group) => (
            <div className="table-row" key={`${group.supportCategoryId}-${group.technicianUserId || "none"}`}>
              <span>{group.supportCategoryName}</span>
              <span>{group.technicianName}</span>
              <span>{formatPercent(group.compliancePercentage)}</span>
              <span>{group.metCycles}/{group.evaluatedCycles}</span>
            </div>
          ))}
        </div>
      </div>
      <div className="panel">
        <div className="section-head"><h2>Alertas de vencimiento</h2></div>
        <div className="ticket-list">
          {(alerts.data?.items || []).map((alert) => (
            <div key={alert.ticketId} className="alert-card">
              <strong>{alert.ticketNumber} · {alert.subject}</strong>
              <span>{toLabel(alert.priority)} · {toLabel(alert.status)}</span>
              <small>{alert.isBreached ? "Vencido" : "Vence"} {formatDate(alert.deadlineAtUtc)}</small>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

function SupervisorAlerts({ token }) {
  const alerts = useAsyncData(() => apiRequest("/api/sla/alerts", { token, query: { pageSize: 5 } }), [token]);
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
          <div key={alert.ticketId} className={alert.isBreached ? "alert-card overdue-card" : "alert-card"}>
            <strong>{alert.ticketNumber}</strong>
            <span>{alert.subject}</span>
            <small>{alert.isBreached ? "Vencido" : "Vence"} {formatDate(alert.deadlineAtUtc)}</small>
          </div>
        ))}
      </div>
    </div>
  );
}

function ReportsView({ token }) {
  const report = useAsyncData(() => apiRequest("/api/sla/report", { token }), [token]);
  const groups = report.data?.groups || [];

  return (
    <section className="panel">
      <div className="section-head">
        <div>
          <p className="eyebrow">Reportes</p>
          <h2>Cumplimiento SLA</h2>
        </div>
        <span className="big-number">{formatPercent(report.data?.compliancePercentage)}</span>
      </div>
      {report.loading && <p className="muted">Cargando reporte...</p>}
      {report.error && <div className="error-box">{report.error}</div>}
      <div className="report-bars">
        {groups.map((group) => {
          const percent = Number(group.compliancePercentage ?? 0);
          return (
            <div className="report-row" key={`${group.supportCategoryId}-${group.technicianUserId || "none"}`}>
              <div>
                <strong>{group.technicianName}</strong>
                <span>{group.supportCategoryName}</span>
              </div>
              <div className="progress-track">
                <div className="progress-fill" style={{ width: `${Math.min(100, percent)}%` }} />
              </div>
              <strong>{formatPercent(group.compliancePercentage)}</strong>
            </div>
          );
        })}
      </div>
    </section>
  );
}

function UsersView({ token, notify }) {
  const [refreshKey, setRefreshKey] = useState(0);
  const users = useAsyncData(() => apiRequest("/api/users", { token, query: { pageSize: 100 } }), [token, refreshKey]);
  const categories = useAsyncData(() => apiRequest("/api/support-categories", { token, query: { pageSize: 100 } }), [token]);
  const [form, setForm] = useState({ fullName: "", email: "", password: "", role: "user", supportCategoryIds: [], maxActiveTickets: 5 });
  const [error, setError] = useState("");

  async function create(event) {
    event.preventDefault();
    setError("");
    if (hasBlankFields(form.fullName, form.email, form.password, form.role)) {
      setError("Completa el nombre, el correo, la contraseña y el rol.");
      return;
    }
    if ((form.role === "technician" || form.role === "supervisor") && hasBlankFields(form.supportCategoryIds)) {
      setError("Selecciona al menos una categoría para el perfil.");
      return;
    }
    try {
      await apiRequest("/api/users", { token, method: "POST", body: form });
      setForm({ fullName: "", email: "", password: "", role: "user", supportCategoryIds: [], maxActiveTickets: 5 });
      setRefreshKey((value) => value + 1);
      notify("Usuario creado.");
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <section className="panel">
      <div className="section-head">
        <div>
          <p className="eyebrow">Administración</p>
          <h2>Usuarios</h2>
        </div>
      </div>
      <form className="category-form" onSubmit={create} noValidate>
        <input placeholder="Nombre completo" value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} required />
        <input placeholder="Correo electrónico" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
        <input placeholder="Contraseña" type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} required />
        <select value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value, supportCategoryIds: [] })}>
          {roles.map((role) => <option key={role} value={role}>{toLabel(role)}</option>)}
        </select>
        {(form.role === "technician" || form.role === "supervisor") && (
          <select multiple value={form.supportCategoryIds} onChange={(e) => setForm({ ...form, supportCategoryIds: Array.from(e.target.selectedOptions, (option) => option.value) })}>
            {(categories.data?.items || []).map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
          </select>
        )}
        {form.role === "technician" && <input type="number" min="1" value={form.maxActiveTickets} onChange={(e) => setForm({ ...form, maxActiveTickets: Number(e.target.value) })} />}
        {error && <div className="error-box">{error}</div>}
        <button className="primary-button">Crear usuario</button>
      </form>
      <div className="table">
        <div className="table-row header"><span>Nombre</span><span>Correo</span><span>Rol</span><span>Activo</span></div>
        {(users.data?.items || []).map((item) => (
          <div className="table-row" key={item.id}><span>{item.fullName}</span><span>{item.email}</span><span>{toLabel(item.role)}</span><span>{item.isActive ? "Sí" : "No"}</span></div>
        ))}
      </div>
    </section>
  );
}

function Badge({ type, value }) {
  return <span className={`badge badge-${type}-${value || "default"}`}>{toLabel(value)}</span>;
}

function EmptyState({ title, description, action }) {
  return (
    <div className="empty-state">
      <span className="empty-state-icon" aria-hidden="true" />
      <strong>{title}</strong>
      <p>{description}</p>
      {action}
    </div>
  );
}

function Metric({ label, value }) {
  return <div className="metric"><span>{label}</span><strong>{value}</strong></div>;
}

function HistoryBlock({ title, rows, render }) {
  return (
    <div className="history-block">
      <h3>{title}</h3>
      {(rows || []).length === 0 && <p className="muted">Sin registros.</p>}
      {(rows || []).map((row, index) => <p key={row.id || index}>{render(row)}</p>)}
    </div>
  );
}

createRoot(document.getElementById("root")).render(<App />);



