namespace ASC.PhotoManager.Model
{
	#region

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using ASC.Notify;
	using ASC.PhotoManager.Service;
	using Core;
	using Core.Users;
	using Data;
	using Helpers;
	using NHibernate;
	using NHibernate.Expression;
	using ASC.Notify.Model;

	#endregion

	class ImageStorage : IImageStorage
	{
		#region Constructor

		public ImageStorage()
		{
			NotifySource = new PhotoManagerNotifySource();
			NotifyClient = WorkContext.NotifyContext.NotifyService.RegisterClient(NotifySource);
		}

		#endregion

		#region Properties

		public INotifyClient NotifyClient
		{
			get;
			private set;
		}

		public INotifySource NotifySource
		{
			get;
			private set;
		}
		protected ISession NHibernateSession
		{
			get
			{
				return NHibernateHelper.Instance.GetSession();
			}
		}

		#endregion

		#region Methods

		public bool CreateObject<T>(T obj)
		{
			bool result = false;
			ITransaction tr = NHibernateSession.BeginTransaction();
			object res = null;
			try
			{
				res = NHibernateSession.Save(obj);
				tr.Commit();
				result = true;
			}
			catch (Exception)
			{
				tr.Rollback();
			}
			return result;
		}
		public bool SaveObject<T>(T obj)
		{
			ITransaction tr = NHibernateSession.BeginTransaction();

			bool result = false;

			try
			{
				NHibernateSession.SaveOrUpdate(obj);
				tr.Commit();
				result = true;
			}
			catch
			{
				tr.Rollback();
			}
			return result;
		}

		public bool SaveSettingsFor<T>(T settings, Guid userID) where T : ISettings
		{
			byte[] data = ASC.Runtime.Serialization.BinarySerializer.Instance.Serialize(settings);

			ICriteria criteria = NHibernateSession.CreateCriteria(typeof(Settings));
			Settings settingsDao = criteria.Add(Expression.Eq("ID", settings.ID)).Add(Expression.Eq("UserID", userID)).UniqueResult<Settings>();

			if (settingsDao == null)
			{
				settingsDao = new Settings() { ID = settings.ID, Data = data, UserID = userID };
				return CreateObject<Settings>(settingsDao);
			}

			settingsDao.Data = data;
			return SaveObject<Settings>(settingsDao);
		}

		public T LoadSettingsFor<T>(Guid userID) where T : ISettings
		{
			T defaultSettings = (T)Activator.CreateInstance<T>().GetDefault();

			ICriteria criteria = NHibernateSession.CreateCriteria(typeof(Settings));
			Settings settingsDao = criteria.Add(Expression.Eq("ID", defaultSettings.ID)).Add(Expression.Eq("UserID", userID)).UniqueResult<Settings>();

			if (settingsDao == null || settingsDao.Data == null)
				return defaultSettings;

			return (T)ASC.Runtime.Serialization.BinarySerializer.Instance.Deserialize(settingsDao.Data);
		}


		public Comment GetCommentByID(string id)
		{
			ISession session = NHibernateSession;
			Comment comment;
			comment = (Comment)session.Load(typeof(Comment), Convert.ToInt64(id));

			return comment;
		}
		public Album GetAlbum(long id)
		{
			ISession session = NHibernateSession;
			Album album;

			album = (Album)session.Load(typeof(Album), id);

			return album;
		}
		public Album GetAlbumByID(string id)
		{
			try { return GetAlbum(Convert.ToInt64(id)); }
			catch { return null; }
		}

		public IList<Album> GetAlbums(IList<string> IDs)
		{
			return NHibernateSession.CreateQuery("select album from Album album where album.AlbumID in (:ids)").SetParameterList("ids", IDs).List<Album>() as IList<Album>;
		}
		public void SaveAlbum(Album album)
		{
			ISession session = NHibernateSession;

			ITransaction tx = null;
			tx = session.BeginTransaction();

			try
			{
				session.SaveOrUpdate(album);
				tx.Commit();
			}
			catch (Exception)
			{
				if (tx != null)
				{
					tx.Rollback();
				}
			}
		}

