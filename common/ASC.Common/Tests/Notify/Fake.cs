#if DEBUG
namespace ASC.Common.Tests.Notify
{
    using ASC.Notify.Model;
    using ASC.Notify.Patterns;
    using ASC.Notify.Recipients;

    /// <summary>
    /// Тестовые данные
    /// </summary>
    static class Fake
    {
        #region cпособы отправки
        
        #endregion
        public static readonly string sender_email = "email";
        public static readonly string sender_web = "web";
        #region действия
        
        public static readonly INotifyAction action_new_employee = new NotifyAction("1", "new employee");
        public static readonly INotifyAction action_employee_dismiss = new NotifyAction("2", "employee dismiss");
        public static readonly INotifyAction action_employee_die = new NotifyAction("3", "employee die");

        #endregion

        #region шаблоны и форматтеры
        
        public static readonly IPattern pattern_new_employee = new Pattern("1", "new employee","hi","new employee come");
        public static readonly IPattern pattern_new_employee2 = new Pattern("0001", "new employee", "hi", "new employee come");
        public static readonly IPattern pattern_new_employee3 = new Pattern("0002", "new employee", "hi", "new employee come");
        public static readonly IPattern pattern_new_employee4 = new Pattern("0003", "new employee", "hi", "new employee come");
        
        public static readonly IPatternFormatter pattern_formatter = new NullPatternFormatter(); 
        #endregion

        #region получатели

        //непосредственные получатели
        static public readonly IDirectRecipient recipient_anton = new DirectRecipient("1", "smirnov anton", new[] { "sm_anton@mail.ru" });
        static public readonly IDirectRecipient recipient_nikolay = new DirectRecipient("2", "nikolay ivanov", new[] { "nik@mail.ru" });
        static public readonly IDirectRecipient recipient_valery = new DirectRecipient("3", "valery zykov", new[] { "zvu@yandex.ru" });
        static public readonly IDirectRecipient recipient_lev = new DirectRecipient("4", "lev bannov", new[] { "lev@nctsoft.com" });

        //групповые получатели
        static public readonly IRecipientsGroup recipient_group_netdep = new RecipientsGroup("10", ".NET Department");
        static public readonly IRecipientsGroup recipient_group_activeX = new RecipientsGroup("12", "ActiveX");
        static public readonly IRecipientsGroup recipient_group_administration = new RecipientsGroup("11", "Administration");
        static public readonly IRecipientsGroup recipient_group_users = new RecipientsGroup("13", "Users");

        #endregion
    }
}
#endif