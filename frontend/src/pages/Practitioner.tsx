import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { api } from "../api";
import type { Psychologist, Review, SkillIcon, Slot } from "../types";

function prettify(skill: string) {
  return skill
    .split(/[-_ ]/)
    .filter(Boolean)
    .map((w) => w[0].toUpperCase() + w.slice(1))
    .join(" ");
}

function formatHour(iso: string) {
  const d = new Date(iso);
  return d.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
}

function formatNext(iso: string) {
  const d = new Date(iso);
  const today = new Date();
  const sameDay =
    d.getFullYear() === today.getFullYear() &&
    d.getMonth() === today.getMonth() &&
    d.getDate() === today.getDate();
  const time = formatHour(iso);
  if (sameDay) return `Today, ${time}`;
  const tomorrow = new Date(today);
  tomorrow.setDate(today.getDate() + 1);
  if (
    d.getFullYear() === tomorrow.getFullYear() &&
    d.getMonth() === tomorrow.getMonth() &&
    d.getDate() === tomorrow.getDate()
  )
    return `Tomorrow, ${time}`;
  return `${d.toLocaleDateString([], { weekday: "short", day: "numeric" })}, ${time}`;
}

const REVIEW_TONES = ["tertiary", "secondary"] as const;
type Tone = (typeof REVIEW_TONES)[number];