		public Event GetEventByID(string id)
		{
			return GetEvent(Convert.ToInt64(id));
		}

		public Event GetEvent(long id)
		{
			return (Event)NHibernateSession.Load(typeof(Event), id);
		}
		public List<Event> GetEvents(int offset, int count)
		{
			return (List<Event>)NHibernateSession.
				//CreateQuery("select e from Event e left join Album a where e = a.Event and a.Items.size > 0 group by e order by e.Timestamp desc").
				CreateQuery("select e from Event e, Album a where e = a.Event group by e order by e.Timestamp desc").
				SetFirstResult(offset).
				SetMaxResults(count).
				List<Event>();
		}

		public long GetEventPhotoCount(long eventID)
		{
			return (long)NHibernateSession.
					CreateQuery("select count(*) from AlbumItem as i where i.Album.Event.EventID = :event").
					SetInt64("event", eventID).UniqueResult();
		}

		public long GetAlbumCommentsCount(long albumID)
		{
			return (long)NHibernateSession.
					CreateQuery("select count(*) from Comment as r where r.Item.Album.AlbumID = :album").
					SetInt64("album", albumID).UniqueResult();

		}

		public DateTime GetAlbumItemsLastUpdate(long albumID)
		{
			return (DateTime)NHibernateSession.
					CreateQuery("select max(a.Timestamp) from AlbumItem as a where a.Album.AlbumID = :album").
					SetInt64("album", albumID).UniqueResult();

		}

		public long GetAlbumViewsCount(long albumID)
		{
			try
			{
				IDbCommand updateCommand = NHibernateSession.Connection.CreateCommand();
				updateCommand.CommandText = @"select count(*) from (select distinct r.UserID, r.AlbumItemId from Reviews r, AlbumItems a where r.AlbumItemId = a.Id and a.AlbumID = " + albumID + ") b ";

				updateCommand.Prepare();
				return (long)updateCommand.ExecuteScalar();
			}
			catch
			{
				return -1;
			}

		}

		public long GetAlbumItemViewsCount(long imageID)
		{
			var command = NHibernateSession.Connection.CreateCommand();
			command.CommandText = "select count(distinct UserID) from Reviews where AlbumItemId = " + imageID.ToString();
			return (long)command.ExecuteScalar();

		}
		public long GetAlbumItemCommentsCount(long imageID)
		{
			return (long)NHibernateSession.
					CreateQuery("select count(*) from Comment as c where c.Item.Id = :image").
					SetInt64("image", imageID).UniqueResult();

		}

		public IList<Event> GetAllEvents(int maxCount)
		{
			return NHibernateSession.
				CreateQuery("select event from Event as event order by Timestamp desc").
				SetMaxResults(maxCount).List<Event>() as IList<Event>;
		}

		public IList<Event> GetEvents(string userid)
		{
			if (!string.IsNullOrEmpty(userid))
			{
				return NHibernateSession.
					CreateQuery("select event from Event as event left join event.Albums album where album.UserID = :userid and event.Albums.size > 0 group by event order by event.Timestamp desc").
					SetString("userid", userid).
					List<Event>() as IList<Event>;
			}
			else
			{
				return NHibernateSession.
					CreateQuery("select event from Event as event where event.Albums.size > 0 order by Timestamp desc").
					List<Event>() as IList<Event>;
			}
		}

		public IList<Album> GetEventAlbums(string eventID, int from, int count)
		{
			return NHibernateSession.
				CreateQuery("select album from Album as album where album.Event.EventID = :eventid and album.Items.size > 0").
				SetFirstResult(from).SetMaxResults(count).SetString("eventid", eventID).List<Album>() as IList<Album>;
		}

		public IList<Album> GetEventAlbums(long eventID)
		{
			return NHibernateSession.
				CreateQuery("select album from Album as album where album.Event.EventID = :eventid").
				SetInt64("eventid", eventID).List<Album>() as IList<Album>;
		}

