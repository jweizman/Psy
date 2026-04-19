import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../api";
import type { Psychologist } from "../types";

type Form = Omit<Psychologist, "id">;

const EMPTY: Form = {
  name: "",
  title: "",
  photoUrl: "",
  bio: "",
  skills: [],
  rating: 4.5,
  reviewCount: 0,
  nextAvailable: "",
  verified: false,
  availableToday: true,
};

export default function Admin() {
  const [list, setList] = useState<Psychologist[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<Form>(EMPTY);
  const [skillInput, setSkillInput] = useState("");
  const [saving, setSaving] = useState(false);

  async function refresh() {
    setLoading(true);
    try {
      setList(await api.listPsychologists());
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    refresh();
  }, []);

  function startCreate() {
    setEditingId(null);
    setForm(EMPTY);
    setSkillInput("");
  }

  function startEdit(p: Psychologist) {
    setEditingId(p.id);
    const { id: _id, ...rest } = p;
    void _id;
    setForm(rest);
    setSkillInput("");
  }

  async function save() {
    setSaving(true);
    try {
      if (editingId === null) await api.createPsychologist(form);
      else await api.updatePsychologist(editingId, form);
      await refresh();
      startCreate();
    } catch (e) {
      alert(String(e));
    } finally {
      setSaving(false);
    }
  }

  async function remove(id: number) {
    if (!confirm("Delete this practitioner?")) return;
    await api.deletePsychologist(id);
    await refresh();
    if (editingId === id) startCreate();
  }

  function addSkill() {
    const v = skillInput.trim();
    if (!v) return;
    if (form.skills.includes(v)) return;
    setForm({ ...form, skills: [...form.skills, v] });
    setSkillInput("");
  }

  function removeSkill(s: string) {
    setForm({ ...form, skills: form.skills.filter((x) => x !== s) });
  }

  return (
    <div className="bg-background text-on-surface min-h-screen">
      <header className="fixed top-0 w-full z-40 bg-[#fbf9f5]/80 backdrop-blur-xl">
        <div className="flex items-center gap-3 px-6 h-16 w-full max-w-lg mx-auto">
          <Link to="/" className="text-primary">
            <span className="material-symbols-outlined">arrow_back</span>
          </Link>
          <h1 className="font-headline font-bold tracking-tight text-xl text-on-surface">
            Admin · Psychologists
          </h1>
        </div>
      </header>

      <main className="pt-20 pb-32 max-w-lg mx-auto px-6 space-y-6">
        <section className="bg-surface-container-lowest p-5 rounded-xl space-y-3">
          <h2 className="font-headline font-bold text-lg">
            {editingId === null ? "New practitioner" : `Edit #${editingId}`}
          </h2>

          <Field label="Name">
            <input
              className="input"
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
            />
          </Field>

          <Field label="Title">
            <input
              className="input"
              value={form.title}
              onChange={(e) => setForm({ ...form, title: e.target.value })}
            />
          </Field>

          <Field label="Photo URL">
            <input
              className="input"
              value={form.photoUrl}
              onChange={(e) => setForm({ ...form, photoUrl: e.target.value })}
            />
          </Field>

          <Field label="Bio">
            <textarea
              className="input min-h-[100px]"
              value={form.bio}
              onChange={(e) => setForm({ ...form, bio: e.target.value })}
            />
          </Field>

          <Field label="Skills (tags)">
            <div className="flex flex-wrap gap-2 mb-2">
              {form.skills.map((s) => (
                <button
                  key={s}
                  onClick={() => removeSkill(s)}
                  className="bg-secondary-container text-on-secondary-container px-3 py-1 rounded-full text-xs flex items-center gap-1"
                >
                  {s}
                  <span className="material-symbols-outlined text-[14px]">close</span>
                </button>
              ))}
            </div>
            <div className="flex gap-2">
              <input
                className="input"
                placeholder="e.g. stress"
                value={skillInput}
                onChange={(e) => setSkillInput(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === "Enter") {
                    e.preventDefault();
                    addSkill();
                  }
                }}
              />
              <button
                type="button"
                onClick={addSkill}
                className="px-4 bg-primary text-on-primary rounded-xl text-sm font-semibold"
              >
                Add
              </button>
            </div>
          </Field>

          <div className="grid grid-cols-2 gap-3">
            <Field label="Rating">
              <input
                type="number"
                step="0.1"
                min="0"
                max="5"
                className="input"
                value={form.rating}
                onChange={(e) =>
                  setForm({ ...form, rating: parseFloat(e.target.value) || 0 })
                }
              />
            </Field>
            <Field label="Reviews">
              <input
                type="number"
                min="0"
                className="input"
                value={form.reviewCount}
                onChange={(e) =>
                  setForm({ ...form, reviewCount: parseInt(e.target.value) || 0 })
                }
              />
            </Field>
          </div>

          <Field label="Next available">
            <input
              className="input"
              placeholder="e.g. 14:00"
              value={form.nextAvailable}
              onChange={(e) => setForm({ ...form, nextAvailable: e.target.value })}
            />
          </Field>

          <div className="flex gap-4">
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={form.verified}
                onChange={(e) => setForm({ ...form, verified: e.target.checked })}
              />
              Verified
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={form.availableToday}
                onChange={(e) => setForm({ ...form, availableToday: e.target.checked })}
              />
              Available today
            </label>
          </div>

          <div className="flex gap-2 pt-2">
            <button
              onClick={save}
              disabled={saving || !form.name.trim()}
              className="flex-1 bg-primary text-on-primary rounded-xl py-3 font-semibold disabled:opacity-40"
            >
              {editingId === null ? "Create" : "Save"}
            </button>
            {editingId !== null && (
              <button
                onClick={startCreate}
                className="px-4 bg-surface-container-high rounded-xl font-semibold"
              >
                Cancel
              </button>
            )}
          </div>
        </section>

        <section className="space-y-3">
          <h2 className="font-headline font-bold text-xl">All practitioners ({list.length})</h2>
          {loading && <div className="text-sm text-on-surface-variant">Loading…</div>}
          {list.map((p) => (
            <div
              key={p.id}
              className="bg-surface-container-lowest p-4 rounded-xl flex items-center gap-3"
            >
              <div className="flex-1">
                <div className="font-headline font-bold">{p.name}</div>
                <div className="text-sm text-on-surface-variant">{p.title}</div>
                <div className="text-xs text-on-surface-variant mt-1">
                  {p.skills.join(", ")}
                </div>
              </div>
              <button
                onClick={() => startEdit(p)}
                className="text-primary text-sm font-semibold px-3 py-1"
              >
                Edit
              </button>
              <button
                onClick={() => remove(p.id)}
                className="text-error text-sm font-semibold px-3 py-1"
              >
                Delete
              </button>
            </div>
          ))}
        </section>
      </main>

      <style>{`
        .input {
          width: 100%;
          height: 44px;
          padding: 0 14px;
          background-color: #e3e3dc;
          border-radius: 12px;
          border: none;
          outline: none;
          color: #31332f;
          font-size: 14px;
        }
        textarea.input {
          padding: 12px 14px;
          height: auto;
        }
        .input:focus {
          box-shadow: 0 0 0 2px rgba(55, 103, 103, 0.4);
        }
      `}</style>
    </div>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="block">
      <span className="block text-xs font-semibold text-on-surface-variant mb-1">{label}</span>
      {children}
    </label>
  );
}
