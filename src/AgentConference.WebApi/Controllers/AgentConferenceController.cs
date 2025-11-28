using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentConference.Primitives;
using AgentConference.Service;
using AgentConference.Service.Async;
using AgentConference.WebApi.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AgentConference.WebApi;

[ApiController]
[Route("api/[controller]")]
public class AgentConferenceController : ControllerBase
{
	private readonly IConferenceService _conferenceService;
	private readonly ILogger<AgentConferenceController> _logger;

	public AgentConferenceController(IConferenceService conferenceService, ILogger<AgentConferenceController> logger)
	{
		_conferenceService = conferenceService ?? throw new ArgumentNullException(nameof(conferenceService));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	[HttpPost("solve")]
	[ProducesResponseType(typeof(DeliverableResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status408RequestTimeout)]
	public async Task<ActionResult<DeliverableResponse>> Solve([FromBody] SolveProblemRequest request, CancellationToken cancellationToken)
	{
		if (request == null)
		{
			return BadRequest("Request payload cannot be null.");
		}

		ProblemDefinition problem = request.Problem?.ToDomain();

		if (problem == null || string.IsNullOrWhiteSpace(problem.Problem))
		{
			return BadRequest("Problem definition requires a non-empty statement.");
		}

		ProblemSolvingOptions options = request.Options?.ToDomain();

		if (options.AttendeeOptions.Count() == 0)
		{
			return BadRequest("NumberOfAttendees must be greater than zero.");
		}

		if (options.TimeLimit >= TimeSpan.FromMinutes(5))
        {
            return BadRequest("Time limit cannot be greater than 5 mins.");
        }

		try
		{
			Deliverable<GeneralResult> deliverable = await _conferenceService.Solve<GeneralResult>(problem, options, cancellationToken);

			DeliverableResponse response = deliverable.ToResponse();

			if (response == null)
			{
				return NoContent();
			}

			return Ok(response);
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning("Conference solve request was canceled.");
			return StatusCode(StatusCodes.Status408RequestTimeout);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while solving the problem.");
			return Problem(title: "Conference processing error", detail: ex.Message);
		}
	}

	[HttpPost("solve/async")]
	[ProducesResponseType(typeof(AsyncOperationResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status408RequestTimeout)]
	public async Task<ActionResult<AsyncOperationResponse>> SolveAsync([FromBody] SolveProblemRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
		{
			return BadRequest("Request payload cannot be null.");
		}

		ProblemDefinition problem = request.Problem?.ToDomain();

		if (problem == null || string.IsNullOrWhiteSpace(problem.Problem))
		{
			return BadRequest("Problem definition is invalid.");
		}

		ProblemSolvingOptions options = request.Options?.ToDomain();

		if (options.AttendeeOptions.Count() == 0)
		{
			return BadRequest("NumberOfAttendees must be greater than zero.");
		}

		if (options.TimeLimit >= TimeSpan.FromMinutes(5))
        {
            return BadRequest("Time limit cannot be greater than 5 mins.");
        }

        try
        {
            AsyncOperation asyncOperation = await _conferenceService.SolveAsync<GeneralResult>(problem, options, cancellationToken);

			AsyncOperationResponse response = asyncOperation.ToResponse();

			if (response == null)
			{
				return NoContent();
			}

			return Accepted(response);
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning("Asynchronous conference solve request was canceled.");
			return StatusCode(StatusCodes.Status408RequestTimeout);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while processing asynchronous solve request.");
			return Problem(title: "Asynchronous conference processing error", detail: ex.Message);
        }
    }

	[HttpPost("evaluate/async")]
	[ProducesResponseType(typeof(AsyncOperationResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status408RequestTimeout)]
	public async Task<ActionResult<AsyncOperationResponse>> EvaluateAsync([FromBody] EvaluationRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
		{
			return BadRequest("Request payload cannot be null.");
		}

		EvaluationProblem problem = request?.Problem.ToDomain();

		if (problem == null)
		{
			return BadRequest("Evaluation problem is invalid.");
		}

		ProblemSolvingOptions options = request.Options?.ToDomain();

		if (options.AttendeeOptions.Count() == 0)
		{
			return BadRequest("NumberOfAttendees must be greater than zero.");
		}

        try
        {
            AsyncOperation asyncOperation = await _conferenceService.SolveAsync<EvaluationResult>(problem, options, cancellationToken);

			AsyncOperationResponse response = asyncOperation.ToResponse();

			if (response == null)
			{
				return NoContent();
			}

			return Accepted(response);
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning("Asynchronous conference solve request was canceled.");
			return StatusCode(StatusCodes.Status408RequestTimeout);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while processing asynchronous solve request.");
			return Problem(title: "Asynchronous conference processing error", detail: ex.Message);
        }
    }
	
}