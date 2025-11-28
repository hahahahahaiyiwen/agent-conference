using System.Collections.Generic;

namespace AIJudge.Service;

public class JudgeReport
{
    public IEnumerable<Evaluation> Evaluations { get; set; }

    public IEnumerable<Metric> Metrics { get; set; }
}