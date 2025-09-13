using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WPF04.Domain.Entities.PayloadDefinitions;

namespace WPF04.Infrastructure.Radio.Serial.SerialJsonInterface
{
    //public class SJSHandler
    //{
    //    public void HandleIncomingSJS(string json)
    //    {
    //        var wrapper = JsonSerializer.Deserialize<SJSWrapper>(json);

    //        if (wrapper is null)
    //        {
    //            return;
    //        }

    //        switch (wrapper.statementType)
    //        {
    //            case "GetGlobalConfig":
    //                var globalConfig = wrapper.statementPayload?.Deserialize<GetGlobalConfigPayload>();
    //                //HandleTxNewMessage();
    //                break;

    //            case "RxMessage":
    //                var rxMessage = wrapper.statementPayload?.Deserialize<RxMessagePayload>();
    //                //HandleRxMessage
    //                break;
    //        }

    //    }
    //}
}
