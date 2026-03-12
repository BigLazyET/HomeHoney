window.homeHoneyDownloads = {
    saveFile(fileName, contentType, base64Content) {
        const link = document.createElement('a');
        link.href = `data:${contentType || 'application/octet-stream'};base64,${base64Content}`;
        link.download = fileName || 'download.bin';
        link.style.display = 'none';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
};