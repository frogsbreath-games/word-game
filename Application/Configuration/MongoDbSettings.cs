using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordGame.API.Application.Configuration
{
	public class MongoDbSettings
	{
		public string ConnectionString { get; set; } = string.Empty;
		public string GameDatabaseName { get; set; } = string.Empty;
	}
}
