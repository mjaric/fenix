namespace Fenix.ClientOperations
{
    internal enum InspectionDecision
    {
        DoNothing,
        EndOperation,
        Retry,
        Reconnect,
        Subscribed,
        CloseChannel
    }
}