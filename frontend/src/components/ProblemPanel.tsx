import { ChangeEvent, FormEvent, useState } from 'react';
import type { ProblemSolvingOptionsDto, SolveProblemRequest } from '../types';

type ProblemPanelProps = {
  onSubmit: (payload: SolveProblemRequest) => Promise<void>;
  isSubmitting: boolean;
  activeOperationId?: string;
  statusMessage?: string;
  resultPayload?: string | null;
  errorMessage?: string | null;
};

const DEFAULT_ATTENDEES = 3;
const DEFAULT_TIME_LIMIT_SECONDS = 60;

export function ProblemPanel({
  onSubmit,
  isSubmitting,
  activeOperationId,
  statusMessage,
  resultPayload,
  errorMessage
}: ProblemPanelProps) {
  const [statement, setStatement] = useState('');
  const [context, setContext] = useState('');
  const [numberOfAttendees, setNumberOfAttendees] = useState<number>(DEFAULT_ATTENDEES);
  const [timeLimitSeconds, setTimeLimitSeconds] = useState<number>(DEFAULT_TIME_LIMIT_SECONDS);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const options: ProblemSolvingOptionsDto = {
      numberOfAttendees,
      timeLimitSeconds
    };

    const payload: SolveProblemRequest = {
      problem: {
        statement: statement.trim(),
        context: context.trim() || undefined
      },
      options
    };

    await onSubmit(payload);
  };

  const renderResult = () => {
    if (!resultPayload) {
      return null;
    }

    try {
      const parsed = JSON.parse(resultPayload);
      return <pre className="panel__result">{JSON.stringify(parsed, null, 2)}</pre>;
    } catch {
      return <pre className="panel__result">{resultPayload}</pre>;
    }
  };

  return (
    <section className="panel problem-panel" aria-label="Problem setup">
      <header className="panel__header">
        <h2>Problem</h2>
        {statusMessage && <span className="panel__badge">{statusMessage}</span>}
      </header>
      <form className="problem-form" onSubmit={handleSubmit}>
        <label className="form-field">
          <span>Problem Statement</span>
          <textarea
            required
            value={statement}
            onChange={(event) => setStatement(event.target.value)}
            placeholder="Describe the problem to solve"
            rows={6}
          />
        </label>

        <label className="form-field">
          <span>Context (optional)</span>
          <textarea
            value={context}
            onChange={(event) => setContext(event.target.value)}
            placeholder="Additional context that could help the discussion"
            rows={4}
          />
        </label>

        <div className="form-grid">
          <label className="form-field">
            <span>Attendees</span>
            <input
              type="number"
              min={1}
              value={numberOfAttendees}
              onChange={(event: ChangeEvent<HTMLInputElement>) => {
                const value = Number(event.target.value);
                setNumberOfAttendees(Number.isFinite(value) && value > 0 ? value : DEFAULT_ATTENDEES);
              }}
            />
          </label>
          <label className="form-field">
            <span>Time Limit (seconds)</span>
            <input
              type="number"
              min={1}
              value={timeLimitSeconds}
              onChange={(event: ChangeEvent<HTMLInputElement>) => {
                const value = Number(event.target.value);
                setTimeLimitSeconds(Number.isFinite(value) && value > 0 ? value : DEFAULT_TIME_LIMIT_SECONDS);
              }}
            />
          </label>
        </div>

        <button type="submit" className="primary-button" disabled={isSubmitting || statement.trim().length === 0}>
          {isSubmitting ? 'Submitting…' : 'Start Discussion'}
        </button>
      </form>

      {activeOperationId && (
        <div className="panel__meta">
          <span className="panel__meta-label">Operation Id:</span>
          <code>{activeOperationId}</code>
        </div>
      )}

      {errorMessage && <p className="panel__error">{errorMessage}</p>}

      {renderResult()}
    </section>
  );
}
