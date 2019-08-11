﻿using FortniteDownloader.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace FortniteDownloader
{
    public partial class Downloader : IDownloader
    {
        readonly string ManifestId;
        readonly Client Client;
        
        internal const string DOWNLOAD_BASE_URL = "http://epicgames-download1.akamaized.net/Builds/Fortnite/CloudDir/";

        public Downloader(string manifestId, Client client = null)
        {
            ManifestId = manifestId;
            Client = client ?? new Client();
        }

        public string ManifestUrl => $"{DOWNLOAD_BASE_URL}{ManifestId}.manifest";

        private Manifest downloadManifest;
        public async Task<Manifest> GetDownloadManifest(bool forceUpdate = false) =>
            downloadManifest != null && !forceUpdate ? downloadManifest :
            (downloadManifest = new Manifest(JsonConvert.DeserializeObject<WebManifest>(
                await (await Client.SendAsync("GET", ManifestUrl).ConfigureAwait(false)).GetStringAsync().ConfigureAwait(false)
            )));
        
        public async Task<DownloadStream> OpenFile(string file, bool cachePreviousChunks = false) =>
            new DownloadStream(file, await GetDownloadManifest().ConfigureAwait(false), cachePreviousChunks, Client);

        bool disposed;
        public void Dispose()
        {
            if (disposed) return;
            downloadManifest = null;
            Client.Dispose();
            disposed = true;
        }
    }
}
