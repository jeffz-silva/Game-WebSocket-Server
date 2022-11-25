using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Components
{
    public class WeakRef : WeakReference
    {
        private static readonly WeakRef.NullValue NULL = new WeakRef.NullValue();

        public WeakRef(object target)
          : base(target == null ? (object)WeakRef.NULL : target)
        {
        }

        public WeakRef(object target, bool trackResurrection)
          : base(target == null ? (object)WeakRef.NULL : target, trackResurrection)
        {
        }

        public override object Target
        {
            get
            {
                object target = base.Target;
                if (target != WeakRef.NULL)
                    return target;
                return (object)null;
            }
            set
            {
                base.Target = value == null ? (object)WeakRef.NULL : value;
            }
        }

        private class NullValue
        {
        }
    }
}
