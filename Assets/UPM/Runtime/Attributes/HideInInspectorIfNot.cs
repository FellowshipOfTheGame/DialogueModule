using System;

namespace Fog.Dialogue {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class |
                    AttributeTargets.Struct)]
    public class HideInInspectorIfNot : BaseHideInInspectorIf {
        public HideInInspectorIfNot(string conditionName) : base(conditionName, true) { }
    }
}