export default function Practitioner() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [p, setP] = useState<Psychologist | null>(null);
  const [reviews, setReviews] = useState<Review[]>([]);
  const [slots, setSlots] = useState<Slot[]>([]);
  const [icons, setIcons] = useState<SkillIcon[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedDay, setSelectedDay] = useState<string | null>(null);

  useEffect(() => {
    const pid = Number(id);
    if (!Number.isFinite(pid)) {
      setError("Invalid practitioner id.");
      setLoading(false);
      return;
    }
    (async () => {
      try {
        const [prac, revs, sls, ics] = await Promise.all([
          api.getPsychologist(pid),
          api.listReviews(pid),
          api.listSlots(pid),
          api.listSkillIcons(),
        ]);
        setP(prac);
        setReviews(revs);
        setSlots(sls);
        setIcons(ics);
      } catch (e) {
        setError(String(e));
      } finally {
        setLoading(false);
      }
    })();
  }, [id]);

  const iconMap = useMemo(() => {
    const m = new Map<string, string>();
    for (const i of icons) m.set(i.name.toLowerCase(), i.icon);
    return m;
  }, [icons]);

  const iconFor = (skill: string) => iconMap.get(skill.toLowerCase()) ?? "psychology";

  const dayGroups = useMemo(() => {
    const byKey = new Map<string, Slot[]>();
    for (const s of slots) {
      const d = new Date(s.startUtc);
      const key = d.toISOString().slice(0, 10);
      const arr = byKey.get(key) ?? [];
      arr.push(s);
      byKey.set(key, arr);
    }
    return Array.from(byKey.entries())
      .sort(([a], [b]) => (a < b ? -1 : 1))
      .slice(0, 4);
  }, [slots]);

  useEffect(() => {
    if (selectedDay === null && dayGroups.length > 0) {
      setSelectedDay(dayGroups[0][0]);
    }
  }, [dayGroups, selectedDay]);

  const daySlots = useMemo(() => {
    if (!selectedDay) return [];
    const found = dayGroups.find(([k]) => k === selectedDay);
    return found ? found[1] : [];
  }, [dayGroups, selectedDay]);

  const nextSlot = useMemo(
    () => slots.find((s) => !s.booked) ?? null,
    [slots],
  );

  if (loading) {
    return (
      <div className="pt-24 max-w-lg mx-auto px-6 text-on-surface-variant">
        Loading…
      </div>
    );
  }
  if (error || !p) {
    return (
      <div className="pt-24 max-w-lg mx-auto px-6 space-y-4">
        <p className="text-error text-sm">{error ?? "Not found."}</p>
        <Link to="/" className="text-primary font-semibold text-sm">
          ← Back to home
        </Link>
      </div>
    );
  }

  const satisfaction = Math.round((p.rating / 5) * 100);

  return (
    <div className="bg-background text-on-surface min-h-screen pb-32">
      <header className="bg-[#fbf9f5]/80 backdrop-blur-xl fixed top-0 w-full z-40">
        <div className="flex justify-between items-center px-6 h-16 w-full max-w-lg mx-auto">
          <button
            onClick={() => navigate(-1)}
            className="text-primary hover:opacity-80 transition-opacity active:scale-95 duration-200"
          >
            <span className="material-symbols-outlined">arrow_back</span>
          </button>
          <h1 className="font-headline font-bold tracking-tight text-xl text-on-surface">
            Psychologist Profile
          </h1>
          <div className="w-8 h-8 rounded-full overflow-hidden bg-surface-container-highest" />
        </div>
      </header>

      <main className="pt-20 max-w-lg mx-auto px-6">
        <section className="mb-10">
          <div className="relative flex items-end gap-6">
            <div className="w-2/3 aspect-[4/5] rounded-xl overflow-hidden shadow-sm bg-surface-container-low">
              {p.photoUrl && (
                <img
                  src={p.photoUrl}
                  alt={p.name}
                  className="w-full h-full object-cover"
                />
              )}
            </div>
            <div className="w-1/3 flex flex-col gap-4 mb-4">
              {p.verified && (
                <div className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-secondary-container text-on-secondary-container text-[11px] font-semibold tracking-wide uppercase shadow-[0_4px_12px_rgba(201,236,190,0.3)]">
                  <span
                    className="material-symbols-outlined text-[14px]"
                    style={{ fontVariationSettings: "'FILL' 1" }}
                  >
                    verified
                  </span>
                  VERIFIED
                </div>
              )}
              <div className="p-4 bg-surface-container-lowest rounded-xl shadow-sm">
                <p className="text-primary font-headline font-bold text-lg">
                  {satisfaction}%
                </p>
                <p className="text-[10px] text-on-surface-variant uppercase tracking-wider">
                  SATISFACTION
                </p>
              </div>
            </div>
          </div>
          <div className="mt-8">
            <p className="text-primary font-label font-semibold text-sm tracking-wide uppercase mb-2">
              {p.title}
            </p>
            <h2 className="text-on-surface font-headline font-extrabold text-4xl tracking-tight leading-tight">
              {p.name}
            </h2>
            <p className="text-on-surface-variant mt-3 text-lg leading-relaxed">
              {p.bio.split(". ")[0] + "."}
            </p>
          </div>
        </section>

        {p.skills.length > 0 && (
          <section className="mb-12">
            <h3 className="text-on-surface font-headline font-bold text-xl mb-6">
              Expertises
            </h3>
            <div className="flex flex-wrap gap-3">
              {p.skills.map((s) => (
                <div
                  key={s}
                  className="flex items-center gap-2 px-5 py-3 rounded-xl bg-surface-container-high text-on-surface"
                >
                  <span className="material-symbols-outlined text-primary">
                    {iconFor(s)}
                  </span>
                  <span className="text-sm font-medium">{prettify(s)}</span>
                </div>
              ))}
            </div>
          </section>
        )}

        <section className="mb-12 p-8 bg-surface-container-low rounded-2xl">
          <h3 className="text-on-surface font-headline font-bold text-xl mb-4">
            About
          </h3>
          <div className="space-y-4 text-on-surface-variant leading-relaxed whitespace-pre-line">
            {p.bio}
          </div>
        </section>

        <section className="mb-12">
          <div className="flex justify-between items-center mb-6">
            <h3 className="text-on-surface font-headline font-bold text-xl">
              Availability
            </h3>
            {nextSlot && (
              <span className="text-primary text-sm font-medium">
                Next: {formatNext(nextSlot.startUtc)}
              </span>
            )}
            {!nextSlot && p.nextAvailable && (
              <span className="text-primary text-sm font-medium">
                Next: {p.nextAvailable}
              </span>
            )}
          </div>
          <div className="bg-surface-container-lowest rounded-2xl p-6 shadow-sm">
            {dayGroups.length === 0 ? (
              <p className="text-sm text-on-surface-variant">
                No upcoming availability.
              </p>
            ) : (
              <>
                <div className="grid grid-cols-4 gap-3">
                  {dayGroups.map(([key, list]) => {
                    const d = new Date(list[0].startUtc);
                    const active = key === selectedDay;
                    const names = ["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"];
                    return (
                      <button
                        key={key}
                        onClick={() => setSelectedDay(key)}
                        className={
                          "flex flex-col items-center p-3 rounded-xl " +
                          (active
                            ? "bg-primary text-on-primary"
                            : "bg-surface-container-low")
                        }
                      >
                        <span
                          className={
                            "text-[10px] uppercase font-bold mb-1 " +
                            (active ? "opacity-80" : "text-on-surface-variant")
                          }
                        >
                          {names[d.getDay()]}
                        </span>
                        <span className="text-lg font-headline font-bold">
                          {d.getDate()}
                        </span>
                      </button>
                    );
                  })}
                </div>
                <div className="mt-6 grid grid-cols-3 gap-2">
                  {daySlots.map((s) => (
                    <button
                      key={s.id}
                      disabled={s.booked}
                      className={
                        "py-2 px-3 text-sm font-medium rounded-lg transition-colors " +
                        (s.booked
                          ? "bg-surface-container-high text-on-surface-variant/50 line-through cursor-not-allowed"
                          : nextSlot && s.id === nextSlot.id
                            ? "bg-primary-container text-on-primary-container"
                            : "border border-outline-variant/20 hover:bg-surface-container-high")
                      }
                    >
                      {formatHour(s.startUtc)}
                    </button>
                  ))}
                </div>
              </>
            )}
          </div>
        </section>

        <section className="mb-12">
          <h3 className="text-on-surface font-headline font-bold text-xl mb-6">
            Patient reviews{" "}
            <span className="text-on-surface-variant font-body font-normal text-sm">
              ({reviews.length || p.reviewCount})
            </span>
          </h3>
          {reviews.length === 0 ? (
            <p className="text-sm text-on-surface-variant">No reviews yet.</p>
          ) : (
            <div className="space-y-4">
              {reviews.map((r, i) => (
                <ReviewCard key={r.id} r={r} tone={REVIEW_TONES[i % REVIEW_TONES.length]} />
              ))}
            </div>
          )}
        </section>
      </main>

      <div className="fixed bottom-20 left-0 right-0 p-6 z-40 bg-gradient-to-t from-background via-background/90 to-transparent">
        <div className="max-w-lg mx-auto">
          <button className="w-full bg-primary text-on-primary py-5 rounded-2xl font-headline font-bold text-base shadow-[0_20px_40px_rgba(55,103,103,0.25)] flex items-center justify-center gap-3 active:scale-[0.98] transition-transform">
            <span className="material-symbols-outlined">videocam</span>
            Book a teleconsultation
          </button>
        </div>
      </div>
    </div>
  );
}

function ReviewCard({ r, tone }: { r: Review; tone: Tone }) {
  const chip =
    tone === "tertiary"
      ? "bg-tertiary-container text-on-tertiary-container"
      : "bg-secondary-container text-on-secondary-container";
  return (
    <div className="p-6 bg-white rounded-2xl shadow-[0_4px_20px_rgba(49,51,47,0.02)]">
      <div className="flex gap-1 mb-3">
        {Array.from({ length: r.rating }).map((_, i) => (
          <span
            key={i}
            className="material-symbols-outlined text-secondary text-sm"
            style={{ fontVariationSettings: "'FILL' 1" }}
          >
            star
          </span>
        ))}
      </div>
      <p className="text-on-surface-variant text-sm italic mb-4">"{r.text}"</p>
      <div className="flex items-center gap-3">
        <div
          className={
            "w-8 h-8 rounded-full flex items-center justify-center text-[10px] font-bold " +
            chip
          }
        >
          {r.initials}
        </div>
        <span className="text-xs font-bold text-on-surface uppercase tracking-wider">
          {r.author}
        </span>
      </div>
    </div>
  );
}
