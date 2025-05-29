using System;

namespace Fog.Dialogue {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class |
                    AttributeTargets.Struct)]
    public class HideInInspectorIf : BaseHideInInspectorIf {
        public HideInInspectorIf(string conditionName) : base(conditionName, false) { }
    }
}