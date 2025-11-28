using System;
using System.Collections;
using System.Collections.Generic;

namespace AIJudge.Service;

public class Submission
{
    public string SubmissionId { get; set; }

    public bool IsSuccessful { get; set; }

    public string Content { get; set; }

    public TimeSpan TimeToComplete { get; set; }

    public IEnumerable<Metric> Metrics { get; set; }
}