namespace Fenix.ClientOperations
{
    internal class InspectionResult
    {
        public readonly InspectionDecision Decision;
        public readonly string Description;

        public InspectionResult(InspectionDecision decision, string description)
        {
            Decision = decision;
            Description = description;
        }
    }
}