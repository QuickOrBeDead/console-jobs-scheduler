using System.Transactions;
using ConsoleJobScheduler.Core.Domain.Settings;
using ConsoleJobScheduler.Core.Domain.Settings.Model;

namespace ConsoleJobScheduler.Core.Application;

public interface ISettingsApplicationService
{
    Task<TSettings> GetSettings<TSettings>()
        where TSettings : ISettings, new();

    Task SaveSettings<TSettings>(TSettings settings)
        where TSettings : ISettings;
}

public sealed class SettingsApplicationService : ISettingsApplicationService
{
    private readonly ISettingsService _settingsService;

    public SettingsApplicationService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<TSettings> GetSettings<TSettings>()
        where TSettings : ISettings, new()
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }, TransactionScopeAsyncFlowOption.Enabled);

        var result = await _settingsService.GetSettings<TSettings>().ConfigureAwait(false);

        transactionScope.Complete();

        return result;
    }

    public async Task SaveSettings<TSettings>(TSettings settings)
        where TSettings : ISettings
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

        await _settingsService.SaveSettings(settings).ConfigureAwait(false);

        transactionScope.Complete();
    }
}