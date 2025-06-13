using Natsume.Database;
using Natsume.Database.Entities;

namespace Natsume.Services;

public class NatsumeDbService(NatsumeDbContext context)
{
    public List<NatsumeContact> GetAllNatsumeContacts()
    {
        return context.Contacts.ToList();
    }

    public NatsumeContact? AddNatsumeContact(NatsumeContact contact)
    {
        context.Contacts.Add(contact);
        context.SaveChanges();
        return GetNatsumeContactById(contact.DiscordId);
    }

    public NatsumeContact? GetNatsumeContactById(ulong discordId)
    {
        return context.Contacts.FirstOrDefault(x => x.DiscordId == discordId);
    }

    public bool UpdateNatsumeContact(NatsumeContact contact)
    {
        context.Contacts.Update(contact);
        return context.SaveChanges() > 0;
    }
}