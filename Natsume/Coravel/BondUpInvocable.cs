using Coravel.Invocable;
using Natsume.Database.Services;

namespace Natsume.Coravel;

internal class BondUpInvocable(NatsumeContactService natsumeContactService) : IInvocable
{
    public async Task Invoke()
    {
        try
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var contacts = await natsumeContactService.GetAllNatsumeContactsAsync(
                cancellationToken: cts.Token
            );

            foreach (var contact in contacts)
            {
                if (contact.IsFriend is false) continue;
                if (contact.CurrentFavor < contact.MaximumFavor)
                {
                    contact.BondUp();
                }
            }

            await natsumeContactService.UpdateNatsumeContactsAsync(
                contacts: contacts,
                cancellationToken: cts.Token
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}