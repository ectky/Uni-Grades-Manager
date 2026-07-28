import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getMyStudentGrades, getMyStudentStats } from "../../api/analyticsApi";
import { AppLayout } from "../../components/AppLayout";
import { ErrorBanner, LoadingSpinner, extractErrorMessage } from "../../components/Common";
import { useAuth } from "../../auth/AuthContext";

export function StudentDashboardPage() {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [grades, setGrades] = useState(null);
  const [stats, setStats] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        // No id passed — the backend resolves "me" from the JWT itself.
        // See analyticsApi.js: User.Id and the roll number Grades Service
        // stores against (User.StudentId) are different numbers here.
        const [gradesData, statsData] = await Promise.all([
          getMyStudentGrades(),
          getMyStudentStats(),
        ]);
        if (!cancelled) {
          setGrades(gradesData);
          setStats(statsData);
        }
      } catch (err) {
        if (!cancelled) setError(extractErrorMessage(err));
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <AppLayout title="Моите курсове" subtitle={`${user.name} — изглед на студента`}>
      <ErrorBanner message={error} />

      {!grades || !stats ? (
        <LoadingSpinner />
      ) : (
        <>
          <div className="cards" style={{ maxWidth: 520 }}>
            <div className="card">
              <div className="label">Курсове с оценка</div>
              <div className="value green">{stats.gradedCourseCount}</div>
            </div>
            <div className="card">
              <div className="label">Среден успех</div>
              <div className="value blue">{stats.average?.toFixed(2)}</div>
            </div>
          </div>

          <table className="data-table">
            <thead>
              <tr>
                <td>Курс</td>
                <td>Период</td>
                <td>Оценка</td>
                <td></td>
              </tr>
            </thead>
            <tbody>
              {grades.map((g) => (
                <tr key={g.courseId}>
                  <td>{g.courseName}</td>
                  <td>{g.period}</td>
                  <td>{g.value.toFixed(2)}</td>
                  <td>
                    <button
                      className="link-btn"
                      onClick={() => navigate(`/courses/${g.courseId}/stats`)}
                    >
                      Статистики →
                    </button>
                  </td>
                </tr>
              ))}
              {grades.length === 0 && (
                <tr>
                  <td colSpan={4} style={{ color: "var(--text-muted)" }}>
                    Все още няма въведени оценки.
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