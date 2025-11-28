using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AgentConference.Primitives;
using AgentConference.Service;
using AgentConference.Service.Async;
using AgentConference.Service.Attendees;
using AgentConference.Service.Monitoring;

namespace AgentConference.WebApi.Contracts;

internal static class AgentConferenceContractMapper
{
    public static ProblemDefinition ToDomain(this ProblemDefinitionDto input)
    {
        if (input == null)
        {
            return null;
        }

        return new ProblemDefinition(input.Statement, input.Context)
        {
            Metadata = input.Metadata?.Select(m => m.ToDomain()).ToArray()
        };
    }

    public static EvaluationProblem ToDomain(this EvaluationProblemDto input)
    {
        if (input == null)
        {
            return null;
        }

        return new EvaluationProblem(input.GroundTruth, input.Query, input.Response, input.Criteria)
        {
            Metadata = input.Metadata?.Select(m => m.ToDomain()).ToArray()
        };
    }

    public static ProblemSolvingOptions ToDomain(this ProblemSolvingOptionsDto input)
    {
        ProblemSolvingOptions options = new ProblemSolvingOptions();

        if (input == null)
        {
            return options;
        }

        if (input.TimeLimitSeconds.HasValue)
        {
            int safeSeconds = Math.Max(1, input.TimeLimitSeconds.Value);
            options.TimeLimit = TimeSpan.FromSeconds(safeSeconds);
        }

        if (input.MemoryLimitInMB.HasValue)
        {
            options.MemoryLimitInMB = input.MemoryLimitInMB.Value;
        }

        List<AttendeeCreationOptions> attendeeCreationOptions = new List<AttendeeCreationOptions>();

        if (input.NumberOfAttendees.HasValue)
        {
            int numberOfAttendees = Math.Max(1, input.NumberOfAttendees.Value);

            for (int i = 0; i < numberOfAttendees; i++)
            {
                attendeeCreationOptions.Add(new AttendeeCreationOptions());
            }
        }
        else
        {
            if (input.AttendeeOptions != null && input.AttendeeOptions.Length > 0)
            {
                foreach (var attendeeOptionDto in input.AttendeeOptions)
                {
                    attendeeCreationOptions.Add(
                        new AttendeeCreationOptions
                        {
                            Name = attendeeOptionDto.Name,
                            Model = attendeeOptionDto.Model,
                            Instruction = attendeeOptionDto.Instruction
                        });
                }
            }
        }

        options.AttendeeOptions = attendeeCreationOptions;

        return options;
    }

    public static Metadata ToDomain(this MetadataDto input)
    {
        if (input == null)
        {
            return null;
        }

        return new Metadata
        {
            Key = input.Key,
            Value = input.Value
        };
    }

    public static AsyncOperationResponse ToResponse(this AsyncOperation asyncOperation)
    {
        if (asyncOperation == null)
        {
            return null;
        }

        return new AsyncOperationResponse
        {
            OperationId = asyncOperation.Id,
            MonitorId = asyncOperation.MonitorId,
            Status = asyncOperation.Status,
            Result = asyncOperation.Result,
        };
    }

    public static DeliverableResponse ToResponse<T>(this Deliverable<T> deliverable)
    {
        if (deliverable == null)
        {
            return null;
        }

        return new DeliverableResponse
        {
            Id = deliverable.Id,
            Items = deliverable.Items?.Select(x => JsonSerializer.Serialize(x)).ToList(),
            Metadatas = deliverable.Metadata?.Select(m => m.ToDto()).ToList()
        };
    }

    public static RoomEventResponse ToResponse(this RoomEvent roomEvent)
    {
        return new RoomEventResponse
        {
            Timestamp = roomEvent.Timestamp,
            Name = roomEvent.EventName.ToString(),
            Message = JsonSerializer.Serialize(roomEvent.Properties ?? new Dictionary<string, string>())
        };
    }


    private static MetadataDto ToDto(this Metadata input)
    {
        if (input == null)
        {
            return null;
        }

        return new MetadataDto
        {
            Key = input.Key,
            Value = input.Value
        };
    }
}
