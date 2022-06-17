using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yup.Soporte.Api.Application.IntegrationEvents;

namespace Yup.Soporte.Api.Infrastructure.Services;

public interface IEventBus
{
    void Publish(IntegrationEvent @event);
}
