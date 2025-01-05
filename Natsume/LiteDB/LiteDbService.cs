using LiteDB;

namespace Natsume.LiteDB;

public class LiteDbService(string connectionString)
{
    private readonly LiteDatabase _db = new(connectionString);

    private ILiteCollection<NatsumeContact> NatsumeContacts => 
        _db.GetCollection<NatsumeContact>("natsume_contacts");
    
    public List<NatsumeContact> GetAllNatsumeContacts()
    {
        return NatsumeContacts.FindAll().ToList();
    }

    public NatsumeContact? AddNatsumeContact(NatsumeContact contact)
    {
        NatsumeContacts.Insert(contact);
        return GetNatsumeContactById(contact.Id);
    }

    public NatsumeContact? GetNatsumeContactById(ulong id)
    {
        return NatsumeContacts.FindOne(x => x.Id == id);
    }

    public bool UpdateNatsumeContact(NatsumeContact contact)
    {
        return NatsumeContacts.Update(contact);
    }
    
}