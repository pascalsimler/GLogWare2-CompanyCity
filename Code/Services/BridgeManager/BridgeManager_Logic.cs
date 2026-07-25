using Gudel.GLogWare.EFCore.Domain;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;

namespace Gudel.GLogWare.Services.BridgeManager;

public partial class BridgeManager
{
    #region Private members
    private Resource? _resource;
    private JobTypeIdentifiers _lastJobType;
    #endregion

    private async Task<bool> VerifyGeneralConditionsToStartOrder()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        bool rValue = false;
        while (true)
        {
            _resource = _db.Resources
                .Where(x => x.Name == OP)
                .FirstOrDefault();
            if (_resource == null)
            {
                _logger.LogError(
                    $"Resouce.Name=[{OP}] does not exist !");
                break;
            }

            if (!_resource.IsOnline)
            {
                _logger.LogInformation(
                    $"Communication with PLC of the bridge is currently offline !");
                break;
            }

            if (_resource.Mode != nameof(ResourceModeIdentifiers.AUTOMATIC))
            {
                _logger.LogInformation(
                    $"Bridge is in mode=[{_resource.Mode}]!=[{nameof(ResourceModeIdentifiers.AUTOMATIC)}]");
                break;
            }

            var j = _db.Jobs
                .Where(j =>
                    j.Bridge == OP &&
                    j.Status.StartsWith("GANTRY")
                 )
                .FirstOrDefault();
            if (j != null)
            {
                break;
            }

            rValue = true;
            break;
        }

