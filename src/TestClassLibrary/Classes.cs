#pragma warning disable CS0649
#pragma warning disable CS0067
#pragma warning disable CS0169
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClassLibrary.net45
{
    /// <summary>Include</summary>
    /// <remarks>Exclude</remarks>
    public interface PublicInterface
    {
        /// <summary>Include</summary>
        /// <remarks>Exclude</remarks>
        int MyProperty { get; }
    }

    /// <summary>Exclude</summary>
    /// <remarks>Exclude</remarks>
    internal interface InternalInterface
    {
        /// <summary>Include</summary>
        /// <remarks>Exclude</remarks>
        int MyProperty { get; }
    }

    /// <summary>Exclude</summary>
    /// <remarks>Exclude</remarks>
    internal class InternalClass
    {
        /// <summary>Exclude</summary>
        /// <remarks>Exclude</remarks>
        public void PublicMethod() { }

        /// <summary>Exclude</summary>
        /// <remarks>Exclude</remarks>
        public int PublicProperty { get { return 0; } }
    }

    /// <summary>Include</summary>
    /// <remarks>Exclude</remarks>
    public class Class1
    {
        /// <summary>Include</summary><remarks>exclude</remarks>
        public class NestedPublicClass
        {
        }
        /// <summary>Exclude</summary><remarks>exclude</remarks>
        internal class NestedInternalClass
        {
        }
        /// <summary>Exclude</summary><remarks>exclude</remarks>
        private class NestedPrivateClass
        {
        }

        /// <summary>Include</summary><remarks>exclude</remarks>
        protected void ProtectedMethod()
        {
        }
        /// <summary>Include</summary><remarks>exclude</remarks>
        protected internal void ProtectedInternal()
        {
        }

        /// <summary>Include</summary><remarks>exclude</remarks>
        /// <value>Include</value>
        public int PublicProperty { get; } = 0;

        /// <summary>Include</summary><remarks>exclude</remarks>
        /// <param name="parameter">Include</param>
        /// <returns>Include</returns>
        public int PublicMethod(int parameter) {  return 0; }

        /// <summary>Include</summary><remarks>exclude</remarks>
        public event EventHandler PublicEvent;

        /// <summary>Include</summary><remarks>exclude</remarks>
        public int PublicField;

        /// <summary>Include</summary><remarks>exclude</remarks>
        /// <value>Include</value>
        internal int InternalProperty { get; } = 0;

        /// <summary>Exclude</summary><remarks>exclude</remarks>
        /// <param name="parameter">Exclude</param>
        /// <returns>exclude</returns>
        internal int InternalMethod(int parameter) { return 0; }

        /// <summary>Exclude</summary><remarks>exclude</remarks>
        internal event EventHandler InternalEvent;

        /// <summary>Exclude</summary><remarks>exclude</remarks>
        internal int InternalField;

        /// <summary>Include</summary><remarks>exclude</remarks>
        /// <value>Include</value>
        private int PrivateProperty { get; } = 0;

        /// <summary>Exclude</summary><remarks>exclude</remarks>
        /// <param name="parameter">Exclude</param>
        /// <returns>exclude</returns>
        private int PrivateMethod(int parameter) { return 0; }

        /// <summary>Exclude</summary><remarks>exclude</remarks>
        private event EventHandler PrivateEvent;

        /// <summary>Exclude</summary><remarks>exclude</remarks>
        private int PrivateField;
    }
}
#pragma warning restore CS0067
#pragma warning restore CS0169
#pragma warning restore CS0649