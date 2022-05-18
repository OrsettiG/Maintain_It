using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Helpers
{
    internal class Logging
    {

        private void TraceWithMessage(
    string message,
    [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
    [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
    [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
    )
        {
            System.Diagnostics.Trace.WriteLine( $"Message: {message}" );
            System.Diagnostics.Trace.WriteLine( $"Caller Member Name: {memberName}" );
            System.Diagnostics.Trace.WriteLine( $"Source File Path: {sourceFilePath}" );
            System.Diagnostics.Trace.WriteLine( $"Source Line Number: {sourceLineNumber}" );
        }
    }
}