        _logger.LogInformation($"rValue=[{rValue}]");
        _logger.LogInformation(LogMessages.LeaveMethod);
        return rValue;
    }

    private async Task<bool> TryToStartNewOrder()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        bool rValue = true;
        while (true)
        {
            if (!await VerifyGeneralConditionsToStartOrder())
            {
                rValue = false;
                break;
            }

            _logger.LogInformation($"_lastJobType=[{_lastJobType.ToString()}]");
            switch (_lastJobType)
            {
                case JobTypeIdentifiers.INFEED:
                    if (await TryToStartOutputOrder()) break;
                    if (await TryToStartRelocationOrder()) break;
                    if (await TryToStartInputOrder()) break;
                    rValue = false;
                    break;
                case JobTypeIdentifiers.OUTFEED:
                    if (await TryToStartInputOrder()) break;
                    if (await TryToStartRelocationOrder()) break;
                    if (await TryToStartOutputOrder()) break;
                    rValue = false;
                    break;
                case JobTypeIdentifiers.RELOCATION:
                    if (await TryToStartInputOrder()) break;
                    if (await TryToStartOutputOrder()) break;
                    if (await TryToStartRelocationOrder()) break;
                    rValue = false;
                    break;
                case JobTypeIdentifiers.PALLETIZING:
                    if (await TryToStartPalletizingOrder()) break;
                    rValue = false;
                    break;
                default:
                    rValue = false;
                    break;
            }

            break;
        }

        _logger.LogInformation($"rValue=[{rValue}]");
        _logger.LogInformation(LogMessages.LeaveMethod);
        return rValue;
    }

    private async Task<bool> VerifyConditionsToStartInputOrder()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        bool rValue = true;
        while (true)
        {
            if (!_resource!.InfeedEnabled)
            {
                _logger.LogInformation(
                    $"Infeeds are not enabled for that bridge !");
                rValue = false;
                break;
            }

            break;
        }

        _logger.LogInformation($"rValue=[{rValue}]");
        _logger.LogInformation(LogMessages.LeaveMethod);
        return rValue;
    }

    private async Task<bool> TryToStartInputOrder()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        bool rValue = true;
        while (true)
        {
            if (!await VerifyConditionsToStartInputOrder())
            {
                rValue = false;
                break;
            }

            _lastJobType = JobTypeIdentifiers.INFEED;

            break;
        }

        _logger.LogInformation($"rValue=[{rValue}]");
        _logger.LogInformation(LogMessages.LeaveMethod);
        return rValue;
    }

    private async Task<bool> VerifyConditionsToStartOutputOrder()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        bool rValue = true;
        while (true)
        {
            if (!_resource!.OutfeedEnabled)
            {
                _logger.LogInformation(
                    $"Outfeeds are not enabled for that bridge !");
                rValue = false;
                break;
            }

            break;
        }

        _logger.LogInformation($"rValue=[{rValue}]");
        _logger.LogInformation(LogMessages.LeaveMethod);
        return rValue;
    }

    private async Task<bool> TryToStartOutputOrder()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        bool rValue = true;
        while (true)
        {
            if (!await VerifyConditionsToStartOutputOrder())
            {
                rValue = false;
                break;
            }

            _lastJobType = JobTypeIdentifiers.OUTFEED;

            break;
        }

        _logger.LogInformation($"rValue=[{rValue}]");
        _logger.LogInformation(LogMessages.LeaveMethod);
        return rValue;
    }

    private async Task<bool> VerifyConditionsToStartRelocationOrder()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        bool rValue = true;
        while (true)
        {
            if (!_resource!.RelocationEnabled)
            {
                _logger.LogInformation(
                    $"Relocations are not enabled for that bridge !");
                rValue = false;
                break;
            }

            break;
        }

        _logger.LogInformation($"rValue=[{rValue}]");
        _logger.LogInformation(LogMessages.LeaveMethod);
        return rValue;
    }

    private async Task<bool> TryToStartRelocationOrder()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        if (!await VerifyConditionsToStartRelocationOrder()) return false;

        _lastJobType = JobTypeIdentifiers.RELOCATION;
        return true;
    }

    private async Task<bool> VerifyConditionsToStartPalletizingOrder()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        bool rValue = true;
        while (true)
        {
            break;
        }

        _logger.LogInformation($"rValue=[{rValue}]");
        _logger.LogInformation(LogMessages.LeaveMethod);
        return rValue;
    }

    private async Task<bool> TryToStartPalletizingOrder()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        if (!await VerifyConditionsToStartPalletizingOrder()) return false;

        _lastJobType = JobTypeIdentifiers.PALLETIZING;
        return true;
    }

    private async Task Process_STAT(STATBridge stat)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        string json = GLogWareMessage.Serialize<STATBridge>(stat);
        _logger.LogInformation($"stat=[\r\n{json}\r\n]");
        
        var r = _db.Resources.Where(x => x.Name == OP).FirstOrDefault();
        if (r == null) 
        {
            _logger.LogError($"Unknown resource [{OP}]");
            return;
        }

        r.Parked = stat.Parked;
        r.Mode = (stat.WorkingMode) switch 
        {
            STATBridgeWorkingModes.AUTOMATIC => nameof(ResourceModeIdentifiers.AUTOMATIC),
            STATBridgeWorkingModes.MANUAL => nameof(ResourceModeIdentifiers.MANUAL),
            STATBridgeWorkingModes.STOPPED => nameof(ResourceModeIdentifiers.STOPPED),
            _ => nameof(ResourceModeIdentifiers.UNDEFINED)
        };
        r.Occupied = stat.GripperOccupied;
        r.ErrorFlag = stat.ErrorFlag;
        await _db.SaveChangesAsync();

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task Process_COMP(COMP comp)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        string json = GLogWareMessage.Serialize<COMP>(comp);
        _logger.LogInformation($"comp=[\r\n{json}\r\n]");

        var q = _db.Jobs.Where(x =>
                    x.Bridge == OP &&
                    x.Status == JobStatusIdentifiers.BRIDGE_LOAD.ToString()
                );

        int count = 0;
        foreach (var j in q)
        {
            _logger.LogInformation($"jobs[{count}].jobId=[{j.Jobid}]");
            _logger.LogInformation($"jobs[{count}].Type=[{j.Type}]");
            _logger.LogInformation($"jobs[{count}].SourcePlace=[{j.SourcePlace}]");
            _logger.LogInformation($"jobs[{count}].DestinationPlace=[{j.DestinationPlace}]");
            count++;
        }

        if (count == 1)
        {
            var job = q.FirstOrDefault();
            if (comp.FeedbackCode == "0000")
            {
                job!.Status = JobStatusIdentifiers.BRIDGE_LOAD_END.ToString();
            }
            await _db.SaveChangesAsync();
        }
        else
        {
            _logger.LogError($"Found [{count}] jobs where an unique one is expected !");
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
    }
}