
using System.Collections.Generic;
namespace ASC.Web.Core.Subscriptions
{
    
    public enum GroupByType
    {
        Modules,
        Groups
    }
    public interface IProductSubscriptionManager : ISubscriptionManager
    {
        GroupByType GroupByType { get; }
        List<SubscriptionGroup> GetSubscriptionGroups();
    }

}
