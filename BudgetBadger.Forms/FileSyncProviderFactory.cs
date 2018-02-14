using System;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Sync;
using BudgetBadger.FileSyncProvider.Dropbox;
using BudgetBadger.Forms.Enums;

namespace BudgetBadger.Forms
{
    public static class FileSyncProviderFactory
    {
        public static IFileSyncProvider CreateFileSyncProvider(ISettings settings)
        {
            if (settings.GetValueOrDefault(SettingsKeys.SyncMode) == SyncMode.DropboxSync)
            {
                return new DropboxFileSyncProvider(settings.GetValueOrDefault(SettingsKeys.DropboxAccessToken));
            }

            return new NoFileSyncProvider();
        }
    }
}