		public long GetEventAlbumsCount(long eventID)
		{
			return (long)NHibernateSession.
				CreateQuery("select count(album) from Album as album where album.Event.EventID = :eventid and album.Items.size > 0").
				SetInt64("eventid", eventID).UniqueResult();
		}

		public long GetEventsCount()
		{
			var command = NHibernateSession.Connection.CreateCommand();
			command.CommandText = "select count(distinct EventID) from Albums";
			return (long)command.ExecuteScalar();
		}

		public void SaveEvent(Event Event)
		{
			ISession session = NHibernateSession;

			ITransaction tx = null;
			tx = session.BeginTransaction();

			try
			{
				session.SaveOrUpdate(Event);
				tx.Commit();
			}
			catch (Exception)
			{
				if (tx != null)
				{
					tx.Rollback();
				}
			}
		}
		public void SaveAlbumItem(AlbumItem image)
		{
			ISession session = NHibernateSession;

			ITransaction tx = null;
			tx = session.BeginTransaction();

			try
			{
				if (image.Id == 0) image.Timestamp = DateTime.Now;
				session.SaveOrUpdate(image);
				tx.Commit();
			}
			catch (Exception)
			{
				if (tx != null)
				{
					tx.Rollback();
				}
			}
		}

		public AlbumItem GetAlbumItem(long id)
		{
			ISession session = NHibernateSession;

			var image = (AlbumItem)session.Load(typeof(AlbumItem), id);

			return image;
		}

		public List<AlbumItem> GetAlbumItemsByIDs(IList<string> ids)
		{
			return (List<AlbumItem>)NHibernateSession.
				CreateQuery(@"select a from AlbumItem a where a.Id in (:ids) order by a.ActionDate, a.Name").
				SetParameterList("ids", ids).List<AlbumItem>();
		}

		public Comment GetComment(long id)
		{
			return (Comment)NHibernateSession.Load(typeof(Comment), id);
		}
		public long SaveComment(Comment comment)
		{
			ISession session = NHibernateSession;

			ITransaction tx = null;
			tx = session.BeginTransaction();

			try
			{
				session.SaveOrUpdate(comment);
				tx.Commit();
			}
			catch (Exception)
			{
				if (tx != null)
				{
					tx.Rollback();
				}
			}
			return 0;
		}

		public void RemoveComment(long commentId)
		{
			ISession session = NHibernateSession;

			ITransaction tx = null;
			tx = session.BeginTransaction();

			try
			{
				var comment = session.Get<Comment>(commentId);
				if (comment != null) session.Delete(comment);
				tx.Commit();
			}
			catch (Exception)
			{
				if (tx != null)
				{
					tx.Rollback();
				}
			}
		}

		public IList<AlbumItem> GetLastCommented(int count)
		{
			if (count == 0)
			{
				return NHibernateSession.
					   CreateQuery("select a from AlbumItem a, Comment c where c.Item.Id = a.Id group by a order by max(c.Timestamp) desc").
					   List<AlbumItem>() as IList<AlbumItem>;
			}
			else
			{
				return NHibernateSession.
					CreateQuery("select a from AlbumItem a, Comment c where c.Item.Id = a.Id group by a order by max(c.Timestamp) desc").
					SetMaxResults(count).List<AlbumItem>() as IList<AlbumItem>;
			}
		}

		public long GetAlbumItemsLastCommentedCount()
		{
			return (long)NHibernateSession.
				CreateQuery("select count(distinct a) from AlbumItem a, Comment c where c.Item.Id = a.Id").
				UniqueResult();
		}

		public List<AlbumItem> GetAlbumItemsLastCommented(int start, int count)
		{
			return (List<AlbumItem>)NHibernateSession.
				CreateQuery("select a from AlbumItem a, Comment c where c.Item.Id = a.Id group by a order by max(c.Timestamp) desc").
				SetFirstResult(start).SetMaxResults(count).List<AlbumItem>();
		}

