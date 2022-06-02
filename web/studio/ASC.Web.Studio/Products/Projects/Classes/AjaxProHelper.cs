#region Import



#endregion

namespace ASC.Web.Projects.Classes
{
    //using UserControls.Common;

    //[AjaxNamespace("AjaxProHelper")]
    //public class AjaxProHelper :
    //                            IGatherObject<Participant>,
    //                            ISaveOrUpdateObject<Participant>
    //{
        
    //    #region Property

    //    public Participant Inspector { get; set; }

    //    #endregion

    //    [AjaxMethod]
    //    public void SaveParticipantNote(String hashKey, String note)
    //    {

    //        CommonManagement.AttachView<GatherObjectPresentor<Participant>>(this, CommonManagement.DaoFactory.GetParticipantDao());

    //        ExecGatherObject(this, new GatherObjectEventArgs<Participant>(ASC.Core.SecurityContext.CurrentAccount.ID, true));

    //        Inspector.Notes[hashKey] =  HttpUtility.HtmlEncode(note.Trim());

    //        CommonManagement.AttachView<SaveOrUpdateObjectPresentor<Participant>>(this, CommonManagement.DaoFactory.GetParticipantDao());

    //        ExecSaveOrUpdate(this, EventArgs.Empty);
    //    }
          

    //    [AjaxMethod]
    //    public static String GetPanelAssigneesHtml()
    //    {

    //        Page page = new Page();

    //        PanelAssignees oPanelAssignees = (PanelAssignees)page.LoadControl(Constants.BaseVirtualPath +  "UserControls/Common/PanelAssignees.ascx");
                        
    //        page.Controls.Add(oPanelAssignees);

    //        System.IO.StringWriter writer = new System.IO.StringWriter();
    //        HttpContext.Current.Server.Execute(page, writer, false);

    //        string output = writer.ToString();

    //        writer.Close();

    //        return output;

    //    }

    //    /// <summary>
    //    ///   Добавляет/Удаляет определенный объект из избранного текущего пользователя
    //    /// </summary>
    //    /// <param name="favoriteType">Type объекта, который должен быть добавлен/удален</param>
    //    /// <param name="externalID">ID объекта</param>
    //    /// <param name="IsFavorite">Принадлежит или нет объект избранному</param>
    //    /// <returns>Возвращает true в случае успеха, в противном случае false.</returns>
    //    [AjaxMethod]
    //    public bool ChangeFavorite(String aliasFavoriteType, Int32 favoriteID)
    //    {

    //        String favoriteType = String.Empty;

    //        foreach (var item in CommonManagement.AliasProvider.Keys)
    //        {

    //            if (String.Compare(CommonManagement.AliasProvider[item], aliasFavoriteType, true) == 0)
    //            {
    //                favoriteType = item;

    //                break;
    //            }
    //        }

    //        IFavorite dynMyClass = Assembly.GetAssembly(typeof(Participant)).CreateInstance(favoriteType) as IFavorite;

    //        if (dynMyClass == null) return false;

    //        Type t = dynMyClass.GetType();

    //        t.BaseType.GetProperty("ID").SetValue(dynMyClass, favoriteID, null);

    //        CommonManagement.AttachView<GatherObjectPresentor<Participant>>(this, CommonManagement.DaoFactory.GetParticipantDao());

    //        ExecGatherObject(this, new GatherObjectEventArgs<Participant>(ASC.Core.SecurityContext.CurrentAccount.ID, true));

    //        IFavorite oFavorite = null;

    //        foreach (var item in Inspector.FavoriteList)
    //        {

    //            if (item.Equals(dynMyClass))
    //            {
    //                oFavorite = item;

    //                break;
    //            }
    //        }

    //        if (oFavorite != null)
    //            Inspector.RemoveFavorite(oFavorite);
    //        else
    //            Inspector.AddFavorite(dynMyClass);

    //        CommonManagement.AttachView<SaveOrUpdateObjectPresentor<Participant>>(this, CommonManagement.DaoFactory.GetParticipantDao());

    //        ExecSaveOrUpdate(this, EventArgs.Empty);

    //        return true;

    //    }
       
    //    [AjaxMethod]
    //    public  bool TrackingObject(String externalID)
    //    {

    //        INotifyAction action =  NotifyClient.GetNotifyActionByAlias(externalID.Split('_')[0]);
          
    //        bool IsSubscribed = false;
        
    //        foreach (var recipient in NotifySource.Instance.GetSubscriptionProvider().GetRecipients(action, externalID))
    //            if (String.Compare(recipient.ID, ASC.Core.SecurityContext.CurrentAccount.ID.ToString(), true) == 0)
    //                IsSubscribed = true;

    //        if (IsSubscribed)
    //            NotifySource.Instance.GetSubscriptionProvider().UnSubscribe(
    //                                                           action,
    //                                                           externalID,
    //                                                           NotifySource.Instance.GetRecipientsProvider().GetRecipient(ASC.Core.SecurityContext.CurrentAccount.ID.ToString())

    //                                                        );
    //        else
    //            NotifySource.Instance.GetSubscriptionProvider().Subscribe(
    //                                                                          action,
    //                                                                          externalID,
    //                                                                          NotifySource.Instance.GetRecipientsProvider().GetRecipient(ASC.Core.SecurityContext.CurrentAccount.ID.ToString())

    //                                                                      );

    //        return !IsSubscribed;
        
    //    }
        
    //    #region IGatherObject<Participant> Members

    //    public Participant Target
    //    {
    //        get
    //        {
    //            return Inspector;
    //        }
    //        set
    //        {
    //            Inspector = value;
    //        }
    //    }

    //    public event EventHandler<GatherObjectEventArgs<Participant>> ExecGatherObject;

    //    #endregion

    //    #region ISaveOrUpdateObject<Participant> Members

    //    public event EventHandler ExecSaveOrUpdate;

    //    #endregion
    //}
}