using LiteDB;

namespace Natsume.LiteDB;

public class LiteDbService(string connectionString)
{
    private readonly LiteDatabase _db = new(connectionString);

    private ILiteCollection<Subscriber> Subscribers => _db.GetCollection<Subscriber>("subscribers");
    
    
    public List<Subscriber> GetSubscribers()
    {
        return Subscribers.FindAll().ToList();
    }

    public Subscriber? AddSubscriber(Subscriber subscriber)
    {
        Subscribers.Insert(subscriber);
        return GetSubscriberById(subscriber.Id);
    }

    public Subscriber? GetSubscriberById(ulong id)
    {
        return Subscribers.FindOne(x => x.Id == id);
    }

    public bool UpdateSubscriber(Subscriber subscriber)
    {
        return Subscribers.Update(subscriber);
    }
}