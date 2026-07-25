using Gudel.GLogWare.Logging;
using Gudel.GLogWare.Messages;
using System.Timers;

namespace Gudel.GLogWare.Services.BridgeSimulator;

public partial class BridgeSimulator
{
    private BridgeConfiguration _bridgeConfiguration = null!;
    private STATBridge? _currentSTAT = null;
    private ORDS? _currentORDS = null;
    private System.Timers.Timer _orderExecutionTimer = null!;

    private void InitSimulation()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _bridgeConfiguration = new BridgeConfiguration()
        {
            DelaySendCOMP = 1000
        };

        _orderExecutionTimer = new System.Timers.Timer();
        _orderExecutionTimer.Elapsed += OnOrderExecutionCompleted!;
        _orderExecutionTimer.AutoReset = false;
        _orderExecutionTimer.Enabled = false;

        _currentSTAT = new STATBridge();
        _currentSTAT.Parked = false;
        _currentSTAT.WorkingMode = STATBridgeWorkingModes.AUTOMATIC;
        _currentSTAT.GripperOccupied = false;
        _currentSTAT.ErrorFlag = false;

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private void SetBridgeConfiguration(BridgeConfiguration bc)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _bridgeConfiguration = bc;
        _logger.LogInformation($"DelaySendCOMP=[{_bridgeConfiguration.DelaySendCOMP}]");

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task ProcessPlcMessage(PlcMessage pm)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

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

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task Process_ORDS(ORDS ords)
    {
        if (_currentSTAT.Parked || _currentSTAT.WorkingMode != STATBridgeWorkingModes.AUTOMATIC)
        {
            await SendCOMP(ords.Jobid, "0001");
        }
        else if (_currentORDS != null)
        {
            await SendCOMP(ords.Jobid, "0002");
        }
        else
        {
            _currentORDS = ords;
            if (_bridgeConfiguration.DelaySendCOMP > 0)
            {
                _orderExecutionTimer.Interval = _bridgeConfiguration.DelaySendCOMP;
                _orderExecutionTimer.Start();
            }
        }
    }

    private async void OnOrderExecutionCompleted(object source, ElapsedEventArgs e)
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        await CheckOrderExecution();

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task CheckOrderExecution()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        if (_currentORDS != null)
        {
            await SendCOMP(_currentORDS.Jobid, "0000");
            _currentORDS = null;
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task SendCurrentSTAT()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

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
        _logger.LogInformation(LogMessages.EnterMethod);

        COMP comp = new COMP();
        comp.Jobid = jobId;
        comp.FeedbackCode = feedbackCode;

        PlcMessage pm = new PlcMessage();
        pm.Identifier = PlcMessageIdentifiers.COMP;
        pm.Sender = OP!;
        pm.Receiver = "GLOGWARE";
        pm.Data = comp;

        GLogWareMessage m = new GLogWareMessage();
        m.Identifier = GLogWareMessageIdentifiers.ToGLogWare;
        m.Data = pm;

        await SendGLogWareMessage(_subscriptionTopic, m);
    }
}