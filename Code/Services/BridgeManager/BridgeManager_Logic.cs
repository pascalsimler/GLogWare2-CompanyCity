using Gudel.GLogWare.EFCore.Domain;

namespace Gudel.GLogWare.BridgeManager;

public partial class BridgeManager
{
    private JobTypeIdentifiers _lastJobType;

    private async Task<bool> VerifyGeneralConditionsToStartOrder()
    {
        return false;
    }

    private async Task<bool> TryToStartNewOrder()
    {
        if (!await VerifyGeneralConditionsToStartOrder()) return false;

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

}