using System.Collections.Generic;

namespace AIJudge.Service;

public class Expectation
{
    public string EvaluationRubric { get; set; }

    public IEnumerable<ExpectedDeliverable> ExpectedDeliverables { get; set; }
}
