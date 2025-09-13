using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WPF04.Infrastructure.Radio.Serial.SerialJsonInterface
{
    public class SJSBuilder
    {
        //Method to assemble a new SJS based on varying input types
        public static string BuildSJSStatement(string statementType, object statementPayload)
        {
            //Create a temporary JsonNode for the payload
            JsonNode payloadNode;

            //Determine the type of the payload and handle accordingly
            switch (statementPayload)
            {
                //Statement payload is absent
                case null:
                    payloadNode = JsonValue.Create("");
                    break;

                //Statement payload is a simple string
                case string s:
                    payloadNode = JsonValue.Create(s);
                    break;

                //Statement payload is another json object
                case JsonNode jn:
                    payloadNode = jn;
                    break;


                default:
                    payloadNode = JsonSerializer.SerializeToNode(statementPayload);
                    break;
            }

            //Assemble the root object
            var root = new JsonObject
            {
                //Command to execute
                ["statementType"] = statementType,
                //Payload for the command
                ["statementPayload"] = payloadNode

            };

            //Return the assembled SJS as a string
            return root.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        }
    }
}
