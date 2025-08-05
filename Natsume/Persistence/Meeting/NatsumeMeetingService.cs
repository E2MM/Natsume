using Microsoft.EntityFrameworkCore;

namespace Natsume.Persistence.Meeting;

public class NatsumeMeetingService(NatsumeDbContext context)
{
    public Task<int> CountMeetingsAsNoTrackingAsync(CancellationToken token = default)
    {
        return context
            .Meetings
            .AsNoTracking()
            .CountAsync(
                predicate: m => m.IsRandomMeeting,
                cancellationToken: token
            );
    }

    public Task<int> AddMeetingAsync(NatsumeMeeting meeting, CancellationToken token = default)
    {
        context.Meetings.Add(entity: meeting);
        return context.SaveChangesAsync(cancellationToken: token);
    }
}