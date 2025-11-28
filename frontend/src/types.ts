export interface ProblemDefinitionDto {
  statement: string;
  context?: string;
}

export interface AttendeeOptionsDto {
  name?: string;
  model?: string;
  instruction?: string;
}

export interface ProblemSolvingOptionsDto {
  numberOfAttendees?: number;
  timeLimitSeconds?: number;
  memoryLimitInMB?: number;
  attendeeOptions?: AttendeeOptionsDto[];
}

export interface SolveProblemRequest {
  problem: ProblemDefinitionDto;
  options?: ProblemSolvingOptionsDto;
}

export interface AsyncOperationResponse {
  operationId: string;
  monitorId?: string;
  status?: string;
  result?: string | null;
  errorCode?: string | null;
  errorMessage?: string | null;
}

export interface RoomEventResponse {
  timestamp: string;
  name: string;
  message: string;
}
