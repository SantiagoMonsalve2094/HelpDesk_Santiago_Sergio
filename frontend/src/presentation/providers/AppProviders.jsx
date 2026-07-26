import React, { createContext, useContext } from "react";
import { useAuth as useAuthState } from "../../application/hooks/useAuth";

const AuthContext = createContext(null);

export function AppProviders({ children }) {
  const auth = useAuthState();

  return <AuthContext.Provider value={auth}>{children}</AuthContext.Provider>;
}

export function useAppAuth() {
  const auth = useContext(AuthContext);
  if (!auth) {
    throw new Error("useAppAuth debe utilizarse dentro de AppProviders.");
  }

  return auth;
}
