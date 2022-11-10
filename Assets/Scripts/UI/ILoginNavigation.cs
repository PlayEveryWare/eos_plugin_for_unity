using UnityEngine.UI;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    /// <summary>
    /// Provides a way to get UI navigation information for the UILoginContainer to connect to as well as
    /// setting up the UI outside of the login container for login or logout.
    /// The UILoginContainer sets the UI navigation explicitly and needs to know the next component
    /// to go to when going up from login dropdown.
    /// </summary>
    public interface ILoginNavigation
    {
        /// <summary>
        /// Retrieves the UI component to navigate to when going up the UI tree.
        /// </summary>
        /// <returns>A <see cref="Selectable"/> UI component to navigate to.</returns>
        Selectable GetSelectOnUpObject();

        /// <summary>
        /// Performs any actions needed to set the UI for logging in.
        /// </summary>
        void ConfigureUIForLogin();

        /// <summary>
        /// Performs any actions needed to set the UI for logging out.
        /// </summary>
        void ConfigureUIForLogout();
    }
}
