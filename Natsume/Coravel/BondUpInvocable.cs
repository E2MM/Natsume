using Coravel.Invocable;
using Natsume.Services;

namespace Natsume.Coravel;

public class BondUpInvocable(NatsumeDbService natsumeDbService) : IInvocable
{
    public Task Invoke()
    {
        var contacts = natsumeDbService.GetAllNatsumeContacts();

        foreach (var contact in contacts)
        {
            if (contact is not { IsFriend: true }) continue;
            if (contact.AvailableFavor < contact.Friendship)
            {
                natsumeDbService.UpdateNatsumeContact(contact.BondUp());
            }
        }

        return Task.CompletedTask;
    }
}