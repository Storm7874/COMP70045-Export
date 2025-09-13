using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF04.Domain.Entities;
using WPF04.Domain.Entities.Message;

namespace WPF04.Domain.Interfaces
{
    public interface ITransceiver
    {
        public TransceiverConfig? UpdateLocalConfig();

        public string UpdateRemoteConfig(TransceiverConfig newConfig);
    }
}
