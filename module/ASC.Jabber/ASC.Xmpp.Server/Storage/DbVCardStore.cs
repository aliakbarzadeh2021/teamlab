using System;
using System.Collections.Generic;
using System.Data;
using agsXMPP;
using agsXMPP.protocol.iq.vcard;
using agsXMPP.util;
using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Xmpp.Server.Storage.Interface;

namespace ASC.Xmpp.Server.Storage
{
    public class DbVCardStore : DbStoreBase, IVCardStore
    {
        private IDictionary<string, Vcard> vcardsCache = new SynchronizedDictionary<string, Vcard>();


        protected override SqlCreate[] GetCreateSchemaScript()
        {
            var t1 = new SqlCreate.Table("jabber_vcard", true)
                .AddColumn("jid", DbType.String, 255, true)
                .AddColumn("vcard", DbType.String, UInt16.MaxValue, true)
                .PrimaryKey("jid");
            return new[] { t1 };
        }


        public virtual void SetVCard(Jid jid, Vcard vcard)
        {
            if (jid == null) throw new ArgumentNullException("jid");
            if (vcard == null) throw new ArgumentNullException("vcard");

            try
            {
                lock (vcardsCache)
                {
                    ExecuteNonQuery(
                        new SqlInsert("jabber_vcard", true)
                        .InColumnValue("jid", jid.Bare.ToLowerInvariant())
                        .InColumnValue("vcard", ElementSerializer.SerializeElement(vcard)));
                    vcardsCache[jid.Bare.ToLowerInvariant()] = vcard;
                }
            }
            catch (Exception e)
            {
                throw new JabberException("Could not save VCard.", e);
            }
        }

        public virtual Vcard GetVCard(Jid jid)
        {
            if (jid == null) throw new ArgumentNullException("jid");

            try
            {
                lock (vcardsCache)
                {
                    var bareJid = jid.Bare.ToLowerInvariant();
                    if (!vcardsCache.ContainsKey(bareJid))
                    {
                        var vcardStr = ExecuteScalar<string>(new SqlQuery("jabber_vcard").Select("vcard").Where("jid", bareJid));
                        vcardsCache[bareJid] = !string.IsNullOrEmpty(vcardStr) ? ElementSerializer.DeSerializeElement<Vcard>(vcardStr) : null;
                    }
                    return vcardsCache[bareJid];
                }
            }
            catch (Exception e)
            {
                throw new JabberException("Could not get VCard.", e);
            }
        }

        public virtual ICollection<Vcard> Search(Vcard pattern)
        {
            return new Vcard[0];
        }
    }
}
