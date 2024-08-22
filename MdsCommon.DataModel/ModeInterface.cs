namespace MdsCommon;

public class User
{
    public string Name { get; set; }
    public string AuthType { get; set; }
}

public interface IHasUser
{
    public User User { get; set; }
}

public interface IHasValidationPanel
{
    string ValidationMessage { get; set; }
}

public interface IHasLoadingPanel
{
    public bool IsLoading { get; set; }
}

public static class Menu
{
    public class Entry
    {
        public string Code { get; set; }
        public string Label { get; set; }
        public string Href { get; set; }
        public string SvgIcon { get; set; }
    }
}

public static class ModelExtensions
{
    public static bool IsSignedIn(this User user)
    {
        if (user == null)
            return false;

        return !string.IsNullOrEmpty(user.AuthType);
    }
}