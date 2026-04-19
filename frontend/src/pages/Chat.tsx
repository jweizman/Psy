import { useEffect, useRef, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { api } from "../api";
import type {
  ChatMessage,
  ChatPhase,
  Psychologist,
  Sentiment,
} from "../types";
import { useSpeech } from "../useSpeech";

type EnrichedMessage = ChatMessage & { sentiment?: Sentiment };

export default function Chat() {
  const [history, setHistory] = useState<EnrichedMessage[]>([
    {
      role: "assistant",
      content: "Hi there. In a few words, tell me what brings you here today.",
    },
  ]);
  const [phase, setPhase] = useState<ChatPhase>("initial");
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [match, setMatch] = useState<Psychologist | null>(null);
  const endRef = useRef<HTMLDivElement>(null);
  const location = useLocation();
  const initialSentRef = useRef(false);
  const speech = useSpeech("en-US");

  useEffect(() => {
    endRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [history, loading]);

  useEffect(() => {
    if (initialSentRef.current) return;
    const state = location.state as { initialMessage?: string } | null;
    const seed = state?.initialMessage?.trim();
    if (!seed) return;
    initialSentRef.current = true;
    void sendText(seed);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location.state]);

  useEffect(() => {
    if (speech.transcript) {
      setInput((prev) =>
        prev
          ? prev.trimEnd() + " " + speech.transcript
          : speech.transcript,
      );
      speech.reset();
    }
  }, [speech.transcript, speech]);

  async function sendText(text: string) {
    const t = text.trim();
    if (!t || loading || phase === "done") return;

    const userMsg: EnrichedMessage = { role: "user", content: t };
    const nextHistory: EnrichedMessage[] = [...history, userMsg];
    const userIndex = nextHistory.length - 1;
    setHistory(nextHistory);
    setLoading(true);

    const sentimentPromise = api
      .sentiment(t)
      .then((s) => {
        setHistory((prev) => {
          const copy = [...prev];
          if (copy[userIndex] && copy[userIndex].role === "user") {
            copy[userIndex] = { ...copy[userIndex], sentiment: s };
          }
          return copy;
        });
      })
      .catch(() => {});

    try {
      const res = await api.chat(
        nextHistory.map(({ role, content }) => ({ role, content })),
        phase,
      );
      setHistory((prev) => [
        ...prev,
        { role: "assistant", content: res.reply },
      ]);
      setPhase(res.phase);
      if (res.matchedPsychologistId) {
        try {
          const p = await api.getPsychologist(res.matchedPsychologistId);
          setMatch(p);
        } catch {
          /* ignore */
        }
      }
    } catch {
      setHistory((prev) => [
        ...prev,
        {
          role: "assistant",
          content:
            "Sorry, something went wrong. Please check that the server is running and that the API key is configured.",
        },
      ]);
    } finally {
      setLoading(false);
      await sentimentPromise;
    }
  }

  async function send() {
    const text = input;
    setInput("");
    if (speech.listening) speech.stop();
    await sendText(text);
  }

  function toggleMic() {
    if (speech.listening) speech.stop();
    else speech.start();
  }

  return (
    <div className="bg-background text-on-surface min-h-screen">
      <header className="fixed top-0 w-full z-40 bg-[#fbf9f5]/80 backdrop-blur-xl">
        <div className="flex items-center gap-3 px-6 h-16 w-full max-w-lg mx-auto">
          <Link to="/" className="text-primary">
            <span className="material-symbols-outlined">arrow_back</span>
          </Link>
          <h1 className="font-headline font-bold tracking-tight text-xl text-on-surface">
            Matching
          </h1>
        </div>
      </header>

      <main className="pt-20 pb-36 max-w-lg mx-auto px-6 space-y-3">
        {history.map((m, i) => (
          <Bubble
            key={i}
            role={m.role}
            content={m.content}
            sentiment={m.sentiment}
          />
        ))}
        {loading && <Bubble role="assistant" content="…" />}
        {speech.listening && speech.interim && (
          <Bubble
            role="user"
            content={`🎙 ${speech.interim}`}
            faded
          />
        )}
        {match && (
          <div className="bg-surface-container-lowest p-5 rounded-xl flex gap-4 items-center mt-4 shadow-[0_10px_30px_rgba(49,51,47,0.06)]">
            {match.photoUrl && (
              <img
                src={match.photoUrl}
                alt={match.name}
                className="w-16 h-16 rounded-2xl object-cover"
              />
            )}
            <div className="flex-1">
              <h3 className="font-headline font-bold text-lg">{match.name}</h3>
              <p className="text-on-surface-variant text-sm">{match.title}</p>
              <p className="text-xs text-on-surface-variant mt-1">
                Next: {match.nextAvailable}
              </p>
            </div>
            <Link
              to={`/practitioner/${match.id}`}
              className="bg-primary text-on-primary px-4 py-2 rounded-full text-sm font-semibold"
            >
              View
            </Link>
          </div>
        )}
        <div ref={endRef} />
      </main>

      <div className="fixed bottom-20 w-full z-40 bg-[#fbf9f5]/80 backdrop-blur-xl">
        <div className="max-w-lg mx-auto px-6 py-3 flex gap-2 items-center">
          {speech.supported && (
            <button
              onClick={toggleMic}
              disabled={phase === "done" || loading}
              title={speech.listening ? "Stop recording" : "Speak your message"}
              className={
                "w-12 h-12 rounded-full flex items-center justify-center transition-colors disabled:opacity-40 " +
                (speech.listening
                  ? "bg-error text-on-error animate-pulse"
                  : "bg-surface-container-high text-primary")
              }
            >
              <span
                className="material-symbols-outlined"
                style={{ fontVariationSettings: "'FILL' 1" }}
              >
                {speech.listening ? "stop" : "mic"}
              </span>
            </button>
          )}
          <input
            className="flex-1 h-12 px-4 bg-surface-container-highest rounded-xl outline-none focus:ring-2 focus:ring-primary/40 text-on-surface placeholder:text-on-surface-variant/50"
            placeholder={
              speech.listening
                ? "Listening…"
                : phase === "done"
                  ? "Session ended"
                  : "Your message…"
            }
            value={input}
            disabled={phase === "done" || loading}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") send();
            }}
          />
          <button
            onClick={send}
            disabled={phase === "done" || loading || !input.trim()}
            className="w-12 h-12 rounded-full bg-primary text-on-primary flex items-center justify-center disabled:opacity-40"
          >
            <span className="material-symbols-outlined">send</span>
          </button>
        </div>
        {speech.error && (
          <div className="max-w-lg mx-auto px-6 pb-2 text-xs text-error">
            Mic error: {speech.error}
          </div>
        )}
      </div>
    </div>
  );
}

