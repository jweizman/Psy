import { BrowserRouter, Routes, Route, Link, useLocation } from "react-router-dom";
import Home from "./pages/Home";
import Chat from "./pages/Chat";
import Admin from "./pages/Admin";

function BottomNav() {
  const { pathname } = useLocation();
  const Item = ({
    to,
    icon,
    label,
    fill,
  }: {
    to: string;
    icon: string;
    label: string;
    fill?: boolean;
  }) => {
    const active = pathname === to;
    return (
      <Link
        to={to}
        className={
          "flex flex-col items-center justify-center px-5 py-2 rounded-2xl transition-transform active:scale-90 " +
          (active ? "bg-primary/10 text-primary" : "text-on-surface/50 hover:text-primary")
        }
      >
        <span
          className="material-symbols-outlined"
          style={fill || active ? { fontVariationSettings: "'FILL' 1" } : undefined}
        >
          {icon}
        </span>
        <span className="font-body text-[10px] font-medium uppercase tracking-wider mt-1">
          {label}
        </span>
      </Link>
    );
  };

  return (
    <nav className="fixed bottom-0 w-full z-50 pb-safe bg-[#fbf9f5]/80 backdrop-blur-xl border-t border-outline-variant/15 shadow-[0_-10px_30px_rgba(49,51,47,0.04)]">
      <div className="flex justify-around items-center h-20 w-full max-w-lg mx-auto px-4">
        <Item to="/" icon="home_health" label="Home" />
        <Item to="/chat" icon="chat_bubble" label="Chat" />
        <Item to="/admin" icon="manage_accounts" label="Admin" />
      </div>
    </nav>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/chat" element={<Chat />} />
        <Route path="/admin" element={<Admin />} />
      </Routes>
      <BottomNav />
    </BrowserRouter>
  );
}