		public bool SaveAlbumFace(long albumID, long imageID)
		{

			ITransaction tx = NHibernateSession.BeginTransaction();
			try
			{
				IDbCommand updateCommand = NHibernateSession.Connection.CreateCommand();
				updateCommand.CommandText = @"update AlbumItems
                                              set IsAlbumFace = 0 where AlbumID = " + albumID;

				updateCommand.Prepare();
				updateCommand.ExecuteNonQuery();


				updateCommand.CommandText = @"update AlbumItems
                                              set IsAlbumFace = 1 where Id = " + imageID;

				updateCommand.Prepare();
				updateCommand.ExecuteNonQuery();


				tx.Commit();
				return true;
			}
			catch
			{
				tx.Rollback();
				return false;
			}

		}

		public long GetAlbumsCount(long eventId, string userId)
		{
			var sql = "select count(album) from Album as album where album.Items.size > 0";
			if (eventId != 0) sql += " and album.Event.EventID = :eventid";
			if (!string.IsNullOrEmpty(userId)) sql += " and album.UserID = :userid";

			var query = NHibernateSession.CreateQuery(sql);

			if (eventId != 0) query.SetInt64("eventid", eventId);
			if (!string.IsNullOrEmpty(userId)) query.SetString("userid", userId);

			return query.UniqueResult<long>();
		}
		public IEnumerable<Album> GetAlbums(int from, int count, bool filled)
		{
			ISession session = NHibernateSession;

			List<Album> categories = new List<Album>();

			string filter = string.Empty;
			if (filled)
			{
				filter = " where album.Items.size > 0 ";
			}

			IQuery query = session.CreateQuery("select album from Album as album " + filter + " order by Timestamp desc ");
			query.SetFirstResult(from);
			query.SetMaxResults(count);

			foreach (Album album in query.Enumerable())
			{
				NHibernateUtil.Initialize(album);
				categories.Add(album);
			}

			return categories;
		}

		public List<Album> GetAlbums(long eventId, string userId)
		{
			var sql = "select album from Album as album where album.Items.size > 0";
			if (eventId != 0) sql += " and album.Event.EventID = :eventid";
			if (!string.IsNullOrEmpty(userId)) sql += " and album.UserID = :userid";

			var query = NHibernateSession.CreateQuery(sql);

			if (eventId != 0) query.SetInt64("eventid", eventId);
			if (!string.IsNullOrEmpty(userId)) query.SetString("userid", userId);

			return (List<Album>)query.List<Album>();
		}

		public List<Album> GetLastAlbums(int count)
		{
			IQuery query = NHibernateSession.CreateQuery("select a.Album from AlbumItem a group by a.Album.AlbumID order by max(a.Timestamp) desc ");

			if (count > 0)
				query.SetMaxResults(count);

			return (List<Album>)query.List<Album>();
		}

		public IEnumerable<UserInfo> GetUsers()
		{
			return CoreContext.UserManager.GetUsers();
		}

		public string UserFullName(string id)
		{
			UserInfo user = CoreContext.UserManager.GetUsers(new Guid(id));

			return user.FirstName + " " + user.LastName;
		}

		public IList<Album> GetUserAlbums(UserInfo user)
		{
			return GetUserAlbums(user.ID);
		}

		public IList<Album> GetUserAlbums(Guid userID)
		{
			return GetUserAlbums(userID.ToString());
		}
		public void RemoveAlbum(long albumId)
		{
			ISession session = NHibernateSession;
			ITransaction tx = null;
			tx = session.BeginTransaction();
			try
			{
				var album = session.Get<Album>(albumId);
				if (album != null) session.Delete(album);
				tx.Commit();
			}
			catch (Exception)
			{
				if (tx != null)
				{
					tx.Rollback();
				}
				throw;
			}
		}
		public void RemoveEvent(long id)
		{
			ISession session = NHibernateSession;
			ITransaction tx = null;
			tx = session.BeginTransaction();
			try
			{
				var ev = session.Get<Event>(id);
				if (ev != null) session.Delete(ev);
				tx.Commit();
			}
			catch (Exception)
			{
				if (tx != null)
				{
					tx.Rollback();
				}
				throw;
			}
		}

