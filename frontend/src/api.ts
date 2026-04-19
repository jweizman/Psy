import type { ChatMessage, ChatPhase, ChatResponse, Psychologist } from "./types";

const BASE =
  (import.meta.env.VITE_API_URL as string | undefined) ?? "http://localhost:5080";

async function req<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...init,
  });
  if (!res.ok) throw new Error(`${res.status} ${await res.text()}`);
  if (res.status === 204) return undefined as T;
  return res.json();
}

export const api = {
  listPsychologists: (q?: string) =>
    req<Psychologist[]>(`/api/psychologists${q ? `?q=${encodeURIComponent(q)}` : ""}`),
  getPsychologist: (id: number) => req<Psychologist>(`/api/psychologists/${id}`),
  createPsychologist: (p: Omit<Psychologist, "id">) =>
    req<Psychologist>("/api/psychologists", { method: "POST", body: JSON.stringify(p) }),
  updatePsychologist: (id: number, p: Omit<Psychologist, "id">) =>
    req<Psychologist>(`/api/psychologists/${id}`, {
      method: "PUT",
      body: JSON.stringify(p),
    }),
  deletePsychologist: (id: number) =>
    req<void>(`/api/psychologists/${id}`, { method: "DELETE" }),
  chat: (history: ChatMessage[], phase: ChatPhase) =>
    req<ChatResponse>("/api/chat", {
      method: "POST",
      body: JSON.stringify({ history, phase }),
    }),
};
