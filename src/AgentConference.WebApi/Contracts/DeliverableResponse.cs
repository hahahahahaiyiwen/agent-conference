using System.Collections.Generic;

namespace AgentConference.WebApi.Contracts;

public class DeliverableResponse
{
    public string Id { get; set; }

    public IEnumerable<string> Items { get; set; }

    public IList<MetadataDto> Metadatas { get; set; }
}
