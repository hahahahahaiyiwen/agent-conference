using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AgentConference.Service.Attendees;

public class AgentAttendeeProvider : IAttendeeProvider
{
    private readonly AgentAttendeeProviderOptions _options;
    private readonly AzureOpenAIClient _client;
    private readonly ILoggerFactory _loggerFactory;

    public AgentAttendeeProvider(IOptions<AgentAttendeeProviderOptions> options, ILoggerFactory loggerFactory)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _client = new AzureOpenAIClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.ApiKey));
        }
        else
        {
            _client = new AzureOpenAIClient(new Uri(_options.Endpoint), new DefaultAzureCredential());
        }
    }

    public async Task<IEnumerable<Attendee>> GetAttendees(IEnumerable<AttendeeCreationOptions> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        List<Attendee> attendees = new List<Attendee>();

        foreach (var option in options)
        {
            attendees.Add(
                await GetAttendee(option));
        }

        return attendees;
    }

    private Task<Attendee> GetAttendee(AttendeeCreationOptions option)
    {
        Debug.Assert(option != null, "Options cannot be null.");

        if (string.IsNullOrWhiteSpace(option.Name))
        {
            option.Name = _options.DefaultAttendeeNames[Random.Shared.Next(0, _options.DefaultAttendeeNames.Count)];
        }

        if (string.IsNullOrWhiteSpace(option.Instruction))
        {
            option.Instruction = _options.DefaultInstruction;
        }

        if (string.IsNullOrWhiteSpace(option.Model))
        {
            option.Model = _options.DeploymentNames.ElementAt(Random.Shared.Next(0, _options.DeploymentNames.Count()));
        }

        return Task.FromResult<Attendee>(
            new AgentAttendee(
                option.Name, 
                option.Model,
                _client
                    .GetChatClient(option.Model)
                    .CreateAIAgent(
                        new ChatClientAgentOptions(
                            name: option.Name,
                            instructions: $"You are agent {option.Name}. "+ option.Instruction)),
                _loggerFactory.CreateLogger<AgentAttendee>()));
    }
}