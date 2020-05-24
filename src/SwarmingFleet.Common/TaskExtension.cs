

namespace SwarmingFleet.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class TaskExtension
    {

        public static Task<TResult> WithTask<TResult>(this TResult result)
        {
            return Task.FromResult(result);
        }
    }
}
