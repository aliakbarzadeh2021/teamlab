using System;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Data.Sql;
using System.Data.SQLite;
using ASC.Web.UserControls.Wiki.Resources;
using ASC.Common.Data;
using ASC.Core.Tenants;
using System.Text.RegularExpressions;
using System.Text;
using ASC.FullTextIndex;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;

namespace ASC.Web.UserControls.Wiki.Data
{

	public class WikiDAO : DAO
	{
		#region SQLQuery
		private static readonly string cmd_pagesGetAllWithBody = @"select wp.*, wph.UserID as OwnerID from wiki_pages wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1 where wp.Tenant = @Tenant order by wp.PageName";
		private static readonly string cmd_pagesGetAll = @"select wp.Tenant, wp.PageName, wp.Version, wp.UserID, wp.Date, '' as Body, wph.UserID as OwnerID from wiki_pages wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1 where wp.Tenant = @Tenant order by wp.PageName";
		private static readonly string cmd_pagesGetTopNew = @"select wp.Tenant, wp.PageName, wph.Version, wph.UserID, wph.Date, '' as Body, wph.UserID as OwnerID from wiki_pages wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1 where wp.Tenant = @Tenant order by wph.Date desc";
		private static readonly string cmd_pagesGetTopNewAndUserCreated = @"select B.Tenant, B.PageName, B.Version, B.UserID, B.Date, '' as Body from wiki_pages A inner join wiki_pages_history B on A.PageName = B.PageName and A.Tenant = B.Tenant and B.Version = 1 where B.Tenant = @Tenant order by B.Date desc";
		private static readonly string cmd_pagesGetTopFresh = @"select wp.Tenant, wp.PageName, wp.Version, wp.UserID, wp.Date, '' as Body, wph.UserID as OwnerID from wiki_pages wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1 where wp.Tenant = @Tenant order by wp.Date desc";

		private string cmd_pagesGetByStartName
		{
			get
			{
				if (IsSQLiteDb)
				{
					return @"select wp.Tenant, wp.PageName, wp.Version, wp.UserID, wp.Date, '' as Body, wph.UserID as OwnerID from wiki_pages wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1 where Upper(wp.PageName) like Upper(@PageName) || '%' and wp.Tenant = @Tenant order by wp.PageName";
				}

				return @"select wp.Tenant, wp.PageName, wp.Version, wp.UserID, wp.Date, '' as Body, wph.UserID as OwnerID from wiki_pages wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1 where Upper(wp.PageName) like Upper(concat(@PageName, '%')) and wp.Tenant = @Tenant order by wp.PageName";
			}
		}

		private string cmd_pagesSearchByName
		{
			get
			{
				if (IsSQLiteDb)
				{
					return @"select wp.Tenant, wp.PageName, wp.Version, wp.UserID, wp.Date, '' as Body, wph.UserID as OwnerID from wiki_pages wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1 where Upper(wp.PageName) like '%' || Upper(@PageName) || '%' and wp.Tenant = @Tenant order by wp.PageName";
				}

				return @"select wp.Tenant, wp.PageName, wp.Version, wp.UserID, wp.Date, '' as Body, wph.UserID as OwnerID from wiki_pages wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1 where Upper(wp.PageName) like Upper(concat('%', @PageName, '%')) and wp.Tenant = @Tenant order by wp.PageName";
			}
		}

		private static readonly string cmd_pagesGetByCreatedUserId = @"select wp.Tenant, wp.PageName, wp.Version, wp.UserID, wp.Date, '' as Body, wph.UserID as OwnerID from wiki_pages wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1 Where wph.UserID = @UserID and wp.Tenant = @Tenant order by wp.PageName";
		private static readonly string cmd_pagesGetByName = @"select wp.*, wph.UserID as OwnerID from wiki_pages wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1 where wp.PageName = @PageName and wp.Tenant = @Tenant";
		private static readonly string cmd_pagesGetCountByCreatedUserId = @"select count(A.PageName) as PageCount from wiki_pages A inner join wiki_pages_history B on A.PageName = B.PageName and A.Tenant = B.Tenant and B.Version = 1 Where B.UserID = @UserID and A.Tenant = @Tenant";
		private static readonly string cmd_pagesInsert = @"insert into wiki_pages (Tenant, PageName, Version, UserID, Date, Body) values (@Tenant, @PageName, @Version, @UserID, @Date, @Body)";
		private static readonly string cmd_pagesUpdate = @"update wiki_pages set Version=@Version, UserID=@UserID, Date=@Date, Body=@Body where PageName = @PageName and Tenant = @Tenant";
		private static readonly string cmd_pagesDelete = @"delete from wiki_pages where PageName = @PageName and Tenant = @Tenant";
		private static readonly string cmd_filesGetAll = @"select wf.*, wfh.UserID as OwnerID from wiki_files wf inner join wiki_files_history wfh on wf.FileName = wfh.FileName and wf.Tenant = wfh.Tenant and wfh.Version = 1 where wf.Tenant = @Tenant order by wf.FileName";

		private string cmd_filesGetByStartName
		{
			get
			{
				if (IsSQLiteDb)
				{
					return @"select wf.*, wfh.UserID as OwnerID from wiki_files wf inner join wiki_files_history wfh on wf.FileName = wfh.FileName and wf.Tenant = wfh.Tenant and wfh.Version = 1 where Upper(wf.FileName) like Upper(@FileName) || '%' and wf.Tenant = @Tenant order by wf.FileName";
				}

				return @"select wf.*, wfh.UserID as OwnerID from wiki_files wf inner join wiki_files_history wfh on wf.FileName = wfh.FileName and wf.Tenant = wfh.Tenant and wfh.Version = 1 where Upper(wf.FileName) like Upper(concat(@FileName, '%')) and wf.Tenant = @Tenant order by wf.FileName";
			}
		}



