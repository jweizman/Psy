import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { api } from "../api";
import type { Psychologist } from "../types";

const SKILL_ICONS: Record<string, string> = {
  stress: "psychology",
  burnout: "work_history",
  "burn-out": "work_history",
  sleep: "bedtime",
  anxiety: "favorite",
  panic: "bolt",
  mindfulness: "self_improvement",
  cbt: "neurology",
  adhd: "target",
  attention: "visibility",
  "executive-function": "checklist",
  procrastination: "schedule",
  "self-esteem": "auto_awesome",
  adolescent: "school",
  trauma: "healing",
  ptsd: "healing",
  emdr: "remove_red_eye",
  grief: "sentiment_very_dissatisfied",
  violence: "shield",
  attachment: "link",
  "emotional-regulation": "mood",
};

function iconFor(skill: string) {
  return SKILL_ICONS[skill.toLowerCase()] ?? "psychology";
}

function prettify(skill: string) {
  return skill
    .split(/[-_ ]/)
    .filter(Boolean)
    .map((w) => w[0].toUpperCase() + w.slice(1))
    .join(" ");
}

export default function Practitioner() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [p, setP] = useState<Psychologist | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const pid = Number(id);
    if (!Number.isFinite(pid)) {
      setError("Invalid practitioner id.");
      setLoading(false);
      return;
    }
    api
      .getPsychologist(pid)
      .then(setP)
      .catch((e) => setError(String(e)))
      .finally(() => setLoading(false));
  }, [id]);

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
            {p.nextAvailable && (
              <span className="text-primary text-sm font-medium">
                Next: {p.nextAvailable}
              </span>
            )}
          </div>
          <div className="bg-surface-container-lowest rounded-2xl p-6 shadow-sm">
            <DayGrid />
            <div className="mt-6 grid grid-cols-3 gap-2">
              {["09:00", "10:30", "14:00", "15:30", "17:00", "18:30"].map(
                (slot) => (
                  <button
                    key={slot}
                    className={
                      "py-2 px-3 text-sm font-medium rounded-lg transition-colors " +
                      (slot === p.nextAvailable
                        ? "bg-primary-container text-on-primary-container"
                        : "border border-outline-variant/20 hover:bg-surface-container-high")
                    }
                  >
                    {slot}
                  </button>
                ),
              )}
            </div>
          </div>
        </section>

        <section className="mb-12">
          <h3 className="text-on-surface font-headline font-bold text-xl mb-6">
            Patient reviews{" "}
            <span className="text-on-surface-variant font-body font-normal text-sm">
              ({p.reviewCount})
            </span>
          </h3>
          <div className="space-y-4">
            <Review
              initials="MD"
              author="Marc D."
              text="Exceptional listening skills. I was finally able to put words to my professional burnout."
              tone="tertiary"
            />
            <Review
              initials="SL"
              author="Sophie L."
              text="Very gentle and professional. The teleconsultation format is really convenient."
              tone="secondary"
            />
          </div>
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

function DayGrid() {
  const today = new Date();
  const days = Array.from({ length: 4 }, (_, i) => {
    const d = new Date(today);
    d.setDate(today.getDate() + i);
    return d;
  });
  const names = ["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"];
  return (
    <div className="grid grid-cols-4 gap-3">
      {days.map((d, i) => {
        const active = i === 1;
        return (
          <div
            key={i}
            className={
              "flex flex-col items-center p-3 rounded-xl " +
              (active ? "bg-primary text-on-primary" : "bg-surface-container-low")
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
            <span className="text-lg font-headline font-bold">{d.getDate()}</span>
          </div>
        );
      })}
    </div>
  );
}

function Review({
  initials,
  author,
  text,
  tone,
}: {
  initials: string;
  author: string;
  text: string;
  tone: "tertiary" | "secondary";
}) {
  const chip =
    tone === "tertiary"
      ? "bg-tertiary-container text-on-tertiary-container"
      : "bg-secondary-container text-on-secondary-container";
  return (
    <div className="p-6 bg-white rounded-2xl shadow-[0_4px_20px_rgba(49,51,47,0.02)]">
      <div className="flex gap-1 mb-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <span
            key={i}
            className="material-symbols-outlined text-secondary text-sm"
            style={{ fontVariationSettings: "'FILL' 1" }}
          >
            star
          </span>
        ))}
      </div>
      <p className="text-on-surface-variant text-sm italic mb-4">"{text}"</p>
      <div className="flex items-center gap-3">
        <div
          className={
            "w-8 h-8 rounded-full flex items-center justify-center text-[10px] font-bold " +
            chip
          }
        >
          {initials}
        </div>
        <span className="text-xs font-bold text-on-surface uppercase tracking-wider">
          {author}
        </span>
      </div>
    </div>
  );
}
