using System.Collections.Generic;

namespace XmqqyBackpack
{
    /// <summary>
    /// 实现此接口的物体可在创建时接收额外数据（如变种、耐久等）
    /// </summary>
    public interface IExtraDataInitializable
    {
        void Initialize(Dictionary<string, object> extraData);
    }
}