		private static readonly string cmd_filesGetByName = @"select wf.*, wfh.UserID as OwnerID from wiki_files wf inner join wiki_files_history wfh on wf.FileName = wfh.FileName and wf.Tenant = wfh.Tenant and wfh.Version = 1 where wf.FileName = @FileName and wf.Tenant = @Tenant";
		private static readonly string cmd_filesGetByUploadName = @"select wf.*, wfh.UserID as OwnerID from wiki_files wf inner join wiki_files_history wfh on wf.FileName = wfh.FileName and wf.Tenant = wfh.Tenant and wfh.Version = 1 where wf.UploadFileName = @UploadFileName and wf.Tenant = @Tenant";
		private static readonly string cmd_filesInsert = @"insert into wiki_files (Tenant, FileName, UploadFileName, FileSize, Version, UserID, Date, FileLocation) values (@Tenant, @FileName, @UploadFileName, @FileSize, @Version, @UserID, @Date, @FileLocation)";
		private static readonly string cmd_filesUpdate = @"update wiki_files set FileSize = @FileSize, Version=@Version, UserID=@UserID, Date=@Date, FileLocation = @FileLocation where FileName = @FileName and Tenant = @Tenant";
		private static readonly string cmd_filesDelete = @"delete from wiki_files where FileName = @FileName and Tenant = @Tenant";

		private static readonly string cmd_pagesGetByList = @"select wp.Tenant, wp.PageName, wp.Version, wp.UserID, wp.Date, '' as Body, wph.UserID as OwnerID from wiki_pages wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1 where wp.Tenant = @Tenant AND wp.PageName in ('{0}') order by wp.PageName";
		private static readonly string cmd_filesGetByList = @"select wf.*, wfh.UserID as OwnerID from wiki_files wf inner join wiki_files_history wfh on wf.FileName = wfh.FileName and wf.Tenant = wfh.Tenant and wfh.Version = 1 where wf.Tenant = @Tenant AND wf.FileName in ('{0}') order by wf.FileName";


		private static readonly string cmd_pagesHistGetAll = @"select wp.Tenant, wp.PageName, wp.Version, wp.UserID, wp.Date, '' as Body, wph.UserID as OwnerID  from wiki_pages_history wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1  where wp.Tenant = @Tenant order by wp.PageName, wp.Version";
		private static readonly string cmd_pagesHistGetAllByName = @"select wp.Tenant, wp.PageName, wp.Version, wp.UserID, wp.Date, '' as Body, wph.UserID as OwnerID  from wiki_pages_history wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1  where wp.PageName = @PageName and wp.Tenant = @Tenant order by  wp.Version desc";

		private static readonly string cmd_pagesHistGetByNameVersion = @"select wp.*, wph.UserID as OwnerID  from wiki_pages_history wp inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1  where wp.PageName = @PageName and wp.Tenant = @Tenant AND wp.Version = @Version";
		private static readonly string cmd_pagesHistInsert = @"insert into wiki_pages_history (Tenant, PageName, Version, UserID, Date, Body) values (@Tenant, @PageName, @Version, @UserID, @Date, @Body)";
		private static readonly string cmd_pagesHistDelete = @"delete from wiki_pages_history where PageName = @PageName and Tenant = @Tenant";
		private static readonly string cmd_pagesHistMaxVersion = @"select max(Version) as MaxVersion from wiki_pages_history where PageName = @PageName and Tenant = @Tenant";
		private static readonly string cmd_filesHistGetAll = @"select wf.*, wfh.UserID as OwnerID from wiki_files_history wf inner join wiki_files_history wfh on wf.FileName = wfh.FileName and wf.Tenant = wfh.Tenant and wfh.Version = 1 where wf.Tenant = @Tenant order by wf.FileName, wf.Version";
		private static readonly string cmd_filesHistGetAllByName = @"select wf.*, wfh.UserID as OwnerID from wiki_files_history wf inner join wiki_files_history wfh on wf.FileName = wfh.FileName and wf.Tenant = wfh.Tenant and wfh.Version = 1 where wf.FileName = @FileName and wf.Tenant = @Tenant order by  wf.Version";
		private static readonly string cmd_filesHistGetByNameVersion = @"select wf.*, wfh.UserID as OwnerID from wiki_files_history wf inner join wiki_files_history wfh on wf.FileName = wfh.FileName and wf.Tenant = wfh.Tenant and wfh.Version = 1 where wf.FileName = @FileName and wf.Tenant = @Tenant  AND wf.Version = @Version";
		private static readonly string cmd_filesHistInsert = @"insert into wiki_files_history (Tenant, FileName, UploadFileName, FileSize, Version, UserID, Date, FileLocation) values (@Tenant, @FileName, @UploadFileName, @FileSize, @Version, @UserID, @Date, @FileLocation)";
		private static readonly string cmd_filesHistDelete = @"delete from wiki_files_history where FileName = @FileName";
		private static readonly string cmd_filesHistMaxVersion = @"select max(Version) as MaxVersion from wiki_files_history where FileName = @FileName and Tenant = @Tenant";


		private static readonly string cmd_pagesGetAllByCategoryName = @"select wp.*, wph.UserID as OwnerID from wiki_pages wp  inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1 inner join wiki_categories wc on wp.PageName = wc.PageName and wp.Tenant = wc.Tenant where wc.CategoryName=@CategoryName and wp.Tenant = @Tenant order by wp.PageName";






