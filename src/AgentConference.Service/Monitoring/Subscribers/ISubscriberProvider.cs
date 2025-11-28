namespace AgentConference.Service.Monitoring;

public interface ISubscriberProvider
{
    IRoomSubscriber Get(string Id);

    IRoomSubscriber Create(string Id);
}