using System.IO;

#if ANDROID || IOS || MACCATALYST || WINDOWS
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
#endif

namespace HomeHoney.Services.Storage;

public interface IAttachmentFileSaver
{
    Task<AttachmentFileSaveResult> SaveAsync(byte[] content, string fileName, CancellationToken cancellationToken = default);
}

public sealed record AttachmentFileSaveResult(
    bool IsSuccess,
    string Message,
    bool WasCancelled = false,
    string? FilePath = null);

public sealed class MauiAttachmentFileSaver : IAttachmentFileSaver
{
    public async Task<AttachmentFileSaveResult> SaveAsync(byte[] content, string fileName, CancellationToken cancellationToken = default)
    {
        if (content.Length == 0)
        {
            return new AttachmentFileSaveResult(false, "附件内容为空，无法保存。");
        }

#if ANDROID || IOS || MACCATALYST || WINDOWS
        try
        {
            var folderResult = MainThread.IsMainThread
                ? await FolderPicker.Default.PickAsync(cancellationToken)
                : await MainThread.InvokeOnMainThreadAsync(() => FolderPicker.Default.PickAsync(cancellationToken));

            if (!folderResult.IsSuccessful || folderResult.Folder is null || string.IsNullOrWhiteSpace(folderResult.Folder.Path))
            {
                return new AttachmentFileSaveResult(
                    false,
                    folderResult.Exception is null ? "已取消选择保存位置。" : $"选择保存位置失败：{folderResult.Exception.Message}",
                    folderResult.Exception is null);
            }

            await using var stream = new MemoryStream(content, writable: false);
            var saveResult = MainThread.IsMainThread
                ? await FileSaver.Default.SaveAsync(folderResult.Folder.Path, fileName, stream, cancellationToken)
                : await MainThread.InvokeOnMainThreadAsync(() => FileSaver.Default.SaveAsync(folderResult.Folder.Path, fileName, stream, cancellationToken));

            if (!saveResult.IsSuccessful || string.IsNullOrWhiteSpace(saveResult.FilePath))
            {
                return new AttachmentFileSaveResult(
                    false,
                    saveResult.Exception is null ? "已取消保存附件。" : $"保存附件失败：{saveResult.Exception.Message}",
                    saveResult.Exception is null,
                    saveResult.FilePath);
            }

            return new AttachmentFileSaveResult(true, $"附件已保存到：{saveResult.FilePath}", false, saveResult.FilePath);
        }
        catch (TaskCanceledException)
        {
            return new AttachmentFileSaveResult(false, "已取消保存附件。", true);
        }
        catch (OperationCanceledException)
        {
            return new AttachmentFileSaveResult(false, "已取消保存附件。", true);
        }
        catch (Exception ex)
        {
            return new AttachmentFileSaveResult(false, $"保存附件失败：{ex.Message}");
        }
#else
        await Task.CompletedTask;
    return new AttachmentFileSaveResult(false, "当前平台暂不支持本地附件保存，需要在 MAUI 客户端中操作。");
#endif
    }
}