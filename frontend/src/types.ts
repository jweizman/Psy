export interface Psychologist {
  id: number;
  name: string;
  title: string;
  photoUrl: string;
  bio: string;
  skills: string[];
  rating: number;
  reviewCount: number;
  nextAvailable: string;
  verified: boolean;
  availableToday: boolean;
}

export type ChatRole = "user" | "assistant";

export interface ChatMessage {
  role: ChatRole;
  content: string;
}

export type ChatPhase =
  | "initial"
  | "awaiting_confirmation"
  | "awaiting_followup"
  | "done";

export interface ChatResponse {
  reply: string;
  phase: ChatPhase;
  matchedPsychologistId: number | null;
  reasoning: string | null;
}

export interface Review {
  id: number;
  psychologistId: number;
  author: string;
  initials: string;
  rating: number;
  text: string;
  createdAt: string;
}

export interface Slot {
  id: number;
  psychologistId: number;
  startUtc: string;
  durationMinutes: number;
  booked: boolean;
}

export interface SkillIcon {
  id: number;
  name: string;
  icon: string;
}

export interface Sentiment {
  label: "positive" | "neutral" | "negative";
  intensity: number;
  emotions: string[];
  note: string;
}
