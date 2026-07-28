import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getCourseDistribution, getCourseStats } from "../../api/analyticsApi";
import { AppLayout } from "../../components/AppLayout";
import { GradeDistributionChart } from "../../components/GradeDistributionChart";
import { ErrorBanner, LoadingSpinner, extractErrorMessage } from "../../components/Common";

export function PublicCourseStatsPage() {
  const { courseId } = useParams();
  const navigate = useNavigate();

  const [stats, setStats] = useState(null);
  const [distribution, setDistribution] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const [statsData, distributionData] = await Promise.all([
          getCourseStats(courseId),
          getCourseDistribution(courseId),
        ]);
        if (!cancelled) {
          setStats(statsData);
          setDistribution(distributionData);
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
      title={stats?.courseName ?? "Статистика по курс"}
      subtitle="Публична статистика по курс"
      actions={
        <button className="link-btn" onClick={() => navigate(-1)}>
          ← Назад
        </button>
      }
    >
      <ErrorBanner message={error} />

      {!stats || !distribution ? (
        <LoadingSpinner />
      ) : (
        <>
          <div className="cards">
            <div className="card">
              <div className="label">Среден успех</div>
              <div className="value blue">{stats.average.toFixed(2)}</div>
            </div>
            <div className="card">
              <div className="label">Брой издържали</div>
              <div className="value green">{Math.round(stats.passRatePercent)}%</div>
            </div>
            <div className="card">
              <div className="label">Брой студенти</div>
              <div className="value">{stats.studentCount}</div>
            </div>
          </div>

          <div className="content-row">
            <div className="panel">
              <h3>Разпределение на оценките</h3>
              <GradeDistributionChart distribution={distribution} />
            </div>
            <div className="panel">
              <h3>Статистически показатели</h3>
              <div className="stats-list">
                <div>
                  <span className="k">Медиана</span>
                  <span className="v">{stats.median.toFixed(2)}</span>
                </div>
                <div>
                  <span className="k">Мода</span>
                  <span className="v">{stats.mode.toFixed(2)}</span>
                </div>
                <div>
                  <span className="k">Стандартно отклонение</span>
                  <span className="v">{stats.stdDev.toFixed(2)}</span>
                </div>
                <div>
                  <span className="k">Минимум</span>
                  <span className="v">{stats.min.toFixed(2)}</span>
                </div>
                <div>
                  <span className="k">Максимум</span>
                  <span className="v">{stats.max.toFixed(2)}</span>
                </div>
                <div>
                  <span className="k">Процент успеваемост</span>
                  <span className="v">{Math.round(stats.passRatePercent)}%</span>
                </div>
              </div>
            </div>
          </div>
        </>
      )}
    </AppLayout>
  );
}
