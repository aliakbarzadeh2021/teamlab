/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2008 by AG-Software 											 *
 * All Rights Reserved.																 *
 * Contact information for AG-Software is available at http://www.ag-software.de	 *
 *																					 *
 * Licence:																			 *
 * The agsXMPP SDK is released under a dual licence									 *
 * agsXMPP can be used under either of two licences									 *
 * 																					 *
 * A commercial licence which is probably the most appropriate for commercial 		 *
 * corporate use and closed source projects. 										 *
 *																					 *
 * The GNU Public License (GPL) is probably most appropriate for inclusion in		 *
 * other open source projects.														 *
 *																					 *
 * See README.html for details.														 *
 *																					 *
 * For general enquiries visit our website at:										 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;

using agsXMPP.protocol.client;

namespace agsXMPP.protocol.iq.bind
{
    using client;

    /// <summary>
	/// Summary description for BindIq.
	/// </summary>
	public class BindIq : IQ
	{
		private Bind m_Bind = new Bind();
		
		public BindIq()
		{
			this.GenerateId();
			this.AddChild(m_Bind);
		}

		public BindIq(IqType type) : this()
		{			
			this.Type = type;		
		}

		public BindIq(IqType type, Jid to) : this()
		{			
			this.Type = type;
			this.To = to;
		}

		public BindIq(IqType type, Jid to, string resource) : this(type, to)
		{			
			m_Bind.Resource = resource;
		}

        public new Bind Query
        {
            get
            {
                return m_Bind;
            }
        }
	}
}
