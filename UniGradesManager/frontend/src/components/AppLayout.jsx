import { Sidebar } from "./Sidebar";

export function AppLayout({ title, subtitle, actions, children }) {
  return (
    <div className="app-shell">
      <Sidebar />
      <div className="main">
        <div className="topbar" style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
          <div>
            <h1>{title}</h1>
            {subtitle && <div className="sub">{subtitle}</div>}
          </div>
          {actions}
        </div>
        {children}
      </div>
    </div>
  );
}
