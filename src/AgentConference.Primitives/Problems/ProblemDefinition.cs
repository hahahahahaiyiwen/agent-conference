using System;
using System.Collections.Generic;
using System.Text;

namespace AgentConference.Primitives;

public class ProblemDefinition
{
    public ProblemDefinition(string problem, string context = null)
    {
        Problem = problem;
        Context = context;
    }

    public string Context { get;}

    public string Problem { get; }
    
    public IEnumerable<Metadata> Metadata { get; set; }

    public virtual string ToProblemString()
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(Context))
        {
            sb.AppendLine("-------------Context:");
            sb.AppendLine($"{Context}");
        }
        
        sb.AppendLine("-------------Problem:");
        sb.AppendLine($"{Problem}");
        
        return sb.ToString();
    }
}