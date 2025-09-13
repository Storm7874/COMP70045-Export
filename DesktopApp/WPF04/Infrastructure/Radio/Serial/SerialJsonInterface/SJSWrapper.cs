using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WPF04.Infrastructure.Radio.Serial.SerialJsonInterface
{
    /// <summary>
    /// Wrapper class for Serialised Json Statements (SJS).
    /// </summary>
    public class SJSWrapper
    {
        //statement type field - e.g, "GetGlobalConfig", "TxNewMessage", "RxMessage"
        public string statementType { get; set; } = string.Empty;

        //statement payload field - can, or can not be, be any valid json object
        public JsonNode? statementPayload { get; set; }
    }
}
