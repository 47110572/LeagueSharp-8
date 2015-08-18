using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EverestUtils.Tween
{
    /// <summary>
    ///     State of a Tween
    /// </summary>
    public enum TweenState
    {
        /// <summary>
        ///     The tween is running
        /// </summary>
        Running,

        /// <summary>
        ///     The tween is paused
        /// </summary>
        Paused,

        /// <summary>
        ///     The tween has stopped
        /// </summary>
        Stopped
    }
}
