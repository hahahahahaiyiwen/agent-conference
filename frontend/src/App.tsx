import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { ChatPanel } from './components/ChatPanel';
import { ProblemPanel } from './components/ProblemPanel';
import type { AsyncOperationResponse, RoomEventResponse, SolveProblemRequest } from './types';

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '');

const ENDPOINTS = {
  solve: `${API_BASE_URL}/api/AgentConference/solve/async`,
  monitor: (id: string) => `${API_BASE_URL}/api/Monitoring/${encodeURIComponent(id)}`,
  operation: (id: string) => `${API_BASE_URL}/api/AsyncOperation/${encodeURIComponent(id)}`
};

const POLL_INTERVAL_MS = 2000;

export default function App() {
  const [events, setEvents] = useState<RoomEventResponse[]>([]);
  const [operationId, setOperationId] = useState<string | undefined>();
  const [monitorId, setMonitorId] = useState<string | undefined>();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isPolling, setIsPolling] = useState(false);
  const [statusMessage, setStatusMessage] = useState<string | undefined>('Idle');
  const [resultPayload, setResultPayload] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const isMountedRef = useRef(true);
  const hasFetchedResultRef = useRef(false);

  useEffect(() => {
    return () => {
      isMountedRef.current = false;
    };
  }, []);

  const submitProblem = useCallback(async (payload: SolveProblemRequest) => {
    setIsSubmitting(true);
    setErrorMessage(null);
    setEvents([]);
    setResultPayload(null);
    setStatusMessage('Contacting agents…');
    setMonitorId(undefined);
    setOperationId(undefined);
    setIsPolling(false);
    hasFetchedResultRef.current = false;

    try {
      const response = await fetch(ENDPOINTS.solve, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });

      if (!response.ok) {
        const failure = await response.text();
        throw new Error(failure || 'Failed to start discussion.');
      }

      const data = (await response.json()) as AsyncOperationResponse;

      if (!data.monitorId || !data.operationId) {
        throw new Error('Server response missing monitor or operation identifier.');
      }

      setOperationId(data.operationId);
      setMonitorId(data.monitorId);
      setStatusMessage('Discussion in progress');
      setIsPolling(true);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unexpected error starting discussion.';
      setErrorMessage(message);
      setStatusMessage('Idle');
    } finally {
      setIsSubmitting(false);
    }
  }, []);

  const fetchFinalResult = useCallback(
    async (operation: string) => {
      if (!operation || hasFetchedResultRef.current) {
        return;
      }

      hasFetchedResultRef.current = true;

      try {
        const response = await fetch(ENDPOINTS.operation(operation));

        if (!response.ok) {
          const failure = await response.text();
          throw new Error(failure || 'Unable to retrieve final result.');
        }

        const data = (await response.json()) as AsyncOperationResponse;
        setStatusMessage(data.status ?? 'Completed');
        setResultPayload(data.result ?? null);
        setErrorMessage(data.errorMessage ?? null);
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Unexpected error retrieving final result.';
        setErrorMessage(message);
        setStatusMessage('Failed');
      } finally {
        setIsPolling(false);
      }
    },
    []
  );

  useEffect(() => {
    if (!monitorId) {
      setIsPolling(false);
      return;
    }

    let cancelled = false;

    const poll = async () => {
      try {
        const response = await fetch(ENDPOINTS.monitor(monitorId));

        if (response.status === 404) {
          setMonitorId(undefined);
          setIsPolling(false);
          setStatusMessage('Discussion complete');
          if (operationId) {
            await fetchFinalResult(operationId);
          }
          return;
        }

        if (!response.ok) {
          const failure = await response.text();
          throw new Error(failure || 'Failed to poll monitor.');
        }

        const data = (await response.json()) as RoomEventResponse[];

        if (data.length > 0 && !cancelled) {
          setEvents((previous: RoomEventResponse[]) => [...previous, ...data]);
        }
      } catch (error) {
        if (!cancelled) {
          const message = error instanceof Error ? error.message : 'Unexpected monitor error.';
          setErrorMessage(message);
          setIsPolling(false);
        }
      }
    };

    poll();
    const intervalId = window.setInterval(poll, POLL_INTERVAL_MS);

    return () => {
      cancelled = true;
      window.clearInterval(intervalId);
    };
  }, [monitorId, operationId, fetchFinalResult]);

  const chatStatus = useMemo(() => {
    if (isPolling) {
      return 'Live';
    }

    return statusMessage ?? 'Idle';
  }, [isPolling, statusMessage]);

  return (
    <div className="app-shell">
      <ChatPanel events={events} statusMessage={chatStatus} />
      <ProblemPanel
        onSubmit={submitProblem}
        isSubmitting={isSubmitting}
        activeOperationId={operationId}
        statusMessage={statusMessage}
        resultPayload={resultPayload}
        errorMessage={errorMessage}
      />
    </div>
  );
}
