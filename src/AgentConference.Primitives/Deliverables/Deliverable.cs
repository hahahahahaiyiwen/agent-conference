using System;
using System.Collections.Generic;

namespace AgentConference.Primitives;

public class Deliverable<T>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public List<T> Items { get; set; }

    public IEnumerable<Metadata> Metadata { get; set; }
}