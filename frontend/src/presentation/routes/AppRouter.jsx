import React, { useEffect, useState } from "react";
import { MainLayout } from "../layouts/MainLayout";
import { useAppAuth } from "../providers/AppProviders";
import {
  CategoriesPage,
  LoginPage,
  ReportsPage,
  SlaPage,
  TicketsPage,
  UsersPage
} from "../pages";

const protectedRoutes = {
  "/": { view: "tickets" },
  "/categories": { view: "categories", roles: ["supervisor", "superAdmin"] },
  "/sla": { view: "sla", roles: ["supervisor", "superAdmin"] },
  "/reports": { view: "reports", roles: ["supervisor"] },
  "/users": { view: "users", roles: ["superAdmin"] }
};

function resolveRoute(pathname) {
  return protectedRoutes[pathname] || protectedRoutes["/"];
}

function pathForView(view) {
  return Object.entries(protectedRoutes)
    .find(([, route]) => route.view === view)?.[0] || "/";
}

export function AppRouter() {
  const { token, user, login, logout, refreshCurrentUser } = useAppAuth();
  const [pathname, setPathname] = useState(() => window.location.pathname);
  const [toast, setToast] = useState("");
  const [refreshing, setRefreshing] = useState(false);
  const [sessionVersion, setSessionVersion] = useState(0);
  const route = resolveRoute(pathname);
  const hasRouteAccess = !route.roles || route.roles.includes(user?.role);
  const activeRoute = hasRouteAccess ? route : protectedRoutes["/"];

  useEffect(() => {
    const onPopState = () => setPathname(window.location.pathname);
    window.addEventListener("popstate", onPopState);
    return () => window.removeEventListener("popstate", onPopState);
  }, []);

  useEffect(() => {
    if (user && !hasRouteAccess) {
      window.history.replaceState({}, "", "/");
      setPathname("/");
    }
  }, [hasRouteAccess, user]);

  useEffect(() => {
    if (!toast) return undefined;
    const timeoutId = window.setTimeout(() => setToast(""), 4000);
    return () => window.clearTimeout(timeoutId);
  }, [toast]);

  function navigate(view) {
    const nextPath = pathForView(view);
    window.history.pushState({}, "", nextPath);
    setPathname(nextPath);
  }

  function handleLogout() {
    logout();
    navigate("tickets");
  }

  async function handleRefreshSession() {
    setRefreshing(true);
    try {
      await refreshCurrentUser();
      setSessionVersion((value) => value + 1);
      setToast("Sesión actualizada correctamente.");
    } catch (error) {
      setToast(error.message);
    } finally {
      setRefreshing(false);
    }
  }

  if (!token || !user) {
    return <LoginPage onLogin={login} />;
  }

  const pageByView = {
    tickets: <TicketsPage key={sessionVersion} token={token} user={user} notify={setToast} />,
    categories: <CategoriesPage token={token} user={user} notify={setToast} />,
    sla: <SlaPage token={token} />,
    reports: <ReportsPage token={token} />,
    users: <UsersPage token={token} notify={setToast} />
  };

  return (
    <MainLayout
      user={user}
      view={activeRoute.view}
      onView={navigate}
      onLogout={handleLogout}
      onRefresh={handleRefreshSession}
      refreshing={refreshing}
      toast={toast}
    >
      {pageByView[activeRoute.view]}
    </MainLayout>
  );
}
