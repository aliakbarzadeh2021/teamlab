using System;

namespace ASC.Web.Core
{
    public interface IGlobalHandler
    {
        /// <summary>
        /// Initialization complite handler
        /// </summary>
        void InitItemsComplete(IWebItem[] items);

        /// <summary>
        /// User login handler
        /// </summary>
        /// <param name="userID">User indetifier</param>
        void Login(Guid userID);

        /// <summary>
        /// User logout handler
        /// </summary>
        /// <param name="userID">User identifier</param>
        void Logout(Guid userID);

        /// <summary>
        /// Session end handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SessionEnd(object sender, EventArgs e);

        /// <summary>
        /// Begin request handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApplicationBeginRequest(object sender, EventArgs e);

        /// <summary>
        /// End request handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ApplicationEndRequest(object sender, EventArgs e);
    }   
}
