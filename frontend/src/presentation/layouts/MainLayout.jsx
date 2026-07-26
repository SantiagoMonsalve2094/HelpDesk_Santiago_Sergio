import React from "react";
import { Header, Sidebar } from "../components/Layout";

export function MainLayout({
  user,
  view,
  onView,
  onLogout,
  onRefresh,
  refreshing,
  toast,
  children
}) {
  return (
    <div className="app-shell">
      <Sidebar
        user={user}
        view={view}
        onView={onView}
        onLogout={onLogout}
      />
      <main className="workspace">
        {toast && <div className="toast" role="status">{toast}</div>}
        <Header user={user} onRefresh={onRefresh} refreshing={refreshing} />
        {children}
      </main>
    </div>
  );
}
