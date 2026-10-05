namespace Voidwell.Platform.Api.Models;

public class BatchUserRequest
{
    public BatchUserRequest(IEnumerable<Guid> userIds)
    {
        UserIds = userIds;
    }

    public IEnumerable<Guid> UserIds { get; set; }
}
