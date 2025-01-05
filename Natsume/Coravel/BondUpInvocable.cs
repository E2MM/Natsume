using Coravel.Invocable;
using Natsume.LiteDB;

namespace Natsume.Coravel;

public class BondUpInvocable(LiteDbService liteDbService) : IInvocable
{
    public Task Invoke()
    {
        var contacts = liteDbService.GetAllNatsumeContacts();

        foreach (var contact in contacts)
        {
            if (contact is not { IsNatsumeFriend: true }) continue;
            if (contact.CurrentFriendship < contact.MaximumFriendship)
            {
                liteDbService.UpdateNatsumeContact(contact.BondUp());
            }
        }

        return Task.CompletedTask;
    }
}