namespace Hasin.Taaghche.TaskScheduler.NotificationServices
{
    public enum Status
    {
        Successful = 0,
        InternalError = 1, //
        InvalidSession = 2,
        InputError = 3,
        LoginNeeded = 4,
        DuplicateEmail = 5, //
        BadContract = 6,
        RedirectToPayment = 7,
        ForceUpdate = 8,
        ForceLogout = 9,
        InvokeKeepUpdate = 10,
    }
}
