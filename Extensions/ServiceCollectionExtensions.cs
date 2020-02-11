using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Application.Configuration;

namespace WordGame.API.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddMongo(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)));

			services.AddScoped<IMongoClient, MongoClient>(provider =>
				new MongoClient(provider.GetRequiredService<IOptions<MongoDbSettings>>().Value.ConnectionString));

			var camelCase = new ConventionPack { new CamelCaseElementNameConvention() };
			var enumString = new ConventionPack { new EnumRepresentationConvention(BsonType.String) };

			ConventionRegistry.Register(nameof(camelCase), camelCase, x => true);
			ConventionRegistry.Register(nameof(enumString), enumString, x => true);

			return services;
		}
	}
}
