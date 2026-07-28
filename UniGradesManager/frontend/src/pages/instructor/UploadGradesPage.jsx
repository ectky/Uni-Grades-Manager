import { useCallback, useState } from "react";
import { useDropzone } from "react-dropzone";
import { useNavigate, useParams } from "react-router-dom";
import { downloadGradeTemplate, uploadGrades } from "../../api/parseApi";
import { AppLayout } from "../../components/AppLayout";
import { ErrorBanner, extractErrorMessage } from "../../components/Common";

export function UploadGradesPage() {
  const { courseId } = useParams();
  const navigate = useNavigate();

  const [file, setFile] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState("");
  const [result, setResult] = useState(null); // ParseResultDto once upload succeeds

  const onDrop = useCallback((acceptedFiles) => {
    setError("");
    setResult(null);
    setFile(acceptedFiles[0] ?? null);
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": [".xlsx"],
    },
    maxFiles: 1,
    maxSize: 10 * 1024 * 1024, // 10 MB — Chapter 4.4
  });

  async function handleUpload() {
    if (!file) return;
    setUploading(true);
    setError("");
    try {
      const data = await uploadGrades(courseId, file);
      setResult(data);
    } catch (err) {
      // Parser Service returns a combined, human-readable list of bad rows
      // (InvalidExcelStructureException) — shown here as-is.
      setError(extractErrorMessage(err, "Файлът не можа да бъде обработен."));
    } finally {
      setUploading(false);
    }
  }

  if (result) {
    return (
      <AppLayout title={`Качване на оценки — ${result.courseName}`} subtitle={result.period}>
        <div className="success-banner">
          <span>✓ Файлът е обработен успешно</span>
          <span>Време за обработка: {result.processingTimeMs} ms</span>
        </div>

        <div className="cards">
          <div className="card">
            <div className="label">Курс</div>
            <div className="value">{result.courseName}</div>
          </div>
          <div className="card">
            <div className="label">Период</div>
            <div className="value">{result.period}</div>
          </div>
          <div className="card">
            <div className="label">Разпознати записи</div>
            <div className="value">{result.recordCount}</div>
          </div>
        </div>

        <table className="data-table" style={{ marginBottom: 20 }}>
          <thead>
            <tr>
              <td>Студент ID</td>
              <td>Оценка</td>
            </tr>
          </thead>
          <tbody>
            {result.records.map((r) => (
              <tr key={r.studentId}>
                <td>{r.studentId}</td>
                <td>{r.value.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>

        <button className="btn btn-success" onClick={() => navigate("/")}>
          Готово — обратно към таблото
        </button>
      </AppLayout>
    );
  }

  return (
    <AppLayout title={`Качване на оценки — курс ${courseId}`}>
      <ErrorBanner message={error} />

      <div {...getRootProps()} className={"dropzone" + (isDragActive ? " active" : "")}>
        <input {...getInputProps()} />
        <div style={{ fontSize: 40, marginBottom: 12 }}>⬆</div>
        <div style={{ fontWeight: 700, marginBottom: 6 }}>
          {file ? file.name : "Плъзнете файл тук или кликнете за избор"}
        </div>
        <div style={{ fontSize: 13 }}>
          Поддържани формати: .xlsx — максимален размер 10 MB
        </div>
      </div>

      <div className="info-box">
        Файлът трябва да съдържа колони <strong>StudentId</strong> и{" "}
        <strong>Value</strong>. Първият ред трябва да съдържа заглавия на колоните.
        Оценките трябва да са между 2.00 и 6.00.
      </div>

      <div className="template-box" style={{ marginBottom: 20 }}>
        <span>Нямате готов файл?</span>
        <button className="link-btn" onClick={downloadGradeTemplate}>
          Изтеглете шаблон →
        </button>
      </div>

      <button
        className="btn btn-primary"
        disabled={!file || uploading}
        onClick={handleUpload}
      >
        {uploading ? "Обработва се…" : "Качи файла"}
      </button>
    </AppLayout>
  );
}
