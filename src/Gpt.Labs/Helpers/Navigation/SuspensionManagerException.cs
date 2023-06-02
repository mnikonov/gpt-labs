using System;

namespace Gpt.Labs.Helpers.Navigation
{
    public class SuspensionManagerException : Exception
    {
        #region Constructors

        public SuspensionManagerException()
        {
        }

        public SuspensionManagerException(Exception e)
            : base("SuspensionManager failed", e)
        {
        }

        #endregion
    }
}
