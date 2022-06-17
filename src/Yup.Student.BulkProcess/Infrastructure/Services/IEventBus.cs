using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yup.Student.BulkProcess.Application.IntegrationEvents;

namespace Yup.Student.BulkProcess.Infrastructure.Services;

public interface IEventBus
{
    Task Publish(IntegrationEvent @event);
}
