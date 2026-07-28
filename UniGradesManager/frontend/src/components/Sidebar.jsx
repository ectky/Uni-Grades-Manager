import { NavLink } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const NAV_BY_ROLE = {
  Student: [{ to: "/", label: "Табло" }],
  Instructor: [
    { to: "/", label: "Табло" },
    { to: "/courses", label: "Курсове" },
  ],
  Admin: [
    { to: "/", label: "Табло" },
    { to: "/admin/users", label: "Потребители" },
    { to: "/admin/courses", label: "Курсове" },
  ],
};

export function Sidebar() {
  const { user, logout } = useAuth();
  const items = NAV_BY_ROLE[user?.role] ?? [];

  return (
    <div className="sidebar">
      <div className="logo">EduGrade</div>
      {items.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          end={item.to === "/"}
          className={({ isActive }) =>
            "nav-item" + (isActive ? " active" : "")
          }
        >
          {item.label}
        </NavLink>
      ))}
      <div className="nav-spacer" />
      <div className="nav-item logout" onClick={logout} role="button">
        Изход
      </div>
    </div>
  );
}
