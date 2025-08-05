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

            var contacts = await natsumeContactService.GetAllNatsumeContactsAsync(
                cancellationToken: CancellationToken
            );

            foreach (var contact in contacts)
            {
                CancellationToken.ThrowIfCancellationRequested();

                if (contact.IsFriend is false) continue;
                if (contact.CurrentFavor < contact.MaximumFavor)
                {
                    contact.BondUp();
                }
            }

            await natsumeContactService.UpdateNatsumeContactsAsync(
                contacts: contacts,
                cancellationToken: CancellationToken
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}