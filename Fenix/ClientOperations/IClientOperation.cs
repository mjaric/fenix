using System;

namespace Fenix.ClientOperations
{
    internal interface IClientOperation
    {
        string CreateNetworkPackage(long @ref);
        
        void Fail(Exception exception);
    }
}