		//Comments
		private static readonly string cmd_commentsGetAllByPageName = @"select *, UserId as OwnerID from wiki_comments where PageName=@PageName and Tenant = @Tenant order by Date";
		private static readonly string cmd_commentsGetTopByPageName = @"select *, UserId as OwnerID from wiki_comments where PageName=@PageName and Tenant = @Tenant AND ParentId='00000000-0000-0000-0000-000000000000' order by Date";
		private static readonly string cmd_commentsGetTopByParentId = @"select *, UserId as OwnerID from wiki_comments where ParentId=@ParentId and Tenant = @Tenant order by Date";
		private static readonly string cmd_commentsGetCountAllByPageName = @"select count(*) as CommentCount from wiki_comments where PageName=@PageName and Tenant = @Tenant";
		private static readonly string cmd_commentsGetById = @"select *, UserId as OwnerID from wiki_comments where Id=@Id and Tenant = @Tenant";
		private static readonly string cmd_commentsInsert = @"insert into wiki_comments (Tenant, Id, ParentId, PageName, Body, UserId, Date, Inactive) values (@Tenant, @Id, @ParentId, @PageName, @Body, @UserId, @Date, @Inactive)";
		private static readonly string cmd_commentsUpdate = @"update wiki_comments set ParentId=@ParentId, PageName=@PageName, Body=@Body, UserId=@UserId, Date=@Date, Inactive=@Inactive where Id=@id and Tenant = @Tenant";
		private static readonly string cmd_commentsDeleteById = @"delete from wiki_comments where Id=@Id and Tenant = @Tenant";
		private static readonly string cmd_commentsDeleteByParentId = @"delete from wiki_comments where ParentId=@ParentId and Tenant = @Tenant";
		private static readonly string cmd_commentsDeleteByPageName = @"delete from wiki_comments where PageName=@PageName and Tenant = @Tenant";

		//Categories
		private static readonly string cmd_categoriesGetAll = @"select Tenant, CategoryName, '' as PageName from wiki_categories where Tenant = @Tenant group by CategoryName order by CategoryName";
		private static readonly string cmd_categoriesGetAllByPageName = @"select * from wiki_categories where PageName=@PageName and Tenant = @Tenant";
		private static readonly string cmd_categoriesGetAllByCategoryName = @"select * from wiki_categories where CategoryName=@CategoryName and Tenant = @Tenant";
		private static readonly string cmd_categoriesGetByPageCategoryName = @"select * from wiki_categories where PageName=@PageName AND CategoryName=@CategoryName and Tenant = @Tenant";
		private static readonly string cmd_categoriesGetCountAllByPageName = @"select count(*) as CategoryCount from wiki_categories where PageName=@PageName and Tenant = @Tenant";
		private static readonly string cmd_categoriesGetCountAllByCategoryName = @"select count(*) as CategoryCount from wiki_categories where CategoryName=@CategoryName and Tenant = @Tenant";
		private static readonly string cmd_categoriesInsert = @"insert into wiki_categories (Tenant, PageName, CategoryName) values (@Tenant, @PageName, @CategoryName)";
		private static readonly string cmd_categoriesDeleteAllByPageName = @"delete from wiki_categories where PageName=@PageName and Tenant = @Tenant";
		private static readonly string cmd_categoriesDeleteAllByCategoryName = @"delete from wiki_categories where CategoryName=@CategoryName and Tenant = @Tenant";
		private static readonly string cmd_categoriesDeleteByPageCategoryName = @"delete from wiki_categories where PageName=@PageName AND CategoryName=@CategoryName and Tenant = @Tenant";
		private static readonly string cmd_categoriesSelectCategoriesWillBeDeletedAtAllByPageName = @"select wc.CategoryName from wiki_categories wc inner join (select distinct CategoryName, Tenant from wiki_categories where PageName = @PageName) wcPage on wc.CategoryName = wcPage.CategoryName and  wc.Tenant = wcPage.Tenant where wc.Tenant = @Tenant group by wc.CategoryName having count(wc.CategoryName) = 1";





		private static readonly string[] cmd_schemaDB = new string[]
        {
            @"CREATE TABLE if not exists wiki_pages (Tenant INT NOT NULL DEFAULT 0, PageName varchar(255) NOT NULL, Version int NOT NULL, UserID TEXT NOT NULL, Date DateTime NOT NULL, Body TEXT,PRIMARY KEY(Tenant, PageName));",
            @"CREATE TABLE if not exists wiki_files(Tenant INT NOT NULL DEFAULT 0, FileName varchar(255) NOT NULL,UploadFileName TEXT NOT NULL, FileSize int NOT NULL, Version int NOT NULL, UserID TEXT NOT NULL, Date DateTime NOT NULL, FileLocation TEXT NOT NULL,PRIMARY KEY(Tenant, FileName));",
            @"CREATE TABLE if not exists wiki_pages_history (Tenant INT NOT NULL DEFAULT 0, PageName varchar(255) NOT NULL, Version int NOT NULL, UserID TEXT NOT NULL, Date DateTime NOT NULL, Body TEXT,PRIMARY KEY(Tenant, PageName, Version));",
            @"CREATE TABLE if not exists wiki_files_history (Tenant INT NOT NULL DEFAULT 0, FileName varchar(255) NOT NULL,UploadFileName TEXT NOT NULL, FileSize int NOT NULL, Version int NOT NULL, UserID TEXT NOT NULL, Date DateTime NOT NULL, FileLocation TEXT NOT NULL,PRIMARY KEY(Tenant, FileName, Version));",
            @"CREATE TABLE if not exists wiki_comments (Tenant INT NOT NULL DEFAULT 0, Id char(38) NOT NULL, ParentId TEXT NOT NULL, PageName TEXT NOT NULL, Body TEXT NOT NULL, UserId TEXT NOT NULL, Date DateTime NOT NULL, Inactive int NOT NULL);",
            @"CREATE TABLE if not exists wiki_categories (Tenant INT NOT NULL DEFAULT 0, PageName TEXT NOT NULL, CategoryName TEXT NOT NULL);"
        };
		#endregion


		#region Help Functions

		protected override void CreateSchema()
		{
            //Disable creating of default pages
            //Pages page = PagesGetByNameInternal(string.Empty);
            //if (page == null)
            //{
            //    page = new Pages();
            //    page.PageName = string.Empty;
            //    page.Body = WikiUCResource.MainPage_DefaultBody;
            //    PagesSave(page);
            //}

            //page = PagesGetByNameInternal(WikiUCResource.HelpPageCaption);
            //if (page == null)
            //{
            //    page = new Pages();
            //    page.PageName = WikiUCResource.HelpPageCaption;
            //    page.Body = WikiUCResource.HelpPage_DefaultBody;
            //    PagesSave(page);
            //}
		}