		public IEnumerable<Comment> GetAlbumItemsComments(AlbumItem item)
		{
			return GetAlbumItemsComments(item.Id);
		}

		public IEnumerable<Comment> GetAlbumItemsComments(long itemID)
		{
			ISession session = NHibernateSession;

			List<Comment> comments = new List<Comment>();

			IQuery query =
				session.CreateQuery(
					"select albumitem from AlbumItem as albumitem where albumitem.Id = :albumid");
			query.SetInt64("albumid", itemID);

			foreach (AlbumItem albumItem in query.Enumerable())
			{
				NHibernateUtil.Initialize(albumItem.ExpandedStore);
			}
			return comments;
		}

		public IList<AlbumItem> GetAllImages()
		{
			return NHibernateSession.CreateQuery("select a from AlbumItem as a").List<AlbumItem>() as IList<AlbumItem>;
		}

		public void AddAlbumItemComment(AlbumItem item, Comment comment)
		{
			ISession session = NHibernateSession;

			ITransaction tx = null;
			tx = session.BeginTransaction();

			try
			{
				comment.ItemID = item.Id;
				comment.Timestamp = DateTime.Now;

				session.SaveOrUpdate(item);
				tx.Commit();


			}
			catch (Exception)
			{
				if (tx != null)
				{
					tx.Rollback();
				}

				throw;
			}
		}

		public IList<Album> GetUserAlbums(string userID)
		{
			ISession session = NHibernateSession;

			IList<Album> albums = new List<Album>();

			IQuery query;
			if (userID != null)
			{
				query =
					session.CreateQuery("select album from Album as album where album.UserID = :userid");
				query.SetString("userid", userID);
			}
			else
			{
				query = session.CreateQuery("select album from Album as album where album.UserID is null");
			}


			albums = query.List<Album>() as IList<Album>;

			return albums;
		}

		public IEnumerable<AlbumItem> GetAlbumItems(Album album)
		{
			return GetAlbumItems(album.Id);
		}

		public IEnumerable<AlbumItem> GetAlbumItems(string albumID)
		{
			return GetAlbumItems(Convert.ToInt64(albumID));
		}

		public List<AlbumItem> GetAlbumItems(Int64 albumID)
		{
			var albumItems = new List<AlbumItem>();
			var query = NHibernateSession.CreateQuery(
				"select albumitem from AlbumItem as albumitem where albumitem.Album.AlbumID = :albumid order by albumitem.ActionDate, albumitem.Name")
				.SetInt64("albumid", albumID);

			foreach (var albumItem in query.List<AlbumItem>())
			{
				NHibernateUtil.Initialize(albumItem.ExpandedStore);
				albumItems.Add(albumItem);
			}
			return albumItems;
		}

		public IEnumerable<AlbumItem> GetAlbumItems(int start, int count, string albumID)
		{
			return GetAlbumItems(start, count, Convert.ToInt64(albumID));
		}

		public Album GetEventAlbumByAuthor(long eventID, string authorID)
		{
			try
			{
				return NHibernateSession.CreateQuery("select a from Album as a where a.Event.EventID = :eventid and a.UserID = :author and a.AlbumCaption is null").
					SetString("author", authorID).SetInt64("eventid", eventID).UniqueResult<Album>();
			}
			catch
			{
				return null;
			}
		}

