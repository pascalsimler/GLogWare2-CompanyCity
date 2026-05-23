using Gudel.GLogWare.Shared;
using System.Timers;

namespace Gudel.GLogWare.BridgeSimulator;

public partial class BridgeSimulator
{
    private BridgeConfiguration _bridgeConfiguration = null!;
    private STATBridge? _currentSTAT = null;
    private ORDS? _currentORDS = null;
    private System.Timers.Timer _orderExecutionTimer = null!;

    private async Task InitSimulation()
    {
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
    }

    private void SetBridgeConfiguration(BridgeConfiguration bc)
    {
        _bridgeConfiguration = bc;
        _logger.LogInformation($"DelaySendCOMP=[{_bridgeConfiguration.DelaySendCOMP}]");
    }

    private void ProcessPlcMessage(PlcMessage pm)
    {
        switch (pm.Identifier)
        {
            case PlcMessageIdentifiers.STAT:
                _currentSTAT = GLogWareMessage.DeSerialize<STATBridge>(pm.Data!.ToString()!)!;
                break;
            case PlcMessageIdentifiers.COMP:
                break;
        }
    }

    //private async Task ProcessGLogWareTelegram(LegacyPlcTelegram t)
    //{
    //    switch (t.Identifier)
    //    {
    //        case nameof(PlcMessageIdentifiers.ORDS):
    //            ORDSStruct ordsStruct = ORDSStruct.FromData(t.Data);
    //            if (_currentSTAT == null) break;
    //            if (_currentSTAT.Parked || _currentSTAT.WorkingMode != STATBridgeWorkingModes.AUTOMATIC)
    //            {
    //                await SendCOMP(ordsStruct.Jobid, "0001");
    //                break;
    //            }
    //            if (_currentORDS != null)
    //            {
    //                await SendCOMP(ordsStruct.Jobid, "0002");
    //                break;
    //            }
    //            if (_bridgeConfiguration.DelaySendCOMP > 0)
    //            {
    //                _currentORDS = ordsStruct.ToMessage(OP!);
    //                _orderExecutionTimer.Interval = _bridgeConfiguration.DelaySendCOMP;
    //                _orderExecutionTimer.Start();
    //                break;
    //            }
    //            break;
    //    }
    //}

    private async void OnOrderExecutionCompleted(object source, ElapsedEventArgs e)
    {
        if (_currentORDS == null) return;

        _orderExecutionTimer.Stop();
        await SendCOMP(_currentORDS.Jobid, "0000");
        _currentORDS = null;
    }

    private async Task SendCurrentSTAT()
    {
        if (_currentSTAT == null) return;

        PlcMessage pm = new PlcMessage();
        pm.Identifier = PlcMessageIdentifiers.STAT;
        pm.Data = _currentSTAT;
        GLogWareMessage m = new GLogWareMessage();
        m.Identifier = GLogWareMessageIdentifiers.ToGLogWare;
        m.Data = pm;

        await SendGLogWareMessageToMqtt(_subscriptionTopic, m);
    }

    private async Task SendCOMP(string jobId, string feedbackCode)
    {
        COMP comp = new COMP();
        comp.Jobid = jobId;
        comp.FeedbackCode = feedbackCode;

        PlcMessage pm = new PlcMessage();
        pm.Identifier = PlcMessageIdentifiers.COMP;
        pm.Data = comp;

        GLogWareMessage m = new GLogWareMessage();
        m.Identifier = GLogWareMessageIdentifiers.ToGLogWare;
        m.Data = pm;

        await SendGLogWareMessageToMqtt(_subscriptionTopic, m);
    }
}