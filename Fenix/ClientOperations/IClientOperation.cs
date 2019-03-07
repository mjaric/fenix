using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fenix.ClientOperations
{
    internal interface IClientOperation
    {   
        /// <summary>
        /// Generates network package for operation
        /// </summary>
        /// <returns>string that will be sent to server</returns>
        Push CreateNetworkPackage(long pushRef);

        /// <summary>
        /// Inspects result from the server
        /// </summary>
        /// <param name="push">Push that is received from server as response to operation instance.</param>
        /// <returns>Inspection result <see cref="InspectionResult"/> that should tell what to do with the push.</returns>
        InspectionResult InspectPackage(Push push);

        /// <summary>
        /// Bubbles exception back to operation <see cref="TaskCompletionSource{TResult}"/>
        /// </summary>
        /// <param name="exception"></param>
        void Fail(Exception exception);
    }

}