using System;
using System.Collections.Generic;

namespace Voidwell.Internal.Models
{
    public class BatchUserRequest
    {
        public BatchUserRequest(IEnumerable<Guid> userIds)
        {
            UserIds = userIds;
        }

        public IEnumerable<Guid> UserIds { get; set; }
    }
}
