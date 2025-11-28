using System.Collections.Generic;
using System.Text;

namespace AgentConference.Primitives;

public class EvaluationProblem : ProblemDefinition
{
    private const string EvalProblem = "Evaluate the given response in terms of how good the response answers the query based on the provided ground truth and evaluation criteria.";

    public EvaluationProblem(string groundTruth, string query, string response, string criteria) 
        : base(EvalProblem, ToContext(groundTruth, query, response, criteria))
    {
    }

    public static string ToContext(string groundTruth, string query, string response, string criteria)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(groundTruth))
        {
            sb.Append($"\nGround Truth: {groundTruth}");
        }

        if (!string.IsNullOrEmpty(query))
        {
            sb.Append($"\nQuery: {query}");
        }

        if (!string.IsNullOrEmpty(response))
        {
            sb.Append($"\nResponse to Evaluate: {response}");
        }

        if (!string.IsNullOrEmpty(criteria))
        {
            sb.Append($"\nEvaluation Criteria: {criteria}");
        }

        return sb.ToString();
    }
}