﻿namespace InSearch.Core.Data.Hooks
{
    /// <summary>
    /// A hook that is executed after an action.
    /// </summary>
    public interface IPostActionHook : IHook
    {
        /// <summary>
        /// Gets the entity state to listen for.
        /// </summary>
        EntityState HookStates { get; }
    }
}
