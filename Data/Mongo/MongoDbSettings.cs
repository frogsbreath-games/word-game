﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordGame.API.Data.Mongo
{
	public class MongoDbSettings
	{
		public string ConnectionString { get; set; }
		public string DatabaseName { get; set; }
	}
}
