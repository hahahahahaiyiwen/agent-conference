namespace AgentConference.WebApi.Contracts;

public class AsyncOperationResponse
{
    public string OperationId { get; set; }

    public string MonitorId { get; set; }

    public string Status { get; set; }

    public string Result { get; set; }

    public string ErrorCode { get; set; }

    public string ErrorMessage { get; set; }
}