function Bubble({
  role,
  content,
  sentiment,
  faded,
}: {
  role: "user" | "assistant";
  content: string;
  sentiment?: Sentiment;
  faded?: boolean;
}) {
  const mine = role === "user";
  return (
    <div
      className={
        "flex flex-col " + (mine ? "items-end" : "items-start")
      }
    >
      <div
        className={
          "max-w-[85%] px-4 py-3 rounded-2xl font-body text-sm leading-relaxed " +
          (mine
            ? "bg-primary text-on-primary rounded-br-sm"
            : "bg-surface-container-high text-on-surface rounded-bl-sm") +
          (faded ? " opacity-60 italic" : "")
        }
      >
        {content}
      </div>
      {sentiment && <SentimentBadge s={sentiment} />}
    </div>
  );
}

function SentimentBadge({ s }: { s: Sentiment }) {
  const emoji =
    s.label === "negative" ? "🌧" : s.label === "positive" ? "☀️" : "🌤";
  const tone =
    s.label === "negative"
      ? "bg-error-container text-on-error-container"
      : s.label === "positive"
        ? "bg-secondary-container text-on-secondary-container"
        : "bg-surface-container-high text-on-surface";
  return (
    <div
      className={
        "mt-1 px-2.5 py-1 rounded-full text-[11px] font-semibold flex items-center gap-1.5 " +
        tone
      }
      title={s.note || undefined}
    >
      <span>{emoji}</span>
      <span className="uppercase tracking-wide">{s.label}</span>
      {s.intensity > 0 && <span className="opacity-70">· {s.intensity}/5</span>}
      {s.emotions.length > 0 && (
        <span className="opacity-80 normal-case font-medium">
          · {s.emotions.slice(0, 2).join(", ")}
        </span>
      )}
    </div>
  );
}
