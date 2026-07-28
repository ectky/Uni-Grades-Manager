import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getCourseDistribution, getCourseGrades, getCourseStats } from "../../api/analyticsApi";
import { getMyCourses } from "../../api/coursesApi";
import { AppLayout } from "../../components/AppLayout";
import { GradeDistributionChart } from "../../components/GradeDistributionChart";
import { ErrorBanner, LoadingSpinner, extractErrorMessage } from "../../components/Common";

export function DetailedCourseStatsPage() {
  const { courseId } = useParams();
  const navigate = useNavigate();

  const [myCourses, setMyCourses] = useState(null);
  const [stats, setStats] = useState(null);
  const [distribution, setDistribution] = useState(null);
  const [studentGrades, setStudentGrades] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    getMyCourses().then(setMyCourses).catch(() => {});
  }, []);

  useEffect(() => {
    let cancelled = false;
    setStats(null);
    setDistribution(null);
    setStudentGrades(null);

    async function load() {
      try {
        const [statsData, distributionData, gradesData] = await Promise.all([
          getCourseStats(courseId),
          getCourseDistribution(courseId),
          getCourseGrades(courseId),
        ]);
        if (!cancelled) {
          setStats(statsData);
          setDistribution(distributionData);
          setStudentGrades(gradesData);
        }
      } catch (err) {
        if (!cancelled) setError(extractErrorMessage(err));
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, [courseId]);

  return (
    <AppLayout
      title="Детайлни статистики по курс"
      subtitle="Изглед на преподавателя"
      actions={
        myCourses && (
          <select
            value={courseId}
            onChange={(e) => navigate(`/courses/${e.target.value}/detailed-stats`)}
          >
            {myCourses.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name} — {c.period}
              </option>
            ))}
          </select>
        )
      }
    >
      <ErrorBanner message={error} />

      {!stats || !distribution || !studentGrades ? (
        <LoadingSpinner />
      ) : (
        <>
          <div className="content-row" style={{ marginBottom: 20 }}>
            <div className="panel">
              <h3>Разпределение на оценките</h3>
              <GradeDistributionChart distribution={distribution} />
            </div>
            <div className="panel">
              <h3>Пълен статистически профил</h3>
              <table className="stats-list" style={{ width: "100%" }}>
                <tbody>
                  <tr><td className="k">Брой студенти</td><td className="v" style={{ textAlign: "right" }}>{stats.studentCount}</td></tr>
                  <tr><td className="k">Среден успех</td><td className="v" style={{ textAlign: "right" }}>{stats.average.toFixed(2)}</td></tr>
                  <tr><td className="k">Медиана</td><td className="v" style={{ textAlign: "right" }}>{stats.median.toFixed(2)}</td></tr>
                  <tr><td className="k">Мода</td><td className="v" style={{ textAlign: "right" }}>{stats.mode.toFixed(2)}</td></tr>
                  <tr><td className="k">Стандартно отклонение</td><td className="v" style={{ textAlign: "right" }}>{stats.stdDev.toFixed(2)}</td></tr>
                  <tr><td className="k">Минимум</td><td className="v" style={{ textAlign: "right" }}>{stats.min.toFixed(2)}</td></tr>
                  <tr><td className="k">Максимум</td><td className="v" style={{ textAlign: "right" }}>{stats.max.toFixed(2)}</td></tr>
                  <tr><td className="k">Процент успеваемост</td><td className="v" style={{ textAlign: "right" }}>{Math.round(stats.passRatePercent)}%</td></tr>
                </tbody>
              </table>
            </div>
          </div>

          <div className="panel">
            <h3>Студенти с оценки по курса</h3>
            <table className="data-table">
              <thead>
                <tr>
                  <td>Студент</td>
                  <td>Оценка</td>
                </tr>
              </thead>
              <tbody>
                {studentGrades.map((g) => (
                  <tr key={g.studentId}>
                    <td>{g.studentName}</td>
                    <td>{g.value.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </AppLayout>
  );
}
