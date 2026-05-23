using Gudel.GLogWare.EFCore.Domain;
using Gudel.GLogWare.EFCore.Infrastructure;
using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.BridgeManager;

public partial class BridgeManager
{
    private Resource? _resource;
    private JobTypeIdentifiers _lastJobType;
    private GLogWareDbContext _db = null!;
    
    private async Task<bool> VerifyGeneralConditionsToStartOrder()
    {
        _resource = _db.Resources
            .Where(x => x.Name == OP)
            .FirstOrDefault();
        if (_resource == null)
        {
            _logger.LogError(
                $"Resouce.Name=[{OP}] does not exist !");
            return false;
        }

        if (!_resource.IsOnline)
        {
            _logger.LogInformation(
                $"Communication with PLC of the bridge is currently offline !");
            return false;
        }

        if (_resource.Mode != nameof(ResourceModeIdentifiers.AUTOMATIC))
        {
            _logger.LogInformation(
                $"Bridge is in mode=[{_resource.Mode}]!=[{nameof(ResourceModeIdentifiers.AUTOMATIC)}]");
            return false;
        }

        var j = _db.Jobs
            .Where(j => 
                j.Bridge == OP &&
                j.Status.StartsWith("GANTRY")
             )
            .FirstOrDefault();
        if (j != null)
        {
            return false;
        }

        return true;
    }

    private async Task<bool> TryToStartNewOrder()
    {
        if (!await VerifyGeneralConditionsToStartOrder()) return false;

        _logger.LogInformation($"_lastJobType=[{_lastJobType.ToString()}]");
        switch (_lastJobType)
        {
            case JobTypeIdentifiers.INFEED:
                if (await TryToStartOutputOrder()) return true;
                if (await TryToStartRelocationOrder()) return true;
                if (await TryToStartInputOrder()) return true;
                break;
            case JobTypeIdentifiers.OUTFEED:
                if (await TryToStartInputOrder()) return true;
                if (await TryToStartRelocationOrder()) return true;
                if (await TryToStartOutputOrder()) return true;
                break;
            case JobTypeIdentifiers.RELOCATION:
                if (await TryToStartInputOrder()) return true;
                if (await TryToStartOutputOrder()) return true;
                if (await TryToStartRelocationOrder()) return true;
                break;
            case JobTypeIdentifiers.PALLETIZING:
                if (await TryToStartPalletizingOrder()) return true;
                break;
        }

        _logger.LogInformation($"No pending jobs available !");
        return false;
    }

    private async Task<bool> VerifyConditionsToStartInputOrder()
    {
        if (!_resource!.InfeedEnabled)
        {
            _logger.LogInformation(
                $"Infeeds are not enabled for that bridge !");
            return false;
        }
        return true;
    }

    private async Task<bool> TryToStartInputOrder()
    {
        if (!await VerifyConditionsToStartInputOrder()) return false;

        _lastJobType = JobTypeIdentifiers.INFEED;
        return true;
    }

    private async Task<bool> VerifyConditionsToStartOutputOrder()
    {
        if (!_resource!.OutfeedEnabled)
        {
            _logger.LogInformation(
                $"Outfeeds are not enabled for that bridge !");
            return false;
        }
        return true;
    }

    private async Task<bool> TryToStartOutputOrder()
    {
        if (!await VerifyConditionsToStartOutputOrder()) return false;

        _lastJobType = JobTypeIdentifiers.OUTFEED;
        return true;
    }

    private async Task<bool> VerifyConditionsToStartRelocationOrder()
    {
        if (!_resource!.RelocationEnabled)
        {
            _logger.LogInformation(
                $"Relocations are not enabled for that bridge !");
            return false;
        }
        return true;
    }

    private async Task<bool> TryToStartRelocationOrder()
    {
        if (!await VerifyConditionsToStartRelocationOrder()) return false;

        _lastJobType = JobTypeIdentifiers.RELOCATION;
        return true;
    }

    private async Task<bool> VerifyConditionsToStartPalletizingOrder()
    {
        return false;
    }

    private async Task<bool> TryToStartPalletizingOrder()
    {
        if (!await VerifyConditionsToStartPalletizingOrder()) return false;

        _lastJobType = JobTypeIdentifiers.PALLETIZING;
        return true;
    }

    private async Task SimulatePlcTelegram(PlcMessage pm)
    {
        switch (pm.Identifier)
        {
            case PlcMessageIdentifiers.STAT:
                STATBridge stat = GLogWareMessage.DeSerialize<STATBridge>(pm.Data!.ToString()!)!;
                await Process_STAT(stat);
                break;
            case PlcMessageIdentifiers.COMP:
                COMP comp = GLogWareMessage.DeSerialize<COMP>(pm.Data!.ToString()!)!;
                await Process_COMP(comp);
                break;
            default:
                break;
        }
    }

    private async Task Process_STAT(STATBridge stat)
    {
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
    }

    private async Task Process_COMP(COMP comp)
    {
        _db = _dbContextFactory.CreateDbContext();

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

        if (count != 1)
        {
            _logger.LogError($"Anomaly: job count=[{count}] != [1] !");
            return;
        }

        var job = q.FirstOrDefault();
        if (comp.FeedbackCode == "0000")
        {
            job!.Status = JobStatusIdentifiers.BRIDGE_LOAD_END.ToString();
        }

        await _db.SaveChangesAsync();
  
/*
    bewRT:= getBewRT_BasedOnJobId(sJobId);

        IF iFeedbackCode = '0000' THEN

            bewRT.SPS_FEHLER := NULL;
        bewRT.STATUS_BEW := pack_Global.BEWSTATUS__GANTRY_LOAD_END;
        ELSE

            bewRT.SPS_FEHLER := iFeedbackCode;
        END IF;

        SELECT DECODE(iFeedbackCode,

            '0000', '',
			'Unknown FeedbackCode=[' || iFeedbackCode || ']'
		)
		INTO sInfo

        FROM DUAL;

        bewRT.INFO := sInfo;
        pack_bew.UpdateBew(bewRT);
*/
    }
}