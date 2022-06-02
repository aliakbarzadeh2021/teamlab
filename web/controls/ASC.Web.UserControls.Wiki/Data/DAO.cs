using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Reflection;

using ASC.Common.Data;
using ASC.Core.Tenants;
using System.Data.SQLite;
using System.Data.Common;

namespace ASC.Web.UserControls.Wiki.Data
{
	public abstract class DAO : IDisposable
	{
		private DbManager dbManager = null;
		public DbManager DbManager
		{
			get
			{
				return dbManager;
			}
		}

		private IDbConnection connection = null;
		public IDbConnection Connection
		{
			get
			{
				return connection;
			}
		}


		protected string connectionStringName = string.Empty;
		public string ConnectionStringName
		{
			set
			{
				connectionStringName = value;
			}
		}


		public int Tenant { get; set; }


		public void InitDAO(int TenantId)
		{

			if (string.IsNullOrEmpty(connectionStringName))
			{
				throw new Exception("ConnectionStringName is Empty");
			}

			dbManager = DbManager.FromHttpContext(connectionStringName);

			Tenant = TenantId;
			connection = dbManager.Connection;

			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}

			if (!m_SchemaChecked.ContainsKey(Tenant) || !m_SchemaChecked[Tenant])
			{
				CreateSchema();
				m_SchemaChecked[Tenant] = true;
			}

		}


		protected abstract void CreateSchema();

		private static Dictionary<int, bool> m_SchemaChecked = new Dictionary<int, bool>();


		protected List<T> ApplyReaderList<T>(IDataReader reader) where T : new()
		{
			List<T> result = new List<T>();
			T item;
			while (reader.Read())
			{
				item = ApplyReader<T>(reader);
				if (item != null)
					result.Add(item);
			}
			reader.Dispose();
			return result;
		}

		protected T ApplyReaderItem<T>(IDataReader reader) where T : new()
		{
			T item = default(T);
			if (reader.Read())
			{
				item = ApplyReader<T>(reader);
			}

			reader.Dispose();
			return item;
		}


		protected bool ExecuteNonQueryWithTransaction(IDbCommand cmd)
		{
			IDbTransaction transaction = Connection.BeginTransaction();
			try
			{
				cmd.Transaction = transaction;
				cmd.ExecuteNonQuery();
			}
			catch
			{
				transaction.Rollback();
				return false;
			}

			transaction.Commit();
			return true;
		}

		private DbProviderFactory _dbProviderFactory = null;

		private DbProviderFactory m_dbProviderFactory
		{
			get
			{
				if (_dbProviderFactory == null)
				{
					_dbProviderFactory = DbRegistry.GetDbProviderFactory(dbManager.DatabaseId);
				}
				return _dbProviderFactory;
			}
		}

        protected static bool? _IsSQLiteDb = null;

		protected bool IsSQLiteDb
		{
			get
			{
                if(_IsSQLiteDb == null)
                {
                    _IsSQLiteDb = m_dbProviderFactory.GetType().Name.Equals("SQLiteFactory", StringComparison.CurrentCultureIgnoreCase);
                }
                return _IsSQLiteDb.Value;				
			}
		}

		private T ApplyReader<T>(IDataReader reader) where T : new()
		{
			var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);
			var result = new T();
			var settedValue = false;

			foreach (var property in properties)
			{
				if (!reader.Exists(property.Name)) continue;

				var value = reader[property.Name];

				if (property.PropertyType.Equals(typeof(Guid)))
				{
					try
					{
						value = new Guid(value.ToString());
					}
					catch
					{
						value = null;
					}
				}
				else if (property.PropertyType.Equals(typeof(bool)))
				{
					try
					{
						value = Convert.ToBoolean(value);
					}
					catch
					{
						value = false;
					}
				}
				else if (property.PropertyType.Equals(typeof(DateTime)))
				{
					try
					{
						value = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(value));
					}
					catch
					{
						value = TenantUtil.DateTimeNow();
					}
				}
				else if (property.PropertyType.Equals(typeof(int)))
				{
					try
					{
						value = Convert.ToInt32(value);
					}
					catch
					{
						value = null;
					}
				}

				if (value != null)
				{
					property.SetValue(result, value, null);
					settedValue = true;
				}
			}

			return settedValue ? result : default(T);
		}

		#region IDisposable Members

		public void Dispose()
		{

		}

		#endregion
	}

	public static class DbExtensions
	{
		public static IDbDataParameter CreateParameter(this IDbCommand cmd, string ParamName, DbType type)
		{
			IDbDataParameter result = cmd.CreateParameter();
			result.ParameterName = ParamName;
			result.DbType = type;
			return result;
		}

		public static IDbDataParameter CreateParameter(this IDbCommand cmd, string ParamName, DbType type, int size)
		{
			IDbDataParameter result = cmd.CreateParameter(ParamName, type);
			result.Size = size;
			return result;
		}

		public static IDbCommand CreateCommand(this IDbConnection conn, int TenantId)
		{
			IDbCommand result = conn.CreateCommand();

			if (TenantId >= 0)
			{
				result.Parameters.Add(result.CreateParameter("@Tenant", DbType.Int32));
				((IDbDataParameter)result.Parameters["@Tenant"]).Value = TenantId;
			}

			return result;
		}
	}
}
