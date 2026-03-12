using HomeHoney.Api.Infrastructure.FileBrowser;
using HomeHoney.Api.Infrastructure.Mongo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

namespace HomeHoney.Api.Tests.Integration;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Development");
		builder.ConfigureServices(services =>
		{
			services.RemoveAll<IFileBrowserGateway>();
			services.RemoveAll<IMongoDatabaseFactory>();
			services.AddSingleton<IFileBrowserGateway, FakeFileBrowserGateway>();
			services.AddSingleton<IMongoDatabaseFactory, FakeMongoDatabaseFactory>();
		});
	}

	private sealed class FakeFileBrowserGateway : IFileBrowserGateway
	{
		public Task<FileBrowserValidationResult> ValidateConnectionAsync(string baseUrl, string apiPath, CancellationToken cancellationToken = default)
			=> Task.FromResult(TestServiceOverrides.FileBrowserValidationResult);

		public Task<FileBrowserUploadResult> UploadFileAsync(string baseUrl, string apiPath, string folderPath, string fileName, Stream content, string? contentType, CancellationToken cancellationToken = default)
			=> Task.FromResult(TestServiceOverrides.FileBrowserUploadResult);

		public Task<FileBrowserDownloadResult> DownloadFileAsync(string baseUrl, string apiPath, string remotePath, CancellationToken cancellationToken = default)
			=> Task.FromResult(TestServiceOverrides.FileBrowserDownloadResult);

		public Task<FileBrowserDeleteResult> DeleteFileAsync(string baseUrl, string apiPath, string remotePath, CancellationToken cancellationToken = default)
			=> Task.FromResult(TestServiceOverrides.FileBrowserDeleteResult);
	}

	private sealed class FakeMongoDatabaseFactory : IMongoDatabaseFactory
	{
		public Task<IMongoDatabase?> GetDatabaseAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IMongoDatabase?>(null);

		public Task<MongoValidationResult> ValidateAsync(string connectionString, string databaseName, CancellationToken cancellationToken = default)
			=> Task.FromResult(TestServiceOverrides.MongoValidationResult);
	}
}
