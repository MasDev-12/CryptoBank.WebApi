using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBank.WebApi.Integrations.Tests.Helpers;

public static class CancellationTokenHelper
{
    public static CancellationToken GetCancellationToken()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(90));

        return cts.Token;
    }
}
