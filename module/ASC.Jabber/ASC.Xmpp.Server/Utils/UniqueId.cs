// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UniqueId.cs" company="">
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using agsXMPP.util;

namespace ASC.Xmpp.Server.Utils
{
	#region usings

	using System.Security.Cryptography;

	#endregion

	/// <summary>
	/// Summary description for UniqueId.
	/// </summary>
	public class UniqueId
	{
		// Lenght of the Session ID on bytes,
		// 4 bytes equaly 8 chars
		// 16^8 possibilites for the session IDs (4.294.967.296)
		// This should be unique enough
		#region Members

		/// <summary>
		/// </summary>
		private static int m_lenght = 4;

		#endregion

		#region Methods

	    /// <summary>
	    /// </summary>
	    /// <returns>
	    /// </returns>
	    public static string CreateNewId()
	    {
	        return CreateNewId(m_lenght);
	    }

	    /// <summary>
		/// </summary>
		/// <returns>
		/// </returns>
		public static string CreateNewId(int length)
		{
			RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] buf = new byte[length];
			rng.GetBytes(buf);

			return Hash.HexToString(buf);
		}

		#endregion
	}
}