using agsXMPP;

namespace ASC.Xmpp.Server.Services.Muc2.Room.Member
{
    using System;
    using agsXMPP.protocol.x.muc;

    internal class MucRoomMemberInfo : IEquatable<MucRoomMemberInfo>
    {
        public Jid Jid { get; set; }
        public Affiliation Affiliation { get; set;}
        public Role Role { get; set;}

        public MucRoomMemberInfo()
        {
            
        }

        public MucRoomMemberInfo(string record)
        {
            string[] fields = record.Trim(';',' ').Split(':');
            if (fields.Length!=3)
            {
                throw new ArgumentException("bad format");
            }
            Jid = new Jid(fields[0]);
            Affiliation = (Affiliation) Enum.Parse(typeof (Affiliation), fields[1]);
            Role = (Role)Enum.Parse(typeof(Role), fields[2]);
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}", Jid.Bare, Affiliation, Role);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public bool Equals(MucRoomMemberInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(other.Jid.Bare, Jid.Bare);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return (Jid != null ? Jid.GetHashCode() : 0);
        }

        public static bool operator ==(MucRoomMemberInfo left, MucRoomMemberInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MucRoomMemberInfo left, MucRoomMemberInfo right)
        {
            return !Equals(left, right);
        }
    }
}