using System;
using MongoDB.Driver;
using TestApp.Entities;

namespace TestApp.Data
{
	public class DbContext
	{
		private readonly IMongoDatabase _database;

		public DbContext(IConfiguration config)
		{
			string connectionString = config.GetConnectionString("MongoDB");
            string dbName = config.GetConnectionString("DatabaseName");
			var client = new MongoClient(connectionString);
			_database = client.GetDatabase(dbName);
        }

		//public IMongoCollection<Review> Reviews => _database.GetCollection<Review>("reviews");
        public IMongoCollection<Movie> Movies => _database.GetCollection<Movie>("movies");
    }
}

