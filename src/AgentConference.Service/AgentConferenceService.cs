using AgentConference.Primitives;
using AgentConference.Service.Attendees;
using AgentConference.Service.Accomodation;
using AgentConference.Service.Async;
using AgentConference.Service.Monitoring;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using AgentConference.Service.Notes;
using System.Text.Json;

namespace AgentConference.Service;

internal class AgentConferenceService : IConferenceService
{
    private readonly TimeSpan AsyncSolveTimeout = TimeSpan.FromMinutes(5);
    private readonly IRoomProvider _roomProvider;
    private readonly IAttendeeProvider _attendeeProvider;
    private readonly IMonitorProvider _monitorProvider;
    private readonly ISubscriberProvider _subscriberProvider;
    private readonly IMeetingNotesArchive _meetingNotesArchive;
    private readonly IAsyncOperationProvider _asyncOperationProvider;
    private readonly AgentConferenceServiceOptions _options;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<AgentConferenceService> _logger;

    public AgentConferenceService(
        IRoomProvider roomProvider,
        IAttendeeProvider attendeeProvider,
        IMonitorProvider monitorProvider,
        ISubscriberProvider subscriberProvider,
        IMeetingNotesArchive meetingNotesArchive,
        IAsyncOperationProvider asyncOperationProvider,
        IOptions<AgentConferenceServiceOptions> options,
        ILoggerFactory loggerFactory)
    {
        _roomProvider = roomProvider ?? throw new ArgumentNullException(nameof(roomProvider));
        _attendeeProvider = attendeeProvider ?? throw new ArgumentNullException(nameof(attendeeProvider));
        _monitorProvider = monitorProvider ?? throw new ArgumentNullException(nameof(monitorProvider));
        _subscriberProvider = subscriberProvider ?? throw new ArgumentNullException(nameof(subscriberProvider));
        _meetingNotesArchive = meetingNotesArchive ?? throw new ArgumentNullException(nameof(meetingNotesArchive));
        _asyncOperationProvider = asyncOperationProvider ?? throw new ArgumentNullException(nameof(asyncOperationProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<AgentConferenceService>();
    }

    public async Task<Deliverable<T>> Solve<T>(ProblemDefinition problem, ProblemSolvingOptions options, CancellationToken cancellationToken)
    {
        if (problem == null)
        {
            throw new ArgumentNullException(nameof(problem));
        }

        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.AttendeeOptions == null || options.AttendeeOptions.Count() == 0)
        {
            throw new ArgumentException("At least one agent option must be provided.", nameof(options));
        }

        //LoggingSubscriber loggingSubscriber = new LoggingSubscriber(_loggerFactory);

        await using IRoomMonitor roomMonitor = _monitorProvider.GetMonitor();

        await using IRoomSubscriber subscriber = _subscriberProvider.Create(Guid.NewGuid().ToString());

        await using IAsyncDisposable subscription = await roomMonitor.SubscribeAsync(subscriber, cancellationToken);

        return await KickOffAndClose<T>(problem, roomMonitor, options, cancellationToken);
    }

    public async Task<AsyncOperation> SolveAsync<T>(ProblemDefinition problem, ProblemSolvingOptions options, CancellationToken cancellationToken)
    {
        if (problem == null)
        {
            throw new ArgumentNullException(nameof(problem));
        }

        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.AttendeeOptions == null || options.AttendeeOptions.Count() == 0)
        {
            throw new ArgumentException("At least one agent option must be provided.", nameof(options));
        }

        AsyncOperation asyncOperation = await _asyncOperationProvider.Create(Guid.NewGuid().ToString(), cancellationToken);

        _ = Task.Run(async () =>
        {

            using CancellationTokenSource cts = new CancellationTokenSource(AsyncSolveTimeout);

            //LoggingSubscriber loggingSubscriber = new LoggingSubscriber(_loggerFactory);

            await using IRoomMonitor monitor = _monitorProvider.GetMonitor();

            await using IRoomSubscriber subscriber = _subscriberProvider.Create(asyncOperation.MonitorId);

            await using IAsyncDisposable subscription = await monitor.SubscribeAsync(subscriber, cts.Token);

            try
            {
                Deliverable<T> deliverable = await KickOffAndClose<T>(problem, monitor, options, cts.Token);

                asyncOperation.Result = JsonSerializer.Serialize(deliverable);

                asyncOperation.Status = "Completed";

                await _asyncOperationProvider.Update(asyncOperation, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Asynchronous problem solving failed.");

                asyncOperation.Status = "Failed";
                asyncOperation.Result = null;

                await _asyncOperationProvider.Update(asyncOperation, CancellationToken.None);
            }
        });

        return asyncOperation;
    }

    private async Task<Deliverable<T>> KickOffAndClose<T>(ProblemDefinition problem, IRoomMonitor roomMonitor, ProblemSolvingOptions options, CancellationToken cancellationToken)
    {
        // Get a room 
        IRoom room = _roomProvider.GetRoom(
            new RoomCreationOptions
            {
                Capacity = options.AttendeeOptions.Count()
            });

        // Get attendees
        IEnumerable<Attendee> attendees = await _attendeeProvider.GetAttendees(options.AttendeeOptions);

        // Wire up the monitor
        room.Monitor = roomMonitor;

        try
        {
            // Kick off the discussion
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            cts.CancelAfter(options.TimeLimit);
        
            // Setup the room
            await room.Setup(problem, attendees, cancellationToken);

            try
            {
                await room.KickOff(cts.Token);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                // Time limit reached, proceed to closing the room
            }

            // Close the room and get the deliverable
            (Deliverable<T> deliverable, MeetingNotes notes) = await room.Close<T>(cancellationToken);

            if (_meetingNotesArchive != null)
            {
                await _meetingNotesArchive.ArchiveAsync(notes, cancellationToken);
            }

            return deliverable;
        }
        finally
        {
            // Dispose the room (returns it to the pool)
            await room.DisposeAsync();
        }
    }
}