        private Pages GetMainPageFromResource()
        {
            if (!IsPageExists(string.Empty))
            {
                var page = new Pages {PageName = string.Empty, Body = WikiUCResource.MainPage_DefaultBody};
                return page;
            }
            return null;//Page already exists in DB
        }

        private Pages GetHelpPageFromResource()
        {
            if (!IsPageExists(WikiUCResource.HelpPageCaption))
            {
                var page = new Pages {PageName = WikiUCResource.HelpPageCaption, Body = WikiUCResource.HelpPage_DefaultBody};
                return page;
            }
            return null;//Page already exists in DB
        }

        //Append this result ot every set of pages
        private IEnumerable<Pages> GetResourceTemplatePages()
        {
            return new List<Pages> {GetMainPageFromResource(), GetHelpPageFromResource()}.Where(x=>x!=null);
        }




		public bool IsFileExists(string pageName)
		{

			Files file = this.FilesGetByName(pageName);
			if (file != null)
				return true;
			return false;
		}

		public bool IsPageExists(string pageName)
		{
            Pages page = this.PagesGetByNameInternal(pageName);
			if (page != null)
				return true;
			return false;
		}

		#endregion

		#region Pages

		public List<Pages> PagesGetAllWithBody()
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.CommandText = cmd_pagesGetAllWithBody;
            return GetPagesList(cmd.ExecuteReader());
		}

