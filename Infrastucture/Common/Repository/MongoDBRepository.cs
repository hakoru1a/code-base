using Contracts.Common.Interface;
using Contracts.Domain;
using MongoDB.Driver;
using Shared.Attributes;
using Shared.Configurations.Database;


namespace Infrastucture.Common.Repository
{
    public class MongoDBRepository<T> : IMongoDBRepository<T> where T : MongoEntity
    {
        private IMongoDatabase _database { get; set; }

        public MongoDBRepository(IMongoClient mongoClient, MongoSettings settings)
        {
            _database = mongoClient.GetDatabase(settings.DatabaseName);
        }

        public string GetCollectionName()
        {
            return typeof(T).GetCustomAttributes(typeof(BsonCollectionAttribute), true).FirstOrDefault() is BsonCollectionAttribute collectionNameAttribute
                ? collectionNameAttribute.CollectionName
                : typeof(T).Name;
        }

        protected virtual IMongoCollection<T> Collection => _database.GetCollection<T>(GetCollectionName());

        public Task CreateAsync(T entity)
            => _database.GetCollection<T>(GetCollectionName()).InsertOneAsync(entity);

        public Task DeleteAsync(string id)
           => _database.GetCollection<T>(GetCollectionName()).DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
        public IMongoCollection<T> FindAll(ReadPreference? readPreference = null)
        {
            var settings = new MongoCollectionSettings
            {
                ReadPreference = readPreference ?? ReadPreference.Primary
            };
            return _database.GetCollection<T>(GetCollectionName(), settings);
        }
        public async Task UpdateAsync(T entity)
        {
            var filter = Builders<T>.Filter.Eq(doc => doc.Id, entity.Id);
            var result = await _database.GetCollection<T>(GetCollectionName())
                .ReplaceOneAsync(filter, entity);
        }
    }
}
