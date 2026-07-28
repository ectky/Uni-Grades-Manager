import { createContext, useContext, useEffect, useMemo, useState } from "react";
import { login as loginRequest } from "../api/authApi";
import {
  clearStoredToken,
  getStoredToken,
  setStoredToken,
} from "../api/httpClient";
import { decodeToken, isTokenExpired } from "./jwt";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const token = getStoredToken();
    if (!token) return null;
    const claims = decodeToken(token);
    if (!claims || isTokenExpired(claims.exp)) {
      clearStoredToken();
      return null;
    }
    return claims;
  });

  // Log out automatically once the token's own expiry (Chapter 4.2) passes,
  // instead of waiting for the next API call to fail with 401.
  useEffect(() => {
    if (!user?.exp) return;
    const msUntilExpiry = user.exp * 1000 - Date.now();
    if (msUntilExpiry <= 0) {
      logout();
      return;
    }
    const timer = setTimeout(logout, msUntilExpiry);
    return () => clearTimeout(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [user?.exp]);

  async function login(email, password) {
    const result = await loginRequest(email, password);
    setStoredToken(result.token);
    const claims = decodeToken(result.token);
    setUser(claims);
    return claims;
  }

  function logout() {
    clearStoredToken();
    setUser(null);
  }

  const value = useMemo(
    () => ({ user, isAuthenticated: !!user, login, logout }),
    [user]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}