		public IEnumerable<AlbumItem> GetAlbumItems(int start, int count, Int64 albumID)
		{
			ISession session = NHibernateSession;
			List<AlbumItem> albumItems = new List<AlbumItem>();
			IQuery query =
				session.CreateQuery(
					"select albumitem from AlbumItem as albumitem where albumitem.Album.AlbumID = :albumid order by albumitem.ActionDate, albumitem.Name ");
			query.SetFirstResult(start).SetMaxResults(count).SetInt64("albumid", albumID);

			foreach (AlbumItem albumItem in query.Enumerable())
			{
				NHibernateUtil.Initialize(albumItem.ExpandedStore);
				albumItems.Add(albumItem);
			}

			return albumItems;
		}

		public long GetAlbumItemsCount(long albumID)
		{
			ISession session = NHibernateSession;
			long result = 0;
			IQuery query = session.CreateQuery(
					"select count(albumitem) from AlbumItem as albumitem where AlbumID = :albumid");

			query.SetInt64("albumid", albumID);

			result = (long)query.UniqueResult();

			return result;
		}

		public long GetAlbumReviewsCount(long albumID)
		{
			ISession session = NHibernateSession;
			long result = 0;
			IQuery query = session.CreateQuery(
					"select count(r) from Review r where r.AlbumItem.Album.AlbumID = :albumid");

			query.SetInt64("albumid", albumID);

			result = (long)query.UniqueResult();

			return result;
		}

		public void AddAlbum(Album album)
		{
			ISession session = NHibernateSession;
			ITransaction tx = null;
			tx = session.BeginTransaction();
			try
			{
				session.SaveOrUpdate(album);
				tx.Commit();
			}
			catch (Exception)
			{
				if (tx != null)
				{
					tx.Rollback();
				}

				throw;
			}
		}

		public void AddAlbumItem(AlbumItem albumItem)
		{
			ISession session = NHibernateSession;

			ITransaction tx = null;
			tx = session.BeginTransaction();
			try
			{
				albumItem.Timestamp = DateTime.Now;
				session.SaveOrUpdate(albumItem);

				tx.Commit();
			}
			catch (Exception)
			{
				if (tx != null)
				{
					tx.Rollback();
				}
				throw;
			}
		}

		public long GetItemsCountByTag(string tag, string userID, string albumID)
		{
			ISession session = NHibernateSession;
			List<AlbumItem> albumItems = new List<AlbumItem>();
			long result = 0;
			string filter = string.Empty;

			if (!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(userID))
			{
				filter = " where albumitem.Id in (select tag.AlbumItem.Id from Tag as tag where tag.TagText = :tagtext and tag.UserID = :userid)";
			}
			if (!string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(userID))
			{
				filter = " where albumitem.Id in (select tag.AlbumItem.Id from Tag as tag where tag.TagText = :tagtext)";
			}
			if (!string.IsNullOrEmpty(albumID))
			{
				filter += " and albumitem.Album.AlbumID = :album ";
			}
			IQuery query = session.CreateQuery(
					"select count(albumitem) from AlbumItem as albumitem " + filter);

			query.SetString("tagtext", tag);

			if (!string.IsNullOrEmpty(userID))
			{
				query.SetString("userid", userID);
			}
			if (!string.IsNullOrEmpty(albumID))
			{
				query.SetInt64("album", Convert.ToInt64(albumID));
			}
			result = (long)query.UniqueResult();

			return result;
		}

		public IEnumerable<AlbumItem> GetItemsByTag(string tag, string userID, string albumID, int from, int count)
		{
			ISession session = NHibernateSession;
			List<AlbumItem> albumItems = new List<AlbumItem>();
			string filter = string.Empty;

			if (!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(userID))
			{
				filter = " where albumitem.Id in (select tag.AlbumItem.Id from Tag as tag where tag.TagText = :tagtext and tag.UserID = :userid)";
			}
			if (!string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(userID))
			{
				filter = " where albumitem.Id in (select tag.AlbumItem.Id from Tag as tag where tag.TagText = :tagtext)";
			}

			if (!string.IsNullOrEmpty(albumID))
			{
				filter += " and albumitem.Album.AlbumID = :album ";
			}
			IQuery query = session.CreateQuery(
					"select albumitem from AlbumItem as albumitem " + filter);

			query.SetString("tagtext", tag);

			if (!string.IsNullOrEmpty(userID))
			{
				query.SetString("userid", userID);
			}
			if (!string.IsNullOrEmpty(albumID))
			{
				query.SetInt64("album", Convert.ToInt64(albumID));
			}

			query.SetMaxResults(count);
			query.SetFirstResult(from);

			foreach (AlbumItem albumItem in query.Enumerable())
			{
				NHibernateUtil.Initialize(albumItem.ExpandedStore);
				albumItems.Add(albumItem);
			}
			return albumItems;
		}

