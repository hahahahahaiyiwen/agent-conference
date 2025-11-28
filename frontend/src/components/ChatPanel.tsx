import { useMemo } from 'react';
import type { RoomEventResponse } from '../types';

type ChatPanelProps = {
  events: RoomEventResponse[];
  statusMessage?: string;
};

type ParsedEvent = {
  actor?: string;
  message: string;
  reasoning?: string;
};

type NormalizedMessage = {
  message: string;
  reasoning?: string;
};

function normalizeMessage(value: unknown, fallback: string): NormalizedMessage {
  if (typeof value === 'string') {
    const trimmed = value.trim();

    if (trimmed.length === 0) {
      return { message: fallback };
    }

    try {
      const parsed = JSON.parse(trimmed);

      if (parsed && typeof parsed === 'object') {
        const container = parsed as Record<string, unknown>;
        const message = container.Message;
        const reasoning = container.Reasoning;

        const messageText = typeof message === 'string' ? message.trim() : undefined;
        const reasoningText = typeof reasoning === 'string' ? reasoning.trim() : undefined;

        if (messageText || reasoningText) {
          if (messageText && reasoningText && reasoningText !== messageText) {
            return { message: messageText, reasoning: reasoningText };
          }

          const primary = messageText ?? reasoningText!;
          return { message: primary };
        }
      }
    } catch {
      // Not JSON - fall back to plain text
    }

    return { message: trimmed };
  }

  return { message: fallback };
}

function parseEvent(event: RoomEventResponse): ParsedEvent {
  try {
    const parsed = JSON.parse(event.message) as Record<string, unknown>;

    const messageValue = parsed.Message;
    const actorValue = parsed.Actor;

    const { message, reasoning } = normalizeMessage(messageValue, event.message);

    const actor = typeof actorValue === 'string' && actorValue.trim().length > 0 ? actorValue.trim() : undefined;

    return { actor, message, reasoning };
  } catch {
    return { actor: undefined, message: event.message };
  }
}

export function ChatPanel({ events, statusMessage }: ChatPanelProps) {
  const rows = useMemo(() => {
    return events.map((event, index) => {
      const parsed = parseEvent(event);
      return (
        <li key={`${event.timestamp}-${index}`} className="chat-row">
          <header className="chat-row__meta">
            <span className="chat-row__time">{new Date(event.timestamp).toLocaleTimeString()}</span>
            <span className="chat-row__name">{event.name}</span>
          </header>
          <div className="chat-row__body">
            {parsed.actor && <span className="chat-row__actor">{parsed.actor}</span>}
            <p className="chat-row__message">{parsed.message}</p>
            {parsed.reasoning && <p className="chat-row__reasoning">{parsed.reasoning}</p>}
          </div>
        </li>
      );
    });
  }, [events]);

  return (
    <section className="panel chat-panel" aria-label="Discussion transcript">
      <header className="panel__header">
        <h2>Discussion</h2>
        {statusMessage && <span className="panel__badge">{statusMessage}</span>}
      </header>
      <ol className="chat-log">{rows}</ol>
      {rows.length === 0 && <p className="panel__placeholder">Submit a problem to start the conversation.</p>}
    </section>
  );
}
