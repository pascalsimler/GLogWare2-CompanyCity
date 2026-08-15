using Gudel.GLogWare.Entities;
using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;

namespace Gudel.GLogWare.Services.BridgeManager;

public partial class BridgeManager
{
    #region Private members
    private Resource? _resource;
    private JobTypeIdentifiers _lastJobType;
    #endregion

    #region Private methods

    #region Orders in general
    private async Task<bool> VerifyGeneralConditionsToStartOrder()
    {
        logger.EnterMethod();

        bool rValue = false;
        while (true)
        {
            _resource = _db.Resources
                .Where(x => x.Name == OP)
                .FirstOrDefault();
            if (_resource == null)
            {
                logger.LogError(
                    "Resource.Name=[{OP}] does not exist !", OP);
                break;
            }

            if (!_resource.IsOnline)
            {
                logger.LogInformation(
                    "Communication with PLC of the bridge is currently offline !");
                break;
            }

            if (_resource.Mode != nameof(ResourceModeIdentifiers.AUTOMATIC))
            {
                logger.LogInformation(
                    "Bridge is in mode=[{IsMode}]!=[{ShouldBeMode}]", _resource.Mode, nameof(ResourceModeIdentifiers.AUTOMATIC));
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

        logger.LogKeyValue("rValue", rValue);
        logger.LeaveMethod();
        return rValue;
    }

    private async Task<bool> TryToStartNewOrder()
    {
        logger.EnterMethod();

        bool rValue = true;
        while (true)
        {
            if (!await VerifyGeneralConditionsToStartOrder())
            {
                rValue = false;
                break;
            }

            logger.LogKeyValue("_lastJobType", _lastJobType.ToString());
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

        logger.LogKeyValue("rValue", rValue);
        logger.LeaveMethod();
        return rValue;
    }
    #endregion

    #region Input orders
    private async Task<bool> VerifyConditionsToStartInputOrder()
    {
        logger.EnterMethod();

        bool rValue = true;
        while (true)
        {
            if (!_resource!.InfeedEnabled)
            {
                logger.LogInformation(
                    "Infeeds are not enabled for that bridge !");
                rValue = false;
                break;
            }

            break;
        }

        logger.LogKeyValue("rValue", rValue);
        logger.LeaveMethod();
        return rValue;
    }

    private async Task<bool> TryToStartInputOrder()
    {
        logger.EnterMethod();

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

        logger.LogKeyValue("rValue", rValue);
        logger.LeaveMethod();
        return rValue;
    }
    #endregion

    #region Output orders
    private async Task<bool> VerifyConditionsToStartOutputOrder()
    {
        logger.EnterMethod();

        bool rValue = true;
        while (true)
        {
            if (!_resource!.OutfeedEnabled)
            {
                logger.LogInformation(
                    "Outfeeds are not enabled for that bridge !");
                rValue = false;
                break;
            }

            break;
        }

        logger.LogKeyValue("rValue", rValue);
        logger.LeaveMethod();
        return rValue;
    }

    private async Task<bool> TryToStartOutputOrder()
    {
        logger.EnterMethod();

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

        logger.LogKeyValue("rValue", rValue);
        logger.LeaveMethod();
        return rValue;
    }
    #endregion

    #region Relocation orders
    private async Task<bool> VerifyConditionsToStartRelocationOrder()
    {
        logger.EnterMethod();

        bool rValue = true;
        while (true)
        {
            if (!_resource!.RelocationEnabled)
            {
                logger.LogInformation(
                    "Relocations are not enabled for that bridge !");
                rValue = false;
                break;
            }

            break;
        }

        logger.LogKeyValue("rValue", rValue);
        logger.LeaveMethod();
        return rValue;
    }

    private async Task<bool> TryToStartRelocationOrder()
    {
        logger.EnterMethod();

        if (!await VerifyConditionsToStartRelocationOrder()) return false;

        _lastJobType = JobTypeIdentifiers.RELOCATION;
        return true;
    }
    #endregion

    #region Palletizing orders
    private async Task<bool> VerifyConditionsToStartPalletizingOrder()
    {
        logger.EnterMethod();

        bool rValue = true;
        while (true)
        {
            break;
        }

        logger.LogKeyValue("rValue", rValue);
        logger.LeaveMethod();
        return rValue;
    }

    private async Task<bool> TryToStartPalletizingOrder()
    {
        logger.EnterMethod();

        if (!await VerifyConditionsToStartPalletizingOrder()) return false;

        _lastJobType = JobTypeIdentifiers.PALLETIZING;
        return true;
    }
    #endregion

    #region PLC notifications
    private async Task Process_STAT(STATBridge stat)
    {
        logger.EnterMethod();

        string json = GLogWareMessage.Serialize<STATBridge>(stat);
        logger.LogKeyValue("stat", $"\r\n{json}\r\n]");
        
        var r = _db.Resources.Where(x => x.Name == OP).FirstOrDefault();
        if (r == null) 
        {
            logger.LogError("Unknown resource [{OP}]", OP);
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

        logger.LeaveMethod();
    }

    private async Task Process_COMP(COMP comp)
    {
        logger.EnterMethod();

        string json = GLogWareMessage.Serialize<COMP>(comp);
        logger.LogKeyValue("comp", $"\r\n{json}\r\n]");

        var q = _db.Jobs.Where(x =>
                    x.Bridge == OP &&
                    x.Status == JobStatusIdentifiers.BRIDGE_LOAD.ToString()
                );

        int count = 0;
        foreach (var j in q)
        {
            logger.LogKeyValue($"jobs[{count}].jobId", j.Jobid);
            logger.LogKeyValue($"jobs[{count}].Type", j.Type);
            logger.LogKeyValue($"jobs[{count}].SourcePlace", j.SourcePlace);
            logger.LogKeyValue($"jobs[{count}].DestinationPlace", j.DestinationPlace);
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
            logger.LogError("Found [{Count}] jobs where an unique one is expected !", count);
        }

        logger.LeaveMethod();
    }
    #endregion

    #endregion
}