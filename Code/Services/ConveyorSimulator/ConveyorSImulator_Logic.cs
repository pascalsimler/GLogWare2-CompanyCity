using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;
using System.Timers;

namespace Gudel.GLogWare.Services.ConveyorSimulator;

public partial class ConveyorSimulator
{
    private BridgeConfiguration _bridgeConfiguration = null!;
    private STATBridge? _currentSTAT = null;
    private ORDS? _currentORDS = null;
    private System.Timers.Timer _orderExecutionTimer = null!;

    private void InitSimulation()
    {
        _logger.EnterMethod();

        _bridgeConfiguration = new BridgeConfiguration()
        {
            DelaySendCOMP = 1000
        };

        _orderExecutionTimer = new System.Timers.Timer();
        _orderExecutionTimer.Elapsed += OnOrderExecutionCompleted!;
        _orderExecutionTimer.AutoReset = false;
        _orderExecutionTimer.Enabled = false;

        _currentSTAT = new()
        {
            Parked = false,
            WorkingMode = STATBridgeWorkingModes.AUTOMATIC,
            GripperOccupied = false,
            ErrorFlag = false
        };
        _logger.LeaveMethod();
    }

    private void SetBridgeConfiguration(BridgeConfiguration bc)
    {
        _logger.EnterMethod();

        _bridgeConfiguration = bc;
        _logger.LogKeyValue("DelaySendCOMP", _bridgeConfiguration.DelaySendCOMP);

        _logger.LeaveMethod();
    }

    private async Task ProcessPlcMessage(PlcMessage pm)
    {
        _logger.EnterMethod();

        switch (pm.Identifier)
        {
            case PlcMessageIdentifiers.STAT:
                _currentSTAT = GLogWareMessage.DeSerialize<STATBridge>(pm.Data!.ToString()!)!;
                break;
            case PlcMessageIdentifiers.COMP:
                if (_currentORDS != null)
                {
                    COMP comp = GLogWareMessage.DeSerialize<COMP>(pm.Data!.ToString()!)!;
                    if ( comp.FeedbackCode == "0000")
                    {
                        _currentORDS = null;
                    }
                }
                break;
            default:
                break;
        }

        _logger.LeaveMethod();
    }

    private async Task Process_TARG(TARG targ)
    {
    }

    private async void OnOrderExecutionCompleted(object source, ElapsedEventArgs e)
    {
        _logger.EnterMethod();

        await CheckOrderExecution();

        _logger.LeaveMethod();
    }

    private async Task CheckOrderExecution()
    {
        _logger.EnterMethod();

        if (_currentORDS != null)
        {
            await SendCOMP(_currentORDS.Jobid, "0000");
            _currentORDS = null;
        }

        _logger.LeaveMethod();
    }

    private async Task SendCurrentSTAT()
    {
        _logger.EnterMethod();

        if (_currentSTAT == null) return;

        PlcMessage pm = new PlcMessage();
        pm.Identifier = PlcMessageIdentifiers.STAT;
        pm.Data = _currentSTAT;
        GLogWareMessage m = new GLogWareMessage();
        m.Identifier = GLogWareMessageIdentifiers.ToGLogWare;
        m.Data = pm;

        await SendGLogWareMessage(_subscriptionTopic, m);
    }

    private async Task SendCOMP(string jobId, string feedbackCode)
    {
        _logger.EnterMethod();

        COMP comp = new()
        {
            Jobid = jobId,
            FeedbackCode = feedbackCode
        };

        PlcMessage pm = new()
        {
            Identifier = PlcMessageIdentifiers.COMP,
            Sender = OP!,
            Receiver = "GLOGWARE",
            Data = comp
        };

        GLogWareMessage m = new()
        {
            Identifier = GLogWareMessageIdentifiers.ToGLogWare,
            Data = pm
        };

        await SendGLogWareMessage(_subscriptionTopic, m);

        _logger.LeaveMethod();
    }
}