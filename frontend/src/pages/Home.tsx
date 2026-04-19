import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api } from "../api";
import type { Psychologist } from "../types";

const FILTERS = ["All", "Apathy", "Stress", "ADHD", "Anger", "Fear"];

export default function Home() {
  const [search, setSearch] = useState("");
  const [filter, setFilter] = useState("All");
  const [list, setList] = useState<Psychologist[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  function openChatWith(text: string) {
    const t = text.trim();
    if (!t) return;
    navigate("/chat", { state: { initialMessage: t } });
  }

  useEffect(() => {
    api
      .listPsychologists()
      .then(setList)
      .catch(() => setList([]))
      .finally(() => setLoading(false));
  }, []);

  const filtered = useMemo(() => {
    const s = search.toLowerCase().trim();
    const f = filter === "All" ? "" : filter.toLowerCase();
    return list.filter((p) => {
      const hay = (p.name + " " + p.title + " " + p.bio + " " + p.skills.join(" ")).toLowerCase();
      if (s && !hay.includes(s)) return false;
      if (f && !hay.includes(f)) return false;
      return true;
    });
  }, [list, search, filter]);

  return (
    <div className="bg-background text-on-surface min-h-screen">
      <header className="fixed top-0 w-full z-40 bg-[#fbf9f5]/80 backdrop-blur-xl">
        <div className="flex justify-between items-center px-6 h-16 w-full max-w-lg mx-auto">
          <div className="flex items-center gap-4">
            <span className="material-symbols-outlined text-primary cursor-pointer">menu</span>
            <h1 className="font-headline font-bold tracking-tight text-xl text-on-surface">
              The Serene Path
            </h1>
          </div>
          <div className="w-10 h-10 rounded-full overflow-hidden bg-surface-container-highest" />
        </div>
      </header>

      <main className="pt-20 pb-32 max-w-lg mx-auto px-6 space-y-8">
        <section className="mt-4">
          <Link
            to="/chat"
            className="w-full bg-error-container text-on-error-container p-5 rounded-xl flex items-center justify-between shadow-[0_10px_30px_rgba(49,51,47,0.06)] active:scale-95 transition-transform duration-200"
          >
            <div className="flex flex-col items-start text-left">
              <span className="font-headline font-bold text-lg">Need help now?</span>
              <span className="font-body text-sm opacity-90">
                Immediate consultation available
              </span>
            </div>
            <span
              className="material-symbols-outlined text-3xl"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              emergency_home
            </span>
          </Link>
        </section>

        <section className="space-y-4">
          <div className="relative">
            <div className="absolute inset-y-0 left-4 flex items-center pointer-events-none">
              <span className="material-symbols-outlined text-on-surface-variant/60">search</span>
            </div>
            <input
              className="w-full h-14 pl-12 pr-4 bg-surface-container-highest border-none rounded-xl focus:ring-2 focus:ring-primary/40 placeholder:text-on-surface-variant/50 text-on-surface transition-all outline-none"
              placeholder="How are you feeling?"
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") openChatWith(search);
              }}
            />
            {search.trim() && (
              <button
                onClick={() => openChatWith(search)}
                className="absolute inset-y-0 right-2 my-2 px-4 bg-primary text-on-primary rounded-lg text-sm font-semibold"
              >
                Talk to us
              </button>
            )}
          </div>
        </section>

        <section>
          <div className="flex overflow-x-auto gap-3 no-scrollbar py-2">
            {FILTERS.map((f) => (
              <button
                key={f}
                onClick={() => setFilter(f)}
                className={
                  "flex-none px-6 py-3 rounded-full font-label font-medium transition-colors " +
                  (filter === f
                    ? "bg-primary text-on-primary"
                    : "bg-surface-container-high text-on-surface hover:bg-surface-container-highest")
                }
              >
                {f}
              </button>
            ))}
          </div>
        </section>

        <section className="space-y-6">
          <div className="flex items-center justify-between">
            <h2 className="font-headline font-bold text-2xl text-on-surface">Available today</h2>
            <span className="text-primary font-label text-sm font-semibold cursor-pointer">
              View all
            </span>
          </div>

          <div className="grid grid-cols-1 gap-4">
            {loading && (
              <div className="text-on-surface-variant text-sm">Loading…</div>
            )}
            {!loading && filtered.length === 0 && (
              <div className="text-on-surface-variant text-sm">No practitioner found.</div>
            )}
            {filtered.map((p) => (
              <PractitionerCard key={p.id} p={p} />
            ))}
          </div>
        </section>

        <section className="bg-surface-container-low p-8 rounded-2xl relative overflow-hidden">
          <div className="absolute top-4 right-4 text-primary-fixed-dim/20 scale-150">
            <span className="material-symbols-outlined text-6xl">lightbulb</span>
          </div>
          <div className="relative z-10 space-y-3">
            <span className="text-tertiary font-label font-bold text-xs tracking-widest uppercase">
              Did you know?
            </span>
            <h4 className="font-headline font-bold text-xl text-on-surface leading-tight">
              Box breathing can calm your nervous system in 60 seconds.
            </h4>
            <p className="text-on-surface-variant text-sm leading-relaxed max-w-[85%]">
              Inhale for 4 seconds, hold for 4 seconds, exhale for 4 seconds, hold for 4 seconds.
              Repeat 3 times.
            </p>
            <button className="mt-2 text-primary font-bold text-sm flex items-center gap-2 group">
              Try now
              <span className="material-symbols-outlined text-sm group-hover:translate-x-1 transition-transform">
                arrow_forward
              </span>
            </button>
          </div>
        </section>
      </main>
    </div>
  );
}

function PractitionerCard({ p }: { p: Psychologist }) {
  return (
    <div className="bg-surface-container-lowest p-5 rounded-xl flex gap-5 items-center relative overflow-hidden group">
      <div className="absolute top-0 right-0 w-32 h-32 bg-primary/5 rounded-full -mr-16 -mt-16 group-hover:scale-110 transition-transform duration-500" />
      <div className="w-20 h-20 rounded-2xl overflow-hidden bg-surface-container-low flex-shrink-0">
        {p.photoUrl && (
          <img src={p.photoUrl} alt={p.name} className="w-full h-full object-cover" />
        )}
      </div>
      <div className="flex-1 space-y-1">
        <div className="flex items-center gap-2 flex-wrap">
          <h3 className="font-headline font-bold text-lg text-on-surface">{p.name}</h3>
          {p.verified && (
            <div className="bg-secondary-container text-on-secondary-container px-2 py-0.5 rounded-full text-[10px] font-bold flex items-center gap-1">
              <span
                className="material-symbols-outlined text-[12px]"
                style={{ fontVariationSettings: "'FILL' 1" }}
              >
                verified
              </span>
              VERIFIED
            </div>
          )}
        </div>
        <p className="text-on-surface-variant text-sm font-body">{p.title}</p>
        <div className="flex items-center gap-4 mt-2">
          <span className="flex items-center gap-1 text-xs text-on-surface-variant">
            <span className="material-symbols-outlined text-sm text-tertiary">star</span>
            {p.rating.toFixed(1)} ({p.reviewCount})
          </span>
          {p.nextAvailable && (
            <span className="flex items-center gap-1 text-xs text-on-surface-variant">
              <span className="material-symbols-outlined text-sm text-primary">schedule</span>
              Next: {p.nextAvailable}
            </span>
          )}
        </div>
      </div>
    </div>
  );
}
