using System;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Sync;
using Dropbox.Api;

namespace BudgetBadger.FileSyncProvider.Dropbox
{
    public static class DropboxFileSyncProviderFactory
    {
        public static DropboxFileSyncProvider CreateFileSyncProvider(ISettings settings)
        {
            var dropboxAccessKey = settings.GetValueOrDefault("");
            return new DropboxFileSyncProvider(dropboxAccessKey);
        }
    }
}