        public IList<Event> GetEventsByTag(string tag, string userID)
		{
			ISession session = NHibernateSession;
			string filter = string.Empty;

			if (!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(userID))
			{
				filter = " select albumitem.Album.Event.EventID from AlbumItem albumitem where albumitem.Id in (select tag.AlbumItem.Id from Tag as tag where tag.TagText = :tagtext and tag.UserID = :userid)";
			}
			if (!string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(userID))
			{
				filter = " select albumitem.Album.Event.EventID from AlbumItem albumitem where albumitem.Id in (select tag.AlbumItem.Id from Tag as tag where tag.TagText = :tagtext)";
			}

			IQuery query = session.CreateQuery(
					"select event from Event as event where event.EventID in (" + filter + ") order by Timestamp desc");

			query.SetString("tagtext", tag);

			if (!string.IsNullOrEmpty(userID))
			{
				query.SetString("userid", userID);
			}

			return query.List<Event>() as IList<Event>;

		}

		public IList<Album> GetEventAlbumsByUser(string eventID, string userID)
		{
			return NHibernateSession.CreateQuery(
					"select album from Album as album where album.UserID = :userid and album.Event.EventID = :eventid and album.Items.size > 0").
					SetString("userid", userID).SetString("eventid", eventID).List<Album>() as IList<Album>;

		}

        public long GetEventAlbumsCountByUser(string eventID, string userID)
		{
			return (long)NHibernateSession.CreateQuery(
					"select count(album) from Album as album where album.UserID = :userid and album.Event.EventID = :eventid and album.Items.size > 0").
					SetString("userid", userID).SetString("eventid", eventID).UniqueResult();

		}

        public IList<Album> GetEventAlbumsByUser(string eventID, string userID, int start, int count)
		{
			return NHibernateSession.CreateQuery(
					"select album from Album as album where album.UserID = :userid and album.Event.EventID = :eventid and album.Items.size > 0").
					SetString("userid", userID).
					SetString("eventid", eventID).
					SetFirstResult(start).
					SetMaxResults(count).
					List<Album>() as IList<Album>;

		}


        public List<Album> GetAlbums(long eventId, string userId, string tag)
		{
			ISession session = NHibernateSession;
			string filter = string.Empty;

			if (!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(userId))
			{
				filter = " select albumitem.Album.AlbumID from AlbumItem albumitem where albumitem.Id in (select tag.AlbumItem.Id from Tag as tag where tag.TagText = :tagtext and tag.UserID = :userid)";
			}
			if (!string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(userId))
			{
				filter = " select albumitem.Album.AlbumID from AlbumItem albumitem where albumitem.Id in (select tag.AlbumItem.Id from Tag as tag where tag.TagText = :tagtext)";
			}

			IQuery query = session.CreateQuery(
					"select album from Album as album where album.AlbumID in (" + filter + ") and album.Event.EventID = :eventid ");

			query.SetString("tagtext", tag).SetInt64("eventid", eventId);

			if (!string.IsNullOrEmpty(userId))
			{
				query.SetString("userid", userId);
			}

			return (List<Album>)query.List<Album>();
		}