		public List<Pages> PagesGetAll()
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.CommandText = cmd_pagesGetAll;
            return GetPagesList(cmd.ExecuteReader());
		}

		public List<Pages> PagesGetByList(List<string> existingLinks)
		{
			for (int i = 0; i < existingLinks.Count; i++)
			{
				existingLinks[i] = existingLinks[i].Replace("'", "''").Replace(@"\", @"\\");
			}

			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.CommandText = string.Format(cmd_pagesGetByList, string.Join("', '", existingLinks.ToArray()));
            return ApplyReaderList<Pages>(cmd.ExecuteReader());
		}

	    private List<Pages> GetPagesList(IDataReader reader)
	    {
            var list = ApplyReaderList<Pages>(reader);
            list.AddRange(GetResourceTemplatePages());
            return list;
	    }

	    public List<Pages> PagesGetTopNew(int maxValue)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.CommandText = cmd_pagesGetTopNew + (maxValue > 0 ? string.Format(" limit {0}", maxValue) : "");
			return ApplyReaderList<Pages>(cmd.ExecuteReader());
		}

		public List<Pages> PagesGetTopNewAndUserCreated(int maxValue)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.CommandText = cmd_pagesGetTopNewAndUserCreated + (maxValue > 0 ? string.Format(" limit {0}", maxValue) : "");
			return ApplyReaderList<Pages>(cmd.ExecuteReader());
		}



		public List<Pages> PagesGetTopFresh(int maxValue)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.CommandText = cmd_pagesGetTopFresh + (maxValue > 0 ? string.Format(" limit {0}", maxValue) : "");
			return ApplyReaderList<Pages>(cmd.ExecuteReader());
		}

		public List<Pages> PagesGetByStartName(string pageName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_pagesGetByStartName;
			return ApplyReaderList<Pages>(cmd.ExecuteReader());
		}

		public List<Pages> PagesSearchByName(string pageName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_pagesSearchByName;
			return ApplyReaderList<Pages>(cmd.ExecuteReader());
		}

		private static Regex rxWords = new Regex(@"\S+", RegexOptions.Compiled | RegexOptions.CultureInvariant);


		public List<Pages> PagesSearchAllByContentEntry(string content)
		{
			if (FullTextSearch.SupportModule(FullTextSearch.WikiModule))
			{
				var pageIds = FullTextSearch
					.Search(content, FullTextSearch.WikiModule)
					.GetIdentifiers()
					.ToList();

				var query = new SqlQuery("wiki_pages p")
					.Select("p.PageName", "p.Version", "p.UserID", "p.Date", "p.Body", "p.Tenant", "h.UserID")
					.InnerJoin("wiki_pages_history h", Exp.EqColumns("p.PageName", "h.PageName") & Exp.EqColumns("p.Tenant", "h.Tenant") & Exp.Eq("h.Version", 1))
					.Where(Exp.Eq("p.Tenant", Tenant) & Exp.In("p.PageName", pageIds));

				return DbManager
					.ExecuteList(query)
					.ConvertAll(r =>
					{
						return new Pages()
						{
							PageName = (string)r[0],
							Version = Convert.ToInt32(r[1]),
							UserID = new Guid((string)r[2]),
							Date = Convert.ToDateTime(r[3]),
							Body = r[4] as string,
							Tenant = Convert.ToInt32(r[5]),
							OwnerID = new Guid((string)r[6]),
						};
					});
			}
			else
			{
				string paramsStartName = "@Content";
				string paramName;
				string[] words = (from m in rxWords.Matches(content).Cast<Match>() select m.Value).ToArray();
				IDbCommand cmd = Connection.CreateCommand(Tenant);

				for (int i = 0; i < words.Length; i++)
				{
					paramName = string.Concat(paramsStartName, i);
					cmd.Parameters.Add(cmd.CreateParameter(paramName, DbType.String));
					((IDbDataParameter)cmd.Parameters[paramName]).Value = words[i].ToLower();
				}

				cmd.CommandText = GetPagesSearchAllByContentEntry(paramsStartName, words.Length);
				return ApplyReaderList<Pages>(cmd.ExecuteReader());
			}
		}

		private string GetPagesSearchAllByContentEntry(string paramsStartName, int paramsCount)
		{
			StringBuilder sb = new StringBuilder();
			StringBuilder sbTitle = new StringBuilder();
			StringBuilder sbContent = new StringBuilder();
			StringBuilder sbWhereList = new StringBuilder();

			string paramName;
			for (int i = 0; i < paramsCount; i++)
			{
				paramName = string.Concat(paramsStartName, i);
				if (i != 0)
				{
					sbTitle.AppendLine("union all");
					sbContent.AppendLine("union all");
				}

				sbTitle.AppendFormat(@"select wp.PageName, {0} as word, wp.Tenant from wiki_pages wp
		                             where lower(wp.PageName) regexp {1}",
									 paramName, GetSqlConcat("'(^|[^:alnum:])'", paramName, "'.*'"));

				//where lower(wp.Body) like {1} or lower(wp.Body) like {2} or lower(wp.Body) like {3}",
				sbContent.AppendFormat(@"select wp.PageName, {0} as word, wp.Tenant from wiki_pages wp
                                     where lower(wp.Body) regexp {1}",
									 paramName, GetSqlConcat("'(^|[^:alnum:])'", paramName, "'.*'"));

				sbTitle.AppendLine(string.Empty);
				sbContent.AppendLine(string.Empty);

				sbWhereList.AppendFormat(@" and words like {0}", GetSqlConcat("'%'", paramName, "'%'"));
			}

			sb.AppendFormat(@"select wp.*, wph.UserID as OwnerID from wiki_pages wp
                            inner join wiki_pages_history wph on wp.PageName = wph.PageName and wp.Tenant = wph.Tenant and wph.Version = 1 
                            inner join
                                    (select PageName, GROUP_CONCAT(word) as words, Tenant from 
                                        (
                                            {0}
                                            union all
                                            {1}
                                        ) as pages
                                     group by PageName, Tenant
                                 ) as pagesSearch on wp.PageName = pagesSearch.PageName and wp.Tenant = pagesSearch.Tenant
                                 
                             where wp.Tenant = @Tenant {2}
                             ", sbTitle, sbContent, sbWhereList);
			return sb.ToString();
		}

		private string GetSqlConcat(params string[] str)
		{
			string separator = IsSQLiteDb ? "||" : ", ";
			string result = string.Join(separator, str);

			if (!IsSQLiteDb)
			{
				result = string.Format("concat({0})", result);
			}

			result = string.Format(" {0} ", result);

			return result;
		}

		public List<Pages> PagesGetAllByCategoryName(string categoryName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@CategoryName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@CategoryName"]).Value = categoryName;
			cmd.CommandText = cmd_pagesGetAllByCategoryName;
			return ApplyReaderList<Pages>(cmd.ExecuteReader());

		}

        public Pages PagesGetByName(string pageName)
        {
            var page = PagesGetByNameInternal(pageName);
            if (page==null)
            {
                if (pageName==string.Empty)
                {
                    //Main page in resources
                    page = new Pages();
                    page.PageName = string.Empty;
                    page.Body = WikiUCResource.MainPage_DefaultBody;
                }
                else if (pageName == WikiUCResource.HelpPageCaption)
                {
                    //Help page in resource
                    page = new Pages();
                    page.PageName = WikiUCResource.HelpPageCaption;
                    page.Body = WikiUCResource.HelpPage_DefaultBody;
                }
            }
            return page;
        }

		private Pages PagesGetByNameInternal(string pageName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_pagesGetByName;
			return ApplyReaderItem<Pages>(cmd.ExecuteReader());
		}

		public List<Pages> PagesGetByCreatedUserId(Guid userID)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@UserID", DbType.String));
			((IDbDataParameter)cmd.Parameters["@UserID"]).Value = userID.ToString();
			cmd.CommandText = cmd_pagesGetByCreatedUserId;
			return ApplyReaderList<Pages>(cmd.ExecuteReader());
		}

		public int PagesGetCountByCreatedUserId(Guid userID)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@UserID", DbType.String));
			((IDbDataParameter)cmd.Parameters["@UserID"]).Value = userID.ToString();
			cmd.CommandText = cmd_pagesGetCountByCreatedUserId;
			object result = cmd.ExecuteScalar();
			if (result == null)
				return 0;
			return Convert.ToInt32(result);
		}

		/// <summary>
		/// The function is update/insert pages. If a Page is new History will be initiate too.
		/// </summary>
		/// <param name="page"></param>
		/// <returns></returns>
		public Pages PagesSave(Pages page)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);

			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@Version", DbType.Int32));
			cmd.Parameters.Add(cmd.CreateParameter("@UserID", DbType.Guid));
			cmd.Parameters.Add(cmd.CreateParameter("@Date", DbType.DateTime));
			cmd.Parameters.Add(cmd.CreateParameter("@Body", DbType.String));

			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = page.PageName;
			((IDbDataParameter)cmd.Parameters["@Version"]).Value = page.Version;
			((IDbDataParameter)cmd.Parameters["@UserID"]).Value = page.UserID;
			((IDbDataParameter)cmd.Parameters["@Date"]).Value = TenantUtil.DateTimeToUtc(page.Date);
			((IDbDataParameter)cmd.Parameters["@Body"]).Value = page.Body;


			if (PagesGetByNameInternal(page.PageName) != null)
			{
				cmd.CommandText = cmd_pagesUpdate;
			}
			else
			{
				cmd.CommandText = cmd_pagesInsert;
				//InitHistory If Insert

				page.Version = 1;
				((IDbDataParameter)cmd.Parameters["@Version"]).Value = 1;
				PagesHistSave(page);
			}


			if (ExecuteNonQueryWithTransaction(cmd))
				return page;
			return null;
		}

		public void PagesDelete(string pageName)
		{
			PagesDelete(pageName, true);
		}

		public void PagesDelete(string pageName, bool withHistory)
		{
			PagesDelete(pageName, withHistory, true);
		}

		public void PagesDelete(string pageName, bool withHistory, bool withComments)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_pagesDelete;

			ExecuteNonQueryWithTransaction(cmd);

			if (withHistory)
			{
				PagesHistDelete(pageName);
			}

			if (withComments)
			{
				CommentsDeleteByPageName(pageName);
			}
		}

		public List<Files> FilesGetAll()
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.CommandText = cmd_filesGetAll;
			return ApplyReaderList<Files>(cmd.ExecuteReader());
		}

		public List<Files> FilesGetByList(List<string> existingLinks)
		{
			for (int i = 0; i < existingLinks.Count; i++)
			{
				existingLinks[i] = existingLinks[i].Replace("'", "''");
			}

			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.CommandText = string.Format(cmd_filesGetByList, string.Join("', '", existingLinks.ToArray()));
			return ApplyReaderList<Files>(cmd.ExecuteReader());
		}

		public List<Files> FilesGetByStartName(string fileName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@FileName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@FileName"]).Value = fileName;
			cmd.CommandText = cmd_filesGetByStartName;
			return ApplyReaderList<Files>(cmd.ExecuteReader());
		}

		public Files FilesGetByName(string fileName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@FileName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@FileName"]).Value = fileName;
			cmd.CommandText = cmd_filesGetByName;
			return ApplyReaderItem<Files>(cmd.ExecuteReader());
		}

		public List<Files> FilesGetByUploadName(string uploadFileName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@UploadFileName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@UploadFileName"]).Value = uploadFileName;
			cmd.CommandText = cmd_filesGetByUploadName;
			return ApplyReaderList<Files>(cmd.ExecuteReader());
		}

		/// <summary>
		/// The function is update/insert files. If a File is new History will be initiate too.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public Files FilesSave(Files file)
		{
			//bool samilarFiles;
			string FileName = string.Empty;

			bool initHistory = false;

			IDbCommand cmd = Connection.CreateCommand(Tenant);
			bool isFileExists = !string.IsNullOrEmpty(file.FileName) && IsFileExists(file.FileName);

			if (string.IsNullOrEmpty(file.FileName) || isFileExists)
			{
				/*
				  samilarFiles = FilesGetByName(file.FileName) != null;// FilesGetByUploadName(file.UploadFileName).Count;
								 if (samilarFiles)
								 {
									 if (isFileExists)
									 {
										 FileName = file.FileName;
										 cmd.CommandText = cmd_filesUpdate;
									 }
									 else
									 {
										 string ext = file.UploadFileName.Split('.')[file.UploadFileName.Split('.').Length - 1];
										 string sFile = file.UploadFileName.Remove(file.UploadFileName.Length - ext.Length - 1);
                
										 if (!string.IsNullOrEmpty(sFile))
										 {
											 FileName = string.Format(@"{0}({1}).{2}", sFile, samilarFiles, ext);
										 }
										 else
										 {
											 FileName = string.Format(@"{0}({1})", file.UploadFileName, samilarFiles);
										 }
                
										 cmd.CommandText = cmd_filesInsert;
										 initHistory = true;
									 }
								 }
								 else
								 {*/

				//FileName = file.UploadFileName;
				file.FileName = file.UploadFileName;
				file.Version = 1;
				if (isFileExists)
				{
					cmd.CommandText = cmd_filesUpdate;
				}
				else
				{
					cmd.CommandText = cmd_filesInsert;
				}

				initHistory = true;
				//}

				//file.FileName = FileName;

			}
			else
			{
				cmd.CommandText = cmd_filesInsert;
				initHistory = true;
			}

			cmd.Parameters.Add(cmd.CreateParameter("@FileName", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@UploadFileName", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@FileSize", DbType.Int32));
			cmd.Parameters.Add(cmd.CreateParameter("@Version", DbType.Int32));
			cmd.Parameters.Add(cmd.CreateParameter("@UserID", DbType.Guid));
			cmd.Parameters.Add(cmd.CreateParameter("@Date", DbType.DateTime));
			cmd.Parameters.Add(cmd.CreateParameter("@FileLocation", DbType.String));

			((IDbDataParameter)cmd.Parameters["@FileName"]).Value = file.FileName;
			((IDbDataParameter)cmd.Parameters["@UploadFileName"]).Value = file.UploadFileName;
			((IDbDataParameter)cmd.Parameters["@FileSize"]).Value = file.FileSize;
			((IDbDataParameter)cmd.Parameters["@Version"]).Value = file.Version;
			((IDbDataParameter)cmd.Parameters["@UserID"]).Value = file.UserID;
			((IDbDataParameter)cmd.Parameters["@Date"]).Value = TenantUtil.DateTimeToUtc(file.Date);
			((IDbDataParameter)cmd.Parameters["@FileLocation"]).Value = file.FileLocation;


			if (initHistory)
			{
				file.Version = 1;
				((IDbDataParameter)cmd.Parameters["@Version"]).Value = 1;
				FilesHistSave(file);
			}

			if (ExecuteNonQueryWithTransaction(cmd))
				return file;
			return null;
		}

		public void FilesDelete(string fileName)
		{
			FilesDelete(fileName, true);
		}

		public void FilesDelete(string fileName, bool withHistory)
		{
			FilesDelete(fileName, withHistory, true);
		}

		public void FilesDelete(string fileName, bool withHistory, bool withComments)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@FileName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@FileName"]).Value = fileName;
			cmd.CommandText = cmd_filesDelete;

			ExecuteNonQueryWithTransaction(cmd);

			if (withHistory)
			{
				FilesHistDelete(fileName);
			}

			if (withComments)
			{
				CommentsDeleteByPageName(fileName);
			}
		}

		#endregion

		#region History Functions
		public List<Pages> PagesHistGetAll()
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.CommandText = cmd_pagesHistGetAll;
			return ApplyReaderList<Pages>(cmd.ExecuteReader());
		}

		public List<Pages> PagesHistGetAllByName(string pageName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_pagesHistGetAllByName;
			return ApplyReaderList<Pages>(cmd.ExecuteReader());
		}

		public Pages PagesHistGetByNameVersion(string pageName, int version)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@Version", DbType.Int32));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			((IDbDataParameter)cmd.Parameters["@Version"]).Value = version;
			cmd.CommandText = cmd_pagesHistGetByNameVersion;
			return ApplyReaderItem<Pages>(cmd.ExecuteReader());
		}

		public Pages PagesHistSave(Pages page)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);

			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@Version", DbType.Int32));
			cmd.Parameters.Add(cmd.CreateParameter("@UserID", DbType.Guid));
			cmd.Parameters.Add(cmd.CreateParameter("@Date", DbType.DateTime));
			cmd.Parameters.Add(cmd.CreateParameter("@Body", DbType.String));



			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = page.PageName;
			((IDbDataParameter)cmd.Parameters["@Version"]).Value = page.Version;
			((IDbDataParameter)cmd.Parameters["@UserID"]).Value = page.UserID;
			((IDbDataParameter)cmd.Parameters["@Date"]).Value = TenantUtil.DateTimeToUtc(page.Date);
			((IDbDataParameter)cmd.Parameters["@Body"]).Value = page.Body;

			cmd.CommandText = cmd_pagesHistInsert;

			if (ExecuteNonQueryWithTransaction(cmd))
				return page;
			return null;
		}


		public void PagesHistDelete(string pageName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_pagesHistDelete;

			ExecuteNonQueryWithTransaction(cmd);

		}

		public int PagesHistGetMaxVersion(string pageName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_pagesHistMaxVersion;
			object result = cmd.ExecuteScalar();
			if (result == null || result==DBNull.Value)
				return 0;
			return Convert.ToInt32(result);
		}


		public List<Files> FilesHistGetAll()
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.CommandText = cmd_filesHistGetAll;
			return ApplyReaderList<Files>(cmd.ExecuteReader());
		}

		public List<Files> FilesHistGetAllByName(string fileName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@FileName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@FileName"]).Value = fileName;
			cmd.CommandText = cmd_filesHistGetAllByName;
			return ApplyReaderList<Files>(cmd.ExecuteReader());
		}

		public Files FilesHistGetByNameVersion(string fileName, int version)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@FileName", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@Version", DbType.Int32));
			((IDbDataParameter)cmd.Parameters["@FileName"]).Value = fileName;
			((IDbDataParameter)cmd.Parameters["@Version"]).Value = version;
			cmd.CommandText = cmd_filesHistGetByNameVersion;
			return ApplyReaderItem<Files>(cmd.ExecuteReader());
		}

		public Files FilesHistSave(Files file)
		{
			string FileName = string.Empty;

			FilesHistDelete(file.FileName);

			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@FileName", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@UploadFileName", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@Version", DbType.Int32));
			cmd.Parameters.Add(cmd.CreateParameter("@FileSize", DbType.Int32));
			cmd.Parameters.Add(cmd.CreateParameter("@UserID", DbType.Guid));
			cmd.Parameters.Add(cmd.CreateParameter("@Date", DbType.DateTime));
			cmd.Parameters.Add(cmd.CreateParameter("@FileLocation", DbType.String));

			((IDbDataParameter)cmd.Parameters["@FileName"]).Value = file.FileName;
			((IDbDataParameter)cmd.Parameters["@UploadFileName"]).Value = file.UploadFileName;
			((IDbDataParameter)cmd.Parameters["@Version"]).Value = file.Version;
			((IDbDataParameter)cmd.Parameters["@FileSize"]).Value = file.FileSize;
			((IDbDataParameter)cmd.Parameters["@UserID"]).Value = file.UserID;
			((IDbDataParameter)cmd.Parameters["@Date"]).Value = TenantUtil.DateTimeToUtc(file.Date);
			((IDbDataParameter)cmd.Parameters["@FileLocation"]).Value = file.FileLocation;

			cmd.CommandText = cmd_filesHistInsert;

			if (ExecuteNonQueryWithTransaction(cmd))
				return file;
			return null;
		}



		public void FilesHistDelete(string fileName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@FileName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@FileName"]).Value = fileName;
			cmd.CommandText = cmd_filesHistDelete;

			ExecuteNonQueryWithTransaction(cmd);
		}

		public int FilesHistGetMaxVersion(string fileName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = fileName;
			cmd.CommandText = cmd_filesHistMaxVersion;
			object result = cmd.ExecuteScalar();
			if (result == null)
				return 0;
			return Convert.ToInt32(result);
		}

		#endregion

		#region Comments
		public List<WikiComments> CommentsGetAllByPageName(string pageName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_commentsGetAllByPageName;
			return ApplyReaderList<WikiComments>(cmd.ExecuteReader());
		}

		public List<WikiComments> CommentsGetTopByPageName(string pageName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_commentsGetTopByPageName;
			return ApplyReaderList<WikiComments>(cmd.ExecuteReader());
		}

		public List<WikiComments> CommentsGetTopByParentId(Guid ParentId)
		{
			if (ParentId.Equals(Guid.Empty))
				return new List<WikiComments>();

			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@ParentId", DbType.String));
			((IDbDataParameter)cmd.Parameters["@ParentId"]).Value = ParentId.ToString();
			cmd.CommandText = cmd_commentsGetTopByParentId;
			return ApplyReaderList<WikiComments>(cmd.ExecuteReader());
		}

		public int CommentsGetCountAllByPageName(string pageName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_commentsGetCountAllByPageName;
			object result = cmd.ExecuteScalar();
			if (result == null)
				return 0;
			return Convert.ToInt32(result);
		}

		public WikiComments CommentsGetById(Guid Id)
		{
			if (Id.Equals(Guid.Empty))
				return null;
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@Id", DbType.Guid));
			((IDbDataParameter)cmd.Parameters["@Id"]).Value = Id;
			cmd.CommandText = cmd_commentsGetById;
			return ApplyReaderItem<WikiComments>(cmd.ExecuteReader());
		}

		public WikiComments CommentsSave(WikiComments comment)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@Id", DbType.Guid));
			cmd.Parameters.Add(cmd.CreateParameter("@ParentId", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@Body", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@UserId", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@Date", DbType.DateTime));
			cmd.Parameters.Add(cmd.CreateParameter("@Inactive", DbType.Int32));

			((IDbDataParameter)cmd.Parameters["@Id"]).Value = comment.Id;
			((IDbDataParameter)cmd.Parameters["@ParentId"]).Value = comment.ParentId.ToString();
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = comment.PageName;
			((IDbDataParameter)cmd.Parameters["@Body"]).Value = comment.Body;
			((IDbDataParameter)cmd.Parameters["@UserId"]).Value = comment.UserId.ToString();
			((IDbDataParameter)cmd.Parameters["@Date"]).Value = TenantUtil.DateTimeToUtc(comment.Date);
			((IDbDataParameter)cmd.Parameters["@Inactive"]).Value = (comment.Inactive ? 1 : 0);

			if (CommentsGetById(comment.Id) == null)
			{
				cmd.CommandText = cmd_commentsInsert;
			}
			else
			{
				cmd.CommandText = cmd_commentsUpdate;
			}

			if (ExecuteNonQueryWithTransaction(cmd))
				return comment;

			return null;

		}

		/// <summary>
		/// Requrcive function.
		/// </summary>
		/// <param name="Id"></param>
		public void CommentsDeleteById(Guid Id)
		{
			foreach (WikiComments comment in CommentsGetTopByParentId(Id))
			{
				CommentsDeleteById(comment.Id);
			}
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@Id", DbType.Guid));
			((IDbDataParameter)cmd.Parameters["@Id"]).Value = Id;
			cmd.CommandText = cmd_commentsDeleteById;

			ExecuteNonQueryWithTransaction(cmd);
		}


		/// <summary>
		/// Important: Use the function with accurate!!!
		/// </summary>
		/// <param name="ParentId"></param>
		public void CommentsDeleteByParentId(Guid ParentId)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@ParentId", DbType.String));
			((IDbDataParameter)cmd.Parameters["@ParentId"]).Value = ParentId.ToString();
			cmd.CommandText = cmd_commentsDeleteByParentId;

			ExecuteNonQueryWithTransaction(cmd);
		}

		public void CommentsDeleteByPageName(string pageName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_commentsDeleteByPageName;

			ExecuteNonQueryWithTransaction(cmd);
		}




		#endregion

		#region Categories

		public List<Categories> CategoriesGetAll()
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.CommandText = cmd_categoriesGetAll;
			return ApplyReaderList<Categories>(cmd.ExecuteReader());
		}


		public List<Categories> CategoriesGetAllByPageName(string pageName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_categoriesGetAllByPageName;
			return ApplyReaderList<Categories>(cmd.ExecuteReader());
		}

		public List<Categories> CategoriesGetAllByCategoryName(string categoryName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@CategoryName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@CategoryName"]).Value = categoryName;
			cmd.CommandText = cmd_categoriesGetAllByCategoryName;
			return ApplyReaderList<Categories>(cmd.ExecuteReader());
		}

		public Categories CategoriesGetByPageCategoryName(string pageName, string categoryName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@CategoryName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			((IDbDataParameter)cmd.Parameters["@CategoryName"]).Value = categoryName;
			cmd.CommandText = cmd_categoriesGetByPageCategoryName;
			return ApplyReaderItem<Categories>(cmd.ExecuteReader());
		}

		public int CategoriesGetCountAllByPageName(string pageName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_categoriesGetCountAllByPageName;

			object result = cmd.ExecuteScalar();
			if (result == null)
				return 0;
			return Convert.ToInt32(result);
		}

		public int CategoriesGetCountAllByCategoryName(string categoryName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@CategoryName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@CategoryName"]).Value = categoryName;
			cmd.CommandText = cmd_categoriesGetCountAllByCategoryName;

			object result = cmd.ExecuteScalar();
			if (result == null)
				return 0;
			return Convert.ToInt32(result);
		}

		public Categories CategorySave(string pageName, string categoryName)
		{
			Categories category = new Categories();
			category.PageName = pageName;
			category.CategoryName = categoryName;
			return CategorySave(category);
		}

		public Categories CategorySave(Categories category)
		{
			if (category.PageName == null || string.IsNullOrEmpty(category.CategoryName))
			{
				return null;
			}
			if (CategoriesGetByPageCategoryName(category.PageName, category.CategoryName) == null)
			{
				IDbCommand cmd = Connection.CreateCommand(Tenant);
				cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
				cmd.Parameters.Add(cmd.CreateParameter("@CategoryName", DbType.String));
				((IDbDataParameter)cmd.Parameters["@PageName"]).Value = category.PageName;
				((IDbDataParameter)cmd.Parameters["@CategoryName"]).Value = category.CategoryName;
				cmd.CommandText = cmd_categoriesInsert;
				ExecuteNonQueryWithTransaction(cmd);
			}

			return category;
		}


		public void CategoryDeleteAllByPageName(string pageName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_categoriesDeleteAllByPageName;
			ExecuteNonQueryWithTransaction(cmd);
		}

		public void CategoryDeleteAllByCategoryName(string categoryName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@CategoryName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@CategoryName"]).Value = categoryName;
			cmd.CommandText = cmd_categoriesDeleteAllByCategoryName;
			ExecuteNonQueryWithTransaction(cmd);
		}

		public void CategoryDeleteByPageCategoryName(string pageName, string categoryName)
		{
			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("@CategoryName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			((IDbDataParameter)cmd.Parameters["@CategoryName"]).Value = categoryName;
			cmd.CommandText = cmd_categoriesDeleteByPageCategoryName;
			ExecuteNonQueryWithTransaction(cmd);
		}

		public List<Categories> CategoriesSelectCategoriesWillBeDeletedAtAllByPageNam(string pageName)
		{

			IDbCommand cmd = Connection.CreateCommand(Tenant);
			cmd.Parameters.Add(cmd.CreateParameter("@PageName", DbType.String));
			((IDbDataParameter)cmd.Parameters["@PageName"]).Value = pageName;
			cmd.CommandText = cmd_categoriesSelectCategoriesWillBeDeletedAtAllByPageName;
			return ApplyReaderList<Categories>(cmd.ExecuteReader());
		}
		#endregion
	}
}
