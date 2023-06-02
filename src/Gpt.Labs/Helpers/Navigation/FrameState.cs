using Gpt.Labs.Models;
using System.Collections.Generic;

namespace Gpt.Labs.Helpers.Navigation
{
    public class FrameState
    {
        #region Properties

        public Dictionary<string, ViewModelState> PageState { get; set; } = new Dictionary<string, ViewModelState>();

        public string Navigation { get; set; }

        #endregion
    }
}
