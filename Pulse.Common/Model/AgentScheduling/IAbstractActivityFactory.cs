using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Abstract;

namespace Pulse.Common.Model.AgentScheduling
{
    public interface IAbstractActivityFactory
    {
        AbstractPoiActivity GetAbstractRoleActivity(IRole role, string activityName);
    }
}
