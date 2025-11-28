using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIJudge.Service;

internal class MultiAgentJudgeService : IJudgeService
{
    public Task<JudgeReport> Judge(Problem problem, IEnumerable<Submission> submissions, Expectation expectation, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}