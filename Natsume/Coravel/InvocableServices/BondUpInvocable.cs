using Coravel.Invocable;
using Natsume.Persistence.Contact;

namespace Natsume.Coravel.InvocableServices;

public class BondUpInvocable(NatsumeContactService natsumeContactService) : IInvocable, ICancellableInvocable
{
    public CancellationToken CancellationToken { get; set; }

    public async Task Invoke()
    {
        try
        {
            CancellationToken.ThrowIfCancellationRequested();

            var friends = await natsumeContactService.GetAllNatsumeFriendsAsync(
                cancellationToken: CancellationToken
            );

            foreach (var friend in friends)
            {
                CancellationToken.ThrowIfCancellationRequested();

                if (friend.CurrentFavor < friend.MaximumFavor)
                {
                    friend.BondUp();
                }
            }

            await natsumeContactService.UpdateNatsumeContactsAsync(
                contacts: friends,
                cancellationToken: CancellationToken
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}