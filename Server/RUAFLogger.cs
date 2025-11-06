using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;

namespace RUAFComeHomeServer;

[Injectable(InjectionType.Singleton)]
public class RUAFLogger
{
    private readonly bool _enableLogs;
    private readonly ISptLogger<RUAFLogger> _logger;
    public RUAFLogger(
        ISptLogger<RUAFLogger> logger,
        ConfigController configController)
    {
        _enableLogs = configController.ModConfig.debug.logs;
        _logger = logger;
    }

    public void Info(string message)
    {
        if (_enableLogs)
        {
            _logger.Info($"[RUAF Mod] {message}");
        }
    }
    public void Warn(string message)
    {
        _logger.Warning($"[RUAF Mod] WARNING: {message}");
    }
    public void Error(string message)
    {
        _logger.Error($"[RUAF Mod] ERROR: {message}");
    }
}