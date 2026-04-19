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
