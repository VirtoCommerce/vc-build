using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Nuke.Common;

namespace Cloud.Models
{
    public class VirtoCloudError
    {
        public string Message { get; set; }
        public List<string> Errors { get; set; }

        public string GetErrorMessage()
        {
            var message = Message;
            if (Errors?.Count > 0)
            {
                message += Environment.NewLine + string.Join(Environment.NewLine, Errors);
            }

            return message;
        }

        public static VirtoCloudError FromStringResponse(string response)
        {
            try
            {
                var cloudError = JsonConvert.DeserializeObject<VirtoCloudError>(response);
                var message = cloudError?.Message ?? response;
                var errors = cloudError?.Errors ?? new List<string>();
                return new VirtoCloudError {
                    Message = message,
                    Errors = errors
                };
            }
            catch (JsonReaderException)
            {
                return new VirtoCloudError
                {
                    Message = response
                };
            }
        }
    }
}