		public IList<Album> GetAlbumsByTag(string tag, string userID)
		{
			ISession session = NHibernateSession;
			string filter = string.Empty;

			if (!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(userID))
			{
				filter = " select albumitem.Album.AlbumID from AlbumItem albumitem where albumitem.Id in (select tag.AlbumItem.Id from Tag as tag where tag.TagText = :tagtext and tag.UserID = :userid)";
			}
			if (!string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(userID))
			{
				filter = " select albumitem.Album.AlbumID from AlbumItem albumitem where albumitem.Id in (select tag.AlbumItem.Id from Tag as tag where tag.TagText = :tagtext)";
			}

			IQuery query = session.CreateQuery(
					"select album from Album as album where album.AlbumID in (" + filter + ")");

			query.SetString("tagtext", tag);

			if (!string.IsNullOrEmpty(userID))
			{
				query.SetString("userid", userID);
			}

			return query.List<Album>() as IList<Album>;
		}

		public IEnumerable<AlbumItem> GetLastItems(int start, int count, string userid)
		{
			ISession session = NHibernateSession;
			List<AlbumItem> albumItems = new List<AlbumItem>();
			IQuery query = null;
			if (!String.IsNullOrEmpty(userid))
			{
				query = session.CreateQuery(
					"select albumitem from AlbumItem as albumitem where albumitem.Album.UserID = :usrid order by albumitem.Timestamp desc");
				query.SetString("usrid", userid);
			}
			else
			{
				query = session.CreateQuery(
					"select albumitem from AlbumItem as albumitem order by albumitem.Timestamp desc");
			}
			query.SetFirstResult(start).SetMaxResults(count);

			foreach (AlbumItem albumItem in query.Enumerable())
			{
				NHibernateUtil.Initialize(albumItem.ExpandedStore);
				albumItems.Add(albumItem);
			}

			return albumItems;
		}

		public List<AlbumItem> SearchAlbumItems(string text)
		{
			return (List<AlbumItem>)NHibernateSession
				.CreateQuery("select i from AlbumItem as i left join i.Comments as c where i.Name like :search or c.CommentText like :search group by i.Id")
				.SetString("search", "%" + text + "%")
				.List<AlbumItem>();
		}

		public long GetCountImages()
		{
			ISession session = NHibernateSession;
			List<AlbumItem> albumItems = new List<AlbumItem>();
			long result = 0;

			IQuery query = session.CreateQuery(
					"select count(albumitem) from AlbumItem as albumitem");
			result = (long)query.UniqueResult();

			return result;
		}



		public DateTime GetLastUserUpdateDate(string userid)
		{
			ISession session = NHibernateSession;
			DateTime result = new DateTime();
			IQuery query = session.
					CreateQuery("select max(albumitem.Timestamp) from AlbumItem as albumitem where albumitem.UserID = :usrid");
			query.SetString("usrid", userid);

			object obj = query.UniqueResult();
			if (obj != null)
				result = (DateTime)obj;

			return result;
		}

		public long GetCountAlbumItemsByUser(string userid)
		{
			ISession session = NHibernateSession;
			List<AlbumItem> albumItems = new List<AlbumItem>();
			long result = 0;
			IQuery query = session.CreateQuery(
					"select count(albumitem) from AlbumItem as albumitem where albumitem.Album.UserID = :usrid");
			query.SetString("usrid", userid);

			result = (long)query.UniqueResult();


			return result;
		}

		public void CommitChanges()
		{
			if (NHibernateHelper.Instance.HasOpenTransaction())
			{
				NHibernateHelper.Instance.CommitTransaction();
			}
			else
			{
				// If there's no transaction, just flush the changes
				NHibernateHelper.Instance.GetSession().Flush();
			}
		}

		public void CloseSession()
		{
			NHibernateHelper.Instance.CloseSession();
		}

		public void ReadAlbum(long albumId, string userId)
		{
		}

		public void ReadAlbumItem(long albumItemId, string userId)
		{
		}

		public List<Comment> GetComments(long albumItemId)
		{
			throw new NotImplementedException();
		}

		public void RemoveAlbumItem(long albumItemId)
		{

		}

		public AlbumItem GetAlbumItem(long albumId, int index)
		{
			return null;
		}

		#endregion
	}
}