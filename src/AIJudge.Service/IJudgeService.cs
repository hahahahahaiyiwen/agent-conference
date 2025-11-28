using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIJudge.Service;

public interface IJudgeService
{
    Task<JudgeReport> Judge(Problem problem, IEnumerable<Submission> submissions, Expectation expectation, CancellationToken cancellationToken);
}
