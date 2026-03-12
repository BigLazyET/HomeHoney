namespace HomeHoney.Api.Tests.Integration;

public static class TestServiceOverrides
{
	public static HomeHoney.Api.Infrastructure.FileBrowser.FileBrowserValidationResult FileBrowserValidationResult { get; set; } = new(true, "ok");
	public static HomeHoney.Api.Infrastructure.FileBrowser.FileBrowserUploadResult FileBrowserUploadResult { get; set; } = new(true, "ok", new HomeHoney.Api.Infrastructure.FileBrowser.FileBrowserFileDescriptor
	{
		Path = "test/path/file.pdf",
		Name = "file.pdf",
		ContentType = "application/pdf",
		SizeBytes = 128,
	});
	public static HomeHoney.Api.Infrastructure.FileBrowser.FileBrowserDownloadResult FileBrowserDownloadResult { get; set; } = new(true, "ok", [1, 2, 3], "file.pdf", "application/pdf", "test/path/file.pdf");
	public static HomeHoney.Api.Infrastructure.FileBrowser.FileBrowserDeleteResult FileBrowserDeleteResult { get; set; } = new(true, "ok");

	public static HomeHoney.Api.Infrastructure.Mongo.MongoValidationResult MongoValidationResult { get; set; } = new(true, "ok");

	public static void Reset()
	{
		FileBrowserValidationResult = new(true, "ok");
		FileBrowserUploadResult = new(true, "ok", new HomeHoney.Api.Infrastructure.FileBrowser.FileBrowserFileDescriptor
		{
			Path = "test/path/file.pdf",
			Name = "file.pdf",
			ContentType = "application/pdf",
			SizeBytes = 128,
		});
		FileBrowserDownloadResult = new(true, "ok", [1, 2, 3], "file.pdf", "application/pdf", "test/path/file.pdf");
		FileBrowserDeleteResult = new(true, "ok");
		MongoValidationResult = new(true, "ok");
	}
}
