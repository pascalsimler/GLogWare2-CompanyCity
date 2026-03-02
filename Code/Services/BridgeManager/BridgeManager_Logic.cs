using Gudel.GLogWare.EFCore.Domain;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Gudel.GLogWare.BridgeManager;

public partial class BridgeManager
{
    private JobTypeIdentifiers _lastJobType;

    private async Task<bool> VerifyGeneralConditionsToStartOrder()
    {
        var q1 = _db.Resources.Where(r => r.Name == OP).FirstOrDefault();
        if (q1 == null)
        {
            return false;
        }

        var q2 = _db.Jobs
            .Where(j => 
                j.Bridge == OP &&
                j.Status.StartsWith("GANTRY")
             )
            .FirstOrDefault();
        if (q2 != null)
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

        return false;
    }

    private async Task<bool> VerifyConditionsToStartInputOrder()
    {
        return false;
    }

    private async Task<bool> TryToStartInputOrder()
    {
        if (!await VerifyConditionsToStartInputOrder()) return false;

        _lastJobType = JobTypeIdentifiers.INFEED;
        return true;
    }

    private async Task<bool> VerifyConditionsToStartOutputOrder()
    {
        return false;
    }

    private async Task<bool> TryToStartOutputOrder()
    {
        if (!await VerifyConditionsToStartOutputOrder()) return false;

        _lastJobType = JobTypeIdentifiers.OUTFEED;
        return true;
    }

    private async Task<bool> VerifyConditionsToStartRelocationOrder()
    {
        return false;
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

}