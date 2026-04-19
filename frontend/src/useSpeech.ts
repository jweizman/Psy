import { useEffect, useRef, useState } from "react";

type SRConstructor = new () => SpeechRecognitionLike;

interface SpeechRecognitionLike {
  continuous: boolean;
  interimResults: boolean;
  lang: string;
  onresult: ((e: SpeechRecognitionEventLike) => void) | null;
  onerror: ((e: unknown) => void) | null;
  onend: (() => void) | null;
  start(): void;
  stop(): void;
  abort(): void;
}

interface SpeechRecognitionEventLike {
  resultIndex: number;
  results: {
    length: number;
    [i: number]: {
      isFinal: boolean;
      [j: number]: { transcript: string };
    };
  };
}

function getCtor(): SRConstructor | null {
  const w = window as unknown as {
    SpeechRecognition?: SRConstructor;
    webkitSpeechRecognition?: SRConstructor;
  };
  return w.SpeechRecognition ?? w.webkitSpeechRecognition ?? null;
}

export interface SpeechState {
  supported: boolean;
  listening: boolean;
  transcript: string;
  interim: string;
  error: string | null;
  start: () => void;
  stop: () => void;
  reset: () => void;
}

export function useSpeech(lang = "en-US"): SpeechState {
  const [supported] = useState<boolean>(() => getCtor() !== null);
  const [listening, setListening] = useState(false);
  const [transcript, setTranscript] = useState("");
  const [interim, setInterim] = useState("");
  const [error, setError] = useState<string | null>(null);
  const recogRef = useRef<SpeechRecognitionLike | null>(null);

  useEffect(() => {
    return () => {
      recogRef.current?.abort();
    };
  }, []);

  function start() {
    if (!supported) {
      setError("Speech recognition is not supported in this browser.");
      return;
    }
    const Ctor = getCtor();
    if (!Ctor) return;
    const rec = new Ctor();
    rec.continuous = true;
    rec.interimResults = true;
    rec.lang = lang;

    rec.onresult = (e) => {
      let finalText = "";
      let interimText = "";
      for (let i = e.resultIndex; i < e.results.length; i++) {
        const chunk = e.results[i];
        const t = chunk[0]?.transcript ?? "";
        if (chunk.isFinal) finalText += t;
        else interimText += t;
      }
      if (finalText) setTranscript((prev) => (prev ? prev + " " : "") + finalText.trim());
      setInterim(interimText);
    };
    rec.onerror = (e) => {
      const msg =
        typeof e === "object" && e && "error" in e
          ? String((e as { error: unknown }).error)
          : "speech error";
      setError(msg);
      setListening(false);
    };
    rec.onend = () => {
      setListening(false);
      setInterim("");
    };

    setError(null);
    setTranscript("");
    setInterim("");
    try {
      rec.start();
      recogRef.current = rec;
      setListening(true);
    } catch (e) {
      setError(String(e));
    }
  }

  function stop() {
    recogRef.current?.stop();
    setListening(false);
  }

  function reset() {
    setTranscript("");
    setInterim("");
    setError(null);
  }

  return { supported, listening, transcript, interim, error, start, stop, reset };
}
