import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getMyCourses } from "../../api/coursesApi";
import { AppLayout } from "../../components/AppLayout";
import { ErrorBanner, LoadingSpinner, extractErrorMessage } from "../../components/Common";
import { useAuth } from "../../auth/AuthContext";

export function InstructorDashboardPage() {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [courses, setCourses] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    getMyCourses()
      .then((data) => {
        if (!cancelled) setCourses(data);
      })
      .catch((err) => {
        if (!cancelled) setError(extractErrorMessage(err));
      });

    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <AppLayout title="Моите курсове" subtitle={user.name}>
      <ErrorBanner message={error} />

      {!courses ? (
        <LoadingSpinner />
      ) : (
        <>
          <div className="cards" style={{ maxWidth: 300 }}>
            <div className="card">
              <div className="label">Активни курсове</div>
              <div className="value blue">{courses.length}</div>
            </div>
          </div>

          <table className="data-table">
            <thead>
              <tr>
                <td>Курс</td>
                <td>Период</td>
                <td></td>
                <td></td>
              </tr>
            </thead>
            <tbody>
              {courses.map((c) => (
                <tr key={c.id}>
                  <td>{c.name}</td>
                  <td>{c.period}</td>
                  <td>
                    <button
                      className="btn btn-primary btn-sm"
                      onClick={() => navigate(`/courses/${c.id}/upload`)}
                    >
                      Качи оценки
                    </button>
                  </td>
                  <td>
                    <button
                      className="link-btn"
                      onClick={() => navigate(`/courses/${c.id}/detailed-stats`)}
                    >
                      Детайлни статистики →
                    </button>
                  </td>
                </tr>
              ))}
              {courses.length === 0 && (
                <tr>
                  <td colSpan={4} style={{ color: "var(--text-muted)" }}>
                    Все още нямате назначени курсове — свържете се с администратор.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </>
      )}
    </AppLayout>
  );
}
