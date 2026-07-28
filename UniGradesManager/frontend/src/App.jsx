import { Navigate, Route, BrowserRouter as Router, Routes } from "react-router-dom";
import { AuthProvider, useAuth } from "./auth/AuthContext";
import { ProtectedRoute } from "./auth/ProtectedRoute";
import { LoginPage } from "./pages/LoginPage";
import { StudentDashboardPage } from "./pages/student/StudentDashboardPage";
import { PublicCourseStatsPage } from "./pages/student/PublicCourseStatsPage";
import { InstructorDashboardPage } from "./pages/instructor/InstructorDashboardPage";
import { UploadGradesPage } from "./pages/instructor/UploadGradesPage";
import { DetailedCourseStatsPage } from "./pages/instructor/DetailedCourseStatsPage";
import { AdminUsersPage } from "./pages/admin/AdminUsersPage";
import { AdminCoursesPage } from "./pages/admin/AdminCoursesPage";

/** "/" renders a different dashboard per role — there's no single generic home. */
function RoleHome() {
  const { user } = useAuth();
  switch (user?.role) {
    case "Student":
      return <StudentDashboardPage />;
    case "Instructor":
      return <InstructorDashboardPage />;
    case "Admin":
      return <Navigate to="/admin/users" replace />;
    default:
      return <Navigate to="/login" replace />;
  }
}

export default function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          <Route path="/login" element={<LoginPage />} />

          <Route
            path="/"
            element={
              <ProtectedRoute>
                <RoleHome />
              </ProtectedRoute>
            }
          />

          {/* Shared — Student and Instructor both view course stats (Fig. 3.3) */}
          <Route
            path="/courses/:courseId/stats"
            element={
              <ProtectedRoute roles={["Student", "Instructor"]}>
                <PublicCourseStatsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/courses"
            element={
              <ProtectedRoute roles={["Instructor"]}>
                <InstructorDashboardPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/courses/:courseId/upload"
            element={
              <ProtectedRoute roles={["Instructor"]}>
                <UploadGradesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/courses/:courseId/detailed-stats"
            element={
              <ProtectedRoute roles={["Instructor"]}>
                <DetailedCourseStatsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/users"
            element={
              <ProtectedRoute roles={["Admin"]}>
                <AdminUsersPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/courses"
            element={
              <ProtectedRoute roles={["Admin"]}>
                <AdminCoursesPage />
              </ProtectedRoute>
            }
          />

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Router>
    </AuthProvider>
  );
}
