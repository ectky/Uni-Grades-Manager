import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";

/**
 * Client-side gate only — purely for UX (don't show an Instructor a Student
 * screen). The real enforcement happens server-side on every request; this
 * never substitutes for that (Chapter 4.3).
 */
export function ProtectedRoute({ roles, children }) {
  const { isAuthenticated, user } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (roles && !roles.includes(user.role)) {
    return <Navigate to="/" replace />;
  }

  return children;
}
