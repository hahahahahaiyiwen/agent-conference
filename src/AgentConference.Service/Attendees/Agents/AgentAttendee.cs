using System;
using System.ClientModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;

namespace AgentConference.Service.Attendees;

public class AgentAttendee : Attendee
{
    private readonly ChatClientAgent _agent;
    private readonly string _model;
    private readonly ILogger<AgentAttendee> _logger;
    private AgentThread _thread;

    public AgentAttendee(string name, string model, ChatClientAgent agent, ILogger<AgentAttendee> logger) : base(name)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _agent = agent ?? throw new ArgumentNullException(nameof(agent));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override string ToString()
    {
        return $"Agent: {Name} (Model: {_model})";
    }

    public override async Task<AttendeeResponse<T>> ThinkAndSpeak<T>(string question, CancellationToken cancellationToken)
    {
        if (_thread == null)
        {
            _thread = _agent.GetNewThread();
        }

        try
        {
            AgentRunResponse<T> response = await Invoke(
                async (cancellationToken) =>
                {
                    return await _agent.RunAsync<T>(question, _thread, cancellationToken: cancellationToken);
                },
                cancellationToken);

            if (response != null)
            {
                AttendeeResponse<T> attendeeResponse = new AttendeeResponse<T>
                {
                    Response = response.Result,
                    Attendee = this,
                };

                return attendeeResponse;
            }
        }
        catch (ArgumentException)
        {
            // Ignore argument exceptions
        }
        catch (JsonException)
        {
            // Ignore JSON parsing errors
        }

        return null;
    }

    private async Task<T> Invoke<T>(Func<CancellationToken, Task<T>> func, CancellationToken cancellationToken)
    {
        int count = 0;

        while (true)
        {
            try
            {
                return await func(cancellationToken);
            }
            catch (ClientResultException ex) when (ex.Status != 400)
            {
                _logger.LogWarning(ex, "ClientResultException occurred: {Message}", ex.Message);

                Exception exception = ex.InnerException;

                await Task.Delay(1000, cancellationToken);
            }
            catch (ClientResultException ex) when (ex.Status == 400)
            {
                if (++count >= 3)
                {
                    throw;
                }

                // Handle rate limiting by waiting and retrying
                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}