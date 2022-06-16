using System;
using System.Collections;
using System.Reflection;
using Rundo.Core.Data;
using Rundo.Core.Utils;

namespace Rundo.Core.Commands
{
    public class SetValueToMemberCommand : DataCommand<object>
    {
        private MemberInfo _memberInfo;
        
        private object _value;

        public SetValueToMemberCommand(object obj, string memberName, object value) : base(obj)
        {
            Init(obj, ReflectionUtils.GetMemberInfo(obj.GetType(), memberName), value);
        }

        public SetValueToMemberCommand(object obj, MemberInfo memberInfo, object value) : base(obj)
        {
            Init(obj, memberInfo, value);
        }

        private void Init(object obj, MemberInfo memberInfo, object value)
        {
            if (obj == null)
                throw new Exception($"Modifying object cant be null");
            if (obj is IList _)
                throw new Exception($"Use {nameof(SetValueToListCommand)} for object of {nameof(IList)} type");
            if (obj.GetType().IsValueType)
                throw new Exception($"Target type {obj.GetType()} cant be a value type, use {nameof(DataHandler)} as target");
            if (memberInfo == null)
                throw new Exception($"MemberInfo is null");

            _memberInfo = memberInfo;
            _value = value;
        }

        public override ICommand CreateUndo()
        {
            return new SetValueToMemberCommand(Data, _memberInfo, ReflectionUtils.GetValue(Data, _memberInfo));
        }

        protected override void ProcessInternal()
        {
            ReflectionUtils.SetValue(Data, _memberInfo, _value);
        }
    }

}

