using MongoDB.Bson;
using MongoDB.Driver;
using MVM.ProcessEngine.Common.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.EnergySuite.Repositories
{
    public class MongoRepository
    {

        private IMongoDatabase _database = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public MongoRepository(string tenant)
        {
            string mongoConnectionString = GestorCalculosHelper.GetMetadataValue(tenant,"MVMCoMongoConnString", true).ToString();
            string mongoDataBase = GestorCalculosHelper.GetMetadataValue(tenant,"MVMCoMongoNameDB", true).ToString();

            var client = new MongoClient(mongoConnectionString);
            if (client != null)
            {
                _database = client.GetDatabase(mongoDataBase);

            }
        }


        /// <summary>
        /// Get Any Collection from mongoDB
        /// </summary>
        /// <param name="collectionName">Name of collection</param>
        /// <returns>List of Bson Document</returns>
        public IEnumerable<BsonDocument> GetAllCollection(string collectionName)
        {

            List<BsonDocument> result = new List<BsonDocument>();
            var collection = _database.GetCollection<BsonDocument>(collectionName);

            try
            {
                var filter = new BsonDocument();
                result = collection.Find(filter).ToList();
                return result;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Get Item(s) from Collection (filter)
        /// </summary>
        /// <param name="collectionName">Name of collection</param>
        /// <param name="fieldName">Name of field of collection</param>
        /// <param name="fieldValue">Value of field</param>
        /// <returns>Bson Document</returns>
        public BsonDocument GetItemCollectionById(string collectionName, string fieldName, int fieldValue )
        {

            BsonDocument result = new BsonDocument();
            var collection = _database.GetCollection<BsonDocument>(collectionName);

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq(fieldName, fieldValue);
                result = collection.Find(filter).ToList().FirstOrDefault();
                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }
    }
}
