import { Bar } from "react-chartjs-2";
import {
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  LinearScale,
  Tooltip,
} from "chart.js";

ChartJS.register(CategoryScale, LinearScale, BarElement, Tooltip);

// Same color logic as the earlier HTML mockups: weak grades red, mid blue, good green.
function colorForGrade(grade) {
  if (grade <= 3) return "#E15759";
  if (grade === 4) return "#3B82F6";
  return "#2CA858";
}

/** distribution: [{ grade: 2, count: 5 }, { grade: 3, count: 9 }, ...] */
export function GradeDistributionChart({ distribution }) {
  const data = {
    labels: distribution.map((d) => String(d.grade)),
    datasets: [
      {
        data: distribution.map((d) => d.count),
        backgroundColor: distribution.map((d) => colorForGrade(d.grade)),
        borderRadius: 4,
        maxBarThickness: 48,
      },
    ],
  };

  const options = {
    responsive: true,
    plugins: {
      legend: { display: false },
      tooltip: { enabled: true },
    },
    scales: {
      y: { beginAtZero: true, ticks: { precision: 0 } },
    },
  };

  return <Bar data={data} options={options} />;
}
