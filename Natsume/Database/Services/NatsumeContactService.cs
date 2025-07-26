using Microsoft.EntityFrameworkCore;
using Natsume.Database.Entities;

namespace Natsume.Database.Services;

public class NatsumeContactService(NatsumeDbContext context)
{
    public Task<List<NatsumeContact>> GetAllNatsumeContactsAsync(
        CancellationToken cancellationToken = default
    )
    {
        return context.Contacts.ToListAsync(cancellationToken);
    }

    public Task<int> UpdateNatsumeContactsAsync(
        CancellationToken cancellationToken = default,
        params IEnumerable<NatsumeContact> contacts
    )
    {
        context.Contacts.UpdateRange(contacts);
        return context.SaveChangesAsync(cancellationToken);
    }

    public async Task<NatsumeContact> AddNatsumeContactAsync(
        ulong discordId,
        string discordNickname,
        bool isFriend = true,
        CancellationToken cancellationToken = default
    )
    {
        var newContact = new NatsumeContact(
            discordId: discordId,
            discordNickname: discordNickname,
            isFriend: isFriend
        );

        if (await GetNatsumeContactByIdAsync(newContact.DiscordId, cancellationToken) is not null)
        {
            throw new Exception("Contact already exists");
        }

        context.Contacts.Add(newContact);
        _ = context.SaveChangesAsync(cancellationToken);
        return newContact;
    }

    public Task<NatsumeContact?> GetNatsumeContactByIdAsync(
        ulong discordId,
        CancellationToken cancellationToken = default
    )
    {
        return context.Contacts.FirstOrDefaultAsync(x => x.DiscordId == discordId, cancellationToken);